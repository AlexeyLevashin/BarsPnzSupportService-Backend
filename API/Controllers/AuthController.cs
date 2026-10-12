using Application.Dto.Authorization.Requests;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IPasswordResetService _passwordResetService;

    public AuthController(IAuthService authService, IPasswordResetService passwordResetService)
    {
        _authService = authService;
        _passwordResetService = passwordResetService;
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
