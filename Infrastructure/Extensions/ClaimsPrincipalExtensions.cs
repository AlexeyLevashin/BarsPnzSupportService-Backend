using System.Security.Claims;
using Domain.Enums;

namespace Infrastructure.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        return Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) 
                          ?? throw new UnauthorizedAccessException());
    }

    public static UserRole GetUserRole(this ClaimsPrincipal user)
    {
        return Enum.Parse<UserRole>(user.FindFirstValue(ClaimTypes.Role) 
                                    ?? throw new UnauthorizedAccessException());
    }
}