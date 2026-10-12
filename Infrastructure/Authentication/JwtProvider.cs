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
        var parameters = _tokenValidationParameters.Clone();
        parameters.ValidAudience = $"{_options.Audience}:password-reset";
        parameters.ClockSkew = TimeSpan.Zero;
        try
        {
            var principal = new JwtSecurityTokenHandler().ValidateToken(token, parameters, out var validatedToken);
            if (validatedToken is not JwtSecurityToken jwtToken ||
                jwtToken.Header.Alg != SecurityAlgorithms.HmacSha256 ||
                principal.FindFirst("TokenType")?.Value != "PasswordReset")
            {
                return null;
            }
            return principal;
        }
        catch
        {
            return null;
        }
    }

    public ClaimsPrincipal? ValidateToken(string refreshToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var principal = tokenHandler.ValidateToken(refreshToken, _tokenValidationParameters, out SecurityToken validatedToken);

            if (validatedToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            var tokenType = principal.FindFirst("TokenType")?.Value;
            if (tokenType != "Refresh")
            {
                return null; 
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }
}
