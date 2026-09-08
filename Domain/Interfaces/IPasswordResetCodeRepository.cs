using Domain.DbModels;

namespace Domain.Interfaces;

public interface IPasswordResetCodeRepository
{
    public Task AddAsync(DbPasswordResetCode entity);
    public Task InvalidateUnusedByUserIdAsync(Guid userId);
    public Task<DbPasswordResetCode?> GetActiveByUserIdAsync(Guid userId);
    public Task<DbPasswordResetCode?> GetByResetTokenAsync(Guid resetToken);
}