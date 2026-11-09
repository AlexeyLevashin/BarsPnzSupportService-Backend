using Domain.DbModels;

namespace Domain.Interfaces;

public interface ITwoFactorAuthRepository
{
    public Task AddAsync(DbTwoFactorAuth twoFactorAuth);
    public Task<DbTwoFactorAuth?> GetByUserIdAsync(Guid userId);
    public Task<DbTwoFactorAuth?> GetByUserIdWithRecoveryCodesAsync(Guid userId);
    public Task AddRecoveryCodesAsync(List<DbTwoFactorRecoveryCode> recoveryCodes);
    public Task RemoveUnusedRecoveryCodesAsync(Guid twoFactorAuthId);
    public Task<bool> TryConsumeStepAsync(Guid twoFactorAuthId, long step);
}
