using Domain.DbModels;
using Domain.Enums;

namespace Domain.Interfaces;

public interface IUserRepository
{
    public Task AddAsync(DbUser dbUser);
    public void UpdateAsync(DbUser dbUser);
    public void DeleteAsync(DbUser dbUser);
    public Task<DbUser?> GetByIdAsync(Guid? userId);
    public Task<DbUser?> GetByEmailAsync(string email);
    public Task<bool> IsEmailTakenAsync(string email);
    public Task<(List<DbUser> Users, int totalCount)> GetAllAsync(int pageNumber, int pageSize, Guid? institutionId = null);
    public Task<List<DbUser>> GetByRolesAsync(List<UserRole> roles);
    public Task<bool> HasAdminAsync();
    public Task<bool> CheckUserExistByInstitutionId(Guid institutionId);
}