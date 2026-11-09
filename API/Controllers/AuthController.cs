using Application.Dto.Authorization.Requests;
using Application.Interfaces;
using Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IPasswordResetService _passwordResetService;
    private readonly ITwoFactorService _twoFactorService;

    public AuthController(IAuthService authService, IPasswordResetService passwordResetService, ITwoFactorService twoFactorService)
    {
        _authService = authService;
        _passwordResetService = passwordResetService;
        _twoFactorService = twoFactorService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        return Ok(await _authService.RegisterAsync(request));
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        return Ok(await _authService.LoginAsync(request));
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken(RefreshRequest request)
    {
        return Ok(await _authService.RefreshTokenAsync(request));
    }

    [HttpPost("2fa/enable")]
    [Authorize]
    public async Task<IActionResult> EnableTwoFactor()
    {
        return Ok(await _twoFactorService.EnableAsync(User.GetUserId()));
    }

    [HttpGet("2fa/status")]
    [Authorize]
    public async Task<IActionResult> TwoFactorStatus()
    {
        return Ok(await _twoFactorService.GetStatusAsync(User.GetUserId()));
    }

    [HttpPost("2fa/confirm")]
    [Authorize]
    public async Task<IActionResult> ConfirmTwoFactor(ConfirmTwoFactorRequest request)
    {
        return Ok(await _twoFactorService.ConfirmAsync(User.GetUserId(), request));
    }

    [HttpPost("2fa/disable")]
    [Authorize]
    public async Task<IActionResult> DisableTwoFactor(ConfirmTwoFactorRequest request)
    {
        await _twoFactorService.DisableAsync(User.GetUserId(), request);
        return Ok();
    }

    [HttpPost("2fa/verify")]
    [EnableRateLimiting("TwoFactor")]
    public async Task<IActionResult> VerifyTwoFactor(VerifyTwoFactorRequest request)
    {
        return Ok(await _twoFactorService.VerifyAsync(request));
    }

    [HttpPost("password-reset/request")]
    [EnableRateLimiting("PasswordReset")]
    public async Task<IActionResult> RequestPasswordReset(PasswordResetRequest request)
    {
        await _passwordResetService.RequestAsync(request);
        return Ok();
    }

    [HttpPost("password-reset/verify")]
    [EnableRateLimiting("PasswordReset")]
    public async Task<IActionResult> VerifyPasswordReset(VerifyPasswordResetRequest request)
    {
        return Ok(await _passwordResetService.VerifyAsync(request));
    }

    [HttpPost("password-reset/confirm")]
    [EnableRateLimiting("PasswordReset")]
    public async Task<IActionResult> ConfirmPasswordReset(ConfirmPasswordResetRequest request)
    {
        await _passwordResetService.ConfirmAsync(request);
        return Ok();
    }
}
