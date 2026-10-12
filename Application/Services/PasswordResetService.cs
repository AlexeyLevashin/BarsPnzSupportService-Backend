using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Authentication;
using Application.Dto.Authorization.Requests;
using Application.Dto.Authorization.Responses;
using Application.Dto.EmailMessages.Requests;
using Application.Exceptions.Auth;
using Application.Interfaces;
using Application.Interfaces.Authentication;
using Application.Interfaces.Repositories;
using Domain.DbModels;
using Domain.Interfaces;
using Microsoft.Extensions.Options;
using Hangfire;

namespace Application.Services;

public class PasswordResetService : IPasswordResetService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetRepository _passwordResetRepository;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IPasswordService _passwordService;
    private readonly IJwtProvider _jwtProvider;
    private readonly JwtOptions _jwtOptions;

    public PasswordResetService(IUserRepository userRepository, IPasswordResetRepository passwordResetRepository,
        IBackgroundJobClient backgroundJobClient, IPasswordService passwordService, IJwtProvider jwtProvider, IOptions<JwtOptions> jwtOptions)
    {
        _userRepository = userRepository;
        _passwordResetRepository = passwordResetRepository;
        _backgroundJobClient = backgroundJobClient;
        _passwordService = passwordService;
        _jwtProvider = jwtProvider;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task RequestAsync(PasswordResetRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email.Trim());
        if (user is null || !user.IsActivated)
        {
            return;
        }

        var code = RandomNumberGenerator.GetInt32(1000000).ToString("D6");
        var resetRequest = new DbPasswordResetRequest
        {
            UserId = user.Id,
            CodeHash = HashCode(user.Id, code),
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };
        if (!await _passwordResetRepository.CreateAsync(resetRequest))
        {
            return;
        }

        try
        {
            var email = new EmailMessageRequest
            {
                ToEmail = user.Email,
                Subject = "Восстановление пароля",
                Message = $"<p>Код восстановления пароля: <strong>{code}</strong></p><p>Код действует 10 минут. Если вы не запрашивали восстановление, проигнорируйте письмо.</p>"
            };
            _backgroundJobClient.Enqueue<IEmailService>(x => x.SendEmailAsync(email));
        }
        catch
        {
            await _passwordResetRepository.CancelAsync(resetRequest.Id);
            throw;
        }
    }

    public async Task<VerifyPasswordResetResponse> VerifyAsync(VerifyPasswordResetRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email.Trim());
        if (user is null || !user.IsActivated)
        {
            throw new InvalidPasswordResetCodeException();
        }

        var resetRequest = await _passwordResetRepository.VerifyAsync(user.Id, HashCode(user.Id, request.Code));
        if (resetRequest is null)
        {
            throw new InvalidPasswordResetCodeException();
        }

        return new VerifyPasswordResetResponse
        {
            ResetToken = _jwtProvider.GeneratePasswordResetToken(resetRequest)
        };
    }

    public async Task ConfirmAsync(ConfirmPasswordResetRequest request)
    {
        var principal = _jwtProvider.ValidatePasswordResetToken(request.ResetToken);
        if (!Guid.TryParse(principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId) ||
            !Guid.TryParse(principal?.FindFirst("RequestId")?.Value, out var requestId))
        {
            throw new InvalidTokenException("Невалидный или просроченный токен восстановления");
        }

        if (!await _passwordResetRepository.ResetPasswordAsync(requestId, userId, _passwordService.Hash(request.NewPassword)))
        {
            throw new InvalidTokenException("Невалидный или просроченный токен восстановления");
        }
    }

    private string HashCode(Guid userId, string code)
    {
        var key = Encoding.UTF8.GetBytes(_jwtOptions.SecretKey);
        var data = Encoding.UTF8.GetBytes($"password-reset:{userId}:{code}");
        return Convert.ToHexString(HMACSHA256.HashData(key, data));
    }
}
