using Domain.DbModels;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Repositories;

public class TwoFactorAuthRepository : ITwoFactorAuthRepository
{
    private readonly ApplicationContext _context;

    public TwoFactorAuthRepository(ApplicationContext context)
    {
        _context = context;
    }
    
    public async Task AddAsync(DbTwoFactorAuth twoFactorAuth)
    {
        await _context.TwoFactorAuths.AddAsync(twoFactorAuth);
    }

    public Task<DbTwoFactorAuth?> GetByUserIdAsync(Guid userId)
    {
        return _context.TwoFactorAuths
            .FirstOrDefaultAsync(e => e.UserId == userId);
    }

    public Task<DbTwoFactorAuth?> GetByUserIdWithRecoveryCodesAsync(Guid userId)
    {
        return _context.TwoFactorAuths
            .Include(e => e.RecoveryCodes)
            .FirstOrDefaultAsync(e => e.UserId == userId);
    }

    public async Task AddRecoveryCodesAsync(List<DbTwoFactorRecoveryCode> recoveryCodes)
    {
        await _context.TwoFactorRecoveryCodes.AddRangeAsync(recoveryCodes);
    }

    public async Task RemoveUnusedRecoveryCodesAsync(Guid twoFactorAuthId)
    {
        await _context.TwoFactorRecoveryCodes
            .Where(e => e.TwoFactorAuthId == twoFactorAuthId && !e.IsUsed)
            .ExecuteDeleteAsync();
    }

    public async Task<bool> TryConsumeStepAsync(Guid twoFactorAuthId, long step)
    {
        var updated = await _context.TwoFactorAuths
            .Where(e => e.Id == twoFactorAuthId && e.LastUsedStep < step)
            .ExecuteUpdateAsync(s => s.SetProperty(e => e.LastUsedStep, step));

        return updated > 0;
    }
}
