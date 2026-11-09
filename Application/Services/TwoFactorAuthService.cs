using Application.Dto.Authorization.Requests;
using Application.Dto.Authorization.Responses;
using Application.Authentication;
using Application.Exceptions.Abstractions;
using Application.Exceptions.Auth;
using Application.Exceptions.Users;
using Application.Interfaces;
using Application.Interfaces.Authentication;
using Domain.DbModels;
using Domain.Interfaces;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Application.Services;

public class TwoFactorAuthService : ITwoFactorService
{
    private readonly IUserRepository _userRepository;
    private readonly ITwoFactorAuthRepository _twoFactorAuthRepository;
    private readonly ITwoFactorCodeProvider _twoFactorCodeProvider;
    private readonly IPasswordService _passwordService;
    private readonly IJwtProvider _jwtProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TwoFactorOptions _options;

    public TwoFactorAuthService(IUserRepository userRepository, ITwoFactorAuthRepository twoFactorAuthRepository,
        ITwoFactorCodeProvider twoFactorCodeProvider, IPasswordService passwordService, IJwtProvider jwtProvider,
        IUnitOfWork unitOfWork, IOptions<TwoFactorOptions> twoFactorOptions)
    {
        _userRepository = userRepository;
        _twoFactorAuthRepository = twoFactorAuthRepository;
        _twoFactorCodeProvider = twoFactorCodeProvider;
        _passwordService = passwordService;
        _jwtProvider = jwtProvider;
        _unitOfWork = unitOfWork;
        _options = twoFactorOptions.Value;
    }

    public async Task<bool> IsEnabledAsync(Guid userId)
    {
        var twoFactorAuth = await _twoFactorAuthRepository.GetByUserIdAsync(userId);
        return twoFactorAuth is { IsEnabled: true };
    }

    public async Task<TwoFactorStatusResponse> GetStatusAsync(Guid userId)
    {
        return new TwoFactorStatusResponse
        {
            IsEnabled = await IsEnabledAsync(userId)
        };
    }

    public async Task<EnableTwoFactorResponse> EnableAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
        {
            throw new UserNotFoundException();
        }

        var twoFactorAuth = await _twoFactorAuthRepository.GetByUserIdAsync(userId);
        var isNewRecord = twoFactorAuth is null;

        if (twoFactorAuth is { IsEnabled: true })
        {
            throw new ConflictException("Двухфакторная аутентификация уже включена");
        }

        twoFactorAuth ??= new DbTwoFactorAuth { UserId = userId };

        // Незавершённая настройка перегенерируется: старый секрет и коды сбрасываются
        twoFactorAuth.Secret = _twoFactorCodeProvider.GenerateSecretBase32();
        twoFactorAuth.IsEnabled = false;
        twoFactorAuth.ConfirmedAt = null;
        twoFactorAuth.FailedAttempts = 0;
        twoFactorAuth.LockedUntil = null;

        if (isNewRecord)
        {
            await _twoFactorAuthRepository.AddAsync(twoFactorAuth);
        }
        else
        {
            await _twoFactorAuthRepository.RemoveUnusedRecoveryCodesAsync(twoFactorAuth.Id);
        }

        await _unitOfWork.SaveChangesAsync();

        return new EnableTwoFactorResponse
        {
            Secret = twoFactorAuth.Secret,
            OtpAuthUri = _twoFactorCodeProvider.BuildOtpAuthUri(_options.Issuer, user.Email, twoFactorAuth.Secret)
        };
    }

    public async Task<ConfirmTwoFactorResponse> ConfirmAsync(Guid userId, ConfirmTwoFactorRequest request)
    {
        var twoFactorAuth = await _twoFactorAuthRepository.GetByUserIdWithRecoveryCodesAsync(userId);

        // Подтверждение работает с pending-записью от EnableAsync
        if (twoFactorAuth is null)
        {
            throw new BadRequestException("Сначала запросите настройку двухфакторной аутентификации");
        }

        if (twoFactorAuth.IsEnabled)
        {
            throw new ConflictException("Двухфакторная аутентификация уже включена");
        }

        if (twoFactorAuth.LockedUntil is { } lockedUntil && lockedUntil > DateTime.UtcNow)
        {
            throw new ConflictException("Слишком много неверных попыток; попробуйте позже");
        }

        if (!await ConsumeAuthCodeAsync(twoFactorAuth, request.Code))
        {
            RegisterFailedAttempt(twoFactorAuth);
            await _unitOfWork.SaveChangesAsync();
            throw new BadRequestException(GetInvalidCodeMessage(twoFactorAuth));
        }

        twoFactorAuth.IsEnabled = true;
        twoFactorAuth.ConfirmedAt = DateTime.UtcNow;
        twoFactorAuth.FailedAttempts = 0;

        var recoveryCodes = GenerateRecoveryCodes();
        await _twoFactorAuthRepository.AddRecoveryCodesAsync(recoveryCodes
            .Select(code => new DbTwoFactorRecoveryCode
            {
                TwoFactorAuthId = twoFactorAuth.Id,
                CodeHash = _passwordService.Hash(code)
            }).ToList());

        await _unitOfWork.SaveChangesAsync();

        return new ConfirmTwoFactorResponse { RecoveryCodes = recoveryCodes };
    }

    public async Task<LoginSuccessResponse> VerifyAsync(VerifyTwoFactorRequest request)
    {
        var principal = _jwtProvider.ValidateTwoFactorToken(request.TwoFactorToken);

        var userIdString = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (principal is null ||
            string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            throw new InvalidTokenException("Невалидный токен подтверждения");
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null || !user.IsActivated)
        {
            throw new InvalidTokenException("Токен относится к недействующему аккаунту");
        }

        var twoFactorAuth = await _twoFactorAuthRepository.GetByUserIdWithRecoveryCodesAsync(userId);
        if (twoFactorAuth is not { IsEnabled: true })
        {
            throw new BadRequestException("Двухфакторная аутентификация не включена");
        }

        if (twoFactorAuth.LockedUntil is { } lockedUntil && lockedUntil > DateTime.UtcNow)
        {
            throw new ConflictException("Слишком много неверных попыток; попробуйте позже");
        }

        if (!await ConsumeAuthCodeAsync(twoFactorAuth, request.Code))
        {
            RegisterFailedAttempt(twoFactorAuth);
            await _unitOfWork.SaveChangesAsync();
            throw new BadRequestException(GetInvalidCodeMessage(twoFactorAuth));
        }

        twoFactorAuth.FailedAttempts = 0;

        await _unitOfWork.SaveChangesAsync();

        return new LoginSuccessResponse
        {
            AccessToken = _jwtProvider.GenerateAccessToken(user),
            RefreshToken = _jwtProvider.GenerateRefreshToken(user)
        };
    }

    public async Task DisableAsync(Guid userId, ConfirmTwoFactorRequest request)
    {
        var twoFactorAuth = await RequireEnabledTwoFactorAsync(userId);

        if (!await ConsumeAuthCodeAsync(twoFactorAuth, request.Code))
        {
            RegisterFailedAttempt(twoFactorAuth);
            await _unitOfWork.SaveChangesAsync();
            throw new BadRequestException(GetInvalidCodeMessage(twoFactorAuth));
        }

        twoFactorAuth.IsEnabled = false;
        twoFactorAuth.FailedAttempts = 0;

        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<DbTwoFactorAuth> RequireEnabledTwoFactorAsync(Guid userId)
    {
        var twoFactorAuth = await _twoFactorAuthRepository.GetByUserIdWithRecoveryCodesAsync(userId);

        if (twoFactorAuth is null)
        {
            throw new BadRequestException("Двухфакторная аутентификация не настроена");
        }

        if (!twoFactorAuth.IsEnabled)
        {
            throw new BadRequestException("Двухфакторная аутентификация не включена");
        }

        if (twoFactorAuth.LockedUntil is { } lockedUntil && lockedUntil > DateTime.UtcNow)
        {
            throw new ConflictException("Слишком много неверных попыток; попробуйте позже");
        }

        return twoFactorAuth;
    }

    // Сверяет код: сначала TOTP, затем незанятые коды восстановления
    private async Task<bool> ConsumeAuthCodeAsync(DbTwoFactorAuth twoFactorAuth, string code)
    {
        if (_twoFactorCodeProvider.VerifyCode(twoFactorAuth.Secret, code, twoFactorAuth.LastUsedStep, out var usedStep))
        {
            // Параллельный запрос мог погасить тот же шаг
            if (!await _twoFactorAuthRepository.TryConsumeStepAsync(twoFactorAuth.Id, usedStep))
            {
                throw new ConflictException("Код уже использован");
            }

            return true;
        }

        foreach (var recoveryCode in twoFactorAuth.RecoveryCodes.Where(c => !c.IsUsed))
        {
            if (_passwordService.Verify(code, recoveryCode.CodeHash))
            {
                recoveryCode.IsUsed = true;
                return true;
            }
        }

        return false;
    }

    private void RegisterFailedAttempt(DbTwoFactorAuth twoFactorAuth)
    {
        twoFactorAuth.FailedAttempts++;

        if (twoFactorAuth.FailedAttempts >= Math.Max(1, _options.MaxFailedAttempts))
        {
            twoFactorAuth.LockedUntil = DateTime.UtcNow.AddMinutes(Math.Max(1, _options.LockoutMinutes));
        }
    }

    private string GetInvalidCodeMessage(DbTwoFactorAuth twoFactorAuth)
    {
        return twoFactorAuth.FailedAttempts >= Math.Max(1, _options.MaxFailedAttempts)
            ? "Неверный код; попытки исчерпаны, попробуйте позже"
            : "Неверный код";
    }

    private List<string> GenerateRecoveryCodes()
    {
        var recoveryCodesCount = Math.Max(1, _options.RecoveryCodesCount);
        var codes = new List<string>(recoveryCodesCount);

        for (var index = 0; index < recoveryCodesCount; index++)
        {
            codes.Add(_twoFactorCodeProvider.GenerateRecoveryCode());
        }

        return codes;
    }
}
