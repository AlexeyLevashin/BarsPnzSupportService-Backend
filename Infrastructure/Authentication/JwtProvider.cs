using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.InteropServices.JavaScript;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Authentication;
using Application.Interfaces.Authentication;
using Domain.DbModels;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Authentication;

public class JwtProvider : IJwtProvider
{
    private readonly JwtOptions _options;
    private readonly TokenValidationParameters _tokenValidationParameters;

    public JwtProvider(IOptions<JwtOptions> options, TokenValidationParameters tokenValidationParameters)
    {
        _options = options.Value;
        _tokenValidationParameters = tokenValidationParameters;
    }
    
    public string GenerateAccessToken(DbUser user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("employeeId", user.Employee.Id.ToString())
        };
        
        if (user.Employee?.EmployeeInstitutions != null && user.Employee.EmployeeInstitutions.Any())
        {
            foreach (var ei in user.Employee.EmployeeInstitutions)
            {
                claims.Add(new Claim("InstitutionId", ei.InstitutionId.ToString()));
            }
        }
        
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_options.AccessTokenExpiresMinutes),
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken(DbUser user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("TokenType", "Refresh")
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(_options.RefreshTokenExpiresDays),
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    public string GeneratePasswordResetToken(DbPasswordResetRequest request)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, request.UserId.ToString()),
                new Claim("RequestId", request.Id.ToString()),
                new Claim("TokenType", "PasswordReset")
            }),
            Expires = request.ExpiresAt,
            Issuer = _options.Issuer,
            Audience = $"{_options.Audience}:password-reset",
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey)), SecurityAlgorithms.HmacSha256)
        });
        return tokenHandler.WriteToken(token);
    }

    public ClaimsPrincipal? ValidatePasswordResetToken(string token)
    {
        return ValidateSpecialToken(token, $"{_options.Audience}:password-reset", "PasswordReset", TimeSpan.Zero);
    }

    public ClaimsPrincipal? ValidateToken(string refreshToken)
    {
        return ValidateSpecialToken(refreshToken, _options.Audience, "Refresh", _tokenValidationParameters.ClockSkew);
    }

    public string GenerateTwoFactorToken(DbUser user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("TokenType", "TwoFactor")
            }),
            Expires = DateTime.UtcNow.AddMinutes(_options.TwoFactorTokenExpiresMinutes),
            Issuer = _options.Issuer,
            Audience = $"{_options.Audience}:two-factor",
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey)),
                    SecurityAlgorithms.HmacSha256)
        });
        return tokenHandler.WriteToken(token);
    }

    public ClaimsPrincipal? ValidateTwoFactorToken(string token)
    {
        return ValidateSpecialToken(token, $"{_options.Audience}:two-factor", "TwoFactor", TimeSpan.Zero);
    }

    private ClaimsPrincipal? ValidateSpecialToken(string token, string audience, string tokenType, TimeSpan clockSkew)
    {
        var parameters = _tokenValidationParameters.Clone();
        parameters.ValidAudience = audience;
        parameters.ClockSkew = clockSkew;
        try
        {
            var principal = new JwtSecurityTokenHandler().ValidateToken(token, parameters, out var validatedToken);
            return validatedToken is JwtSecurityToken jwtToken &&
                   jwtToken.Header.Alg == SecurityAlgorithms.HmacSha256 &&
                   principal.FindFirst("TokenType")?.Value == tokenType
                ? principal
                : null;
        }
        catch
        {
            return null;
        }
    }
}
