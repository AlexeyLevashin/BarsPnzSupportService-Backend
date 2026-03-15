using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Authentication;
using Application.Dto.Authorization.Requests;
using Application.Dto.Authorization.Responses;
using Application.Exceptions.Auth;
using Application.Exceptions.Institutions;
using Application.Exceptions.Requests;
using Application.Exceptions.Users;
using Application.Interfaces;
using Application.Interfaces.Authentication;
using Domain.DbModels;
using Domain.Enums;
using Domain.Interfaces;
using Mapster;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IInstitutionRepository _institutionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordService _passwordService;
    private readonly IJwtProvider _jwtProvider;

    public AuthService(IUserRepository userRepository, IInstitutionRepository institutionRepository, IUnitOfWork unitOfWork, IPasswordService passwordService, IJwtProvider jwtProvider)
    {
        _userRepository = userRepository;
        _institutionRepository = institutionRepository;
        _unitOfWork = unitOfWork;
        _passwordService = passwordService;
        _jwtProvider = jwtProvider;
    }

    public async Task<LoginSuccessResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user is null || !_passwordService.Verify(request.Password, user.PasswordHash))
        {
            throw new FailureAuthorizationException();
        }

        var accessToken = _jwtProvider.GenerateAccessToken(user);
        var refreshToken = _jwtProvider.GenerateRefreshToken(user);
        
        return new LoginSuccessResponse
        {
            AccessToken = accessToken, RefreshToken = refreshToken
        };
    }

    public async Task<LoginSuccessResponse> RegisterAsync(RegisterRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user is not null)
        {
            throw new UserWithEmailIsAlreadyExistException();
        }

        if (request.InstitutionId != null)
        {
            var institution = await _institutionRepository.GetByIdAsync((Guid)request.InstitutionId);
            if (institution is null)
            {
                throw new InstitutionNotFoundException();
            }
        }

        var newUser = request.Adapt<DbUser>();
        newUser.PasswordHash = _passwordService.Hash(request.Password);
        newUser.Role = UserRole.User; 
        await _userRepository.AddAsync(newUser);
        await _unitOfWork.SaveChangesAsync();

        var accessToken = _jwtProvider.GenerateAccessToken(newUser);
        var refreshToken = _jwtProvider.GenerateRefreshToken(newUser);
        
        return new LoginSuccessResponse
        {
            AccessToken = accessToken, RefreshToken = refreshToken
        };
    }

    public async Task<LoginSuccessResponse> RefreshTokenAsync(RefreshRequest request)
    {
        var principal = _jwtProvider.ValidateToken(request.RefreshToken);
        if (principal is null)
        {
            throw new InvalidTokenException();
        }

        var userIdString = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
        {
            throw new InvalidUserIdInTokenException();
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
        {
            throw new UserNotFoundException();
        }

        var newAccessToken = _jwtProvider.GenerateAccessToken(user);
        var newRefreshToken = _jwtProvider.GenerateRefreshToken(user);

        return new LoginSuccessResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        };
    }
}