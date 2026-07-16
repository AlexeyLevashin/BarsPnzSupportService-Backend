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
    
    public static List<Guid> GetInstitutionIds(this ClaimsPrincipal user)
    {
        return user.FindAll("InstitutionId")
            .Select(c => Guid.TryParse(c.Value, out var id) ? id : Guid.Empty)
            .Where(id => id != Guid.Empty)
            .ToList();
    }
    
    public static Guid GetEmployeeId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst("employeeId");
        
        if (claim == null || !Guid.TryParse(claim.Value, out var employeeId))
        {
            throw new UnauthorizedAccessException("Токен не содержит ID сотрудника"); 
        }
        
        return employeeId;
    }
}