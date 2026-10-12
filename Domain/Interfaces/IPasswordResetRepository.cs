using Domain.DbModels;

namespace Domain.Interfaces;

public interface IPasswordResetRepository
{
    public Task<bool> CreateAsync(DbPasswordResetRequest request);
    public Task<DbPasswordResetRequest?> VerifyAsync(Guid userId, string codeHash);
    public Task<bool> ResetPasswordAsync(Guid requestId, Guid userId, string passwordHash);
    public Task CancelAsync(Guid requestId);
}
