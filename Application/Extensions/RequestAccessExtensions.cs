using Domain.DbModels;
using Domain.Enums;

namespace Application.Extensions;

public static class RequestAccessExtensions
{
    public static bool CanBeReadBy(this DbRequest request, Guid userId, UserRole userRole)
    {
        return userRole == UserRole.Operator || userRole == UserRole.SuperAdmin ||
               request.ClientId == userId && (userRole == UserRole.User || userRole == UserRole.UserAdmin);
    }
}
