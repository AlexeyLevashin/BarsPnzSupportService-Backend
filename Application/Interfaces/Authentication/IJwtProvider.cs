using System.Security.Claims;
using Domain.DbModels;

namespace Application.Interfaces.Authentication;

public interface IJwtProvider
{
    public string GenerateAccessToken(DbUser user);
    public string GenerateRefreshToken(DbUser user);
    public string GeneratePasswordResetToken(DbPasswordResetRequest request);
    public ClaimsPrincipal? ValidatePasswordResetToken(string token);

    public ClaimsPrincipal? ValidateToken(string refreshToken);

}
