using Domain.DbModels;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Repositories;

public class PasswordResetRepository : IPasswordResetRepository
{
    private readonly ApplicationContext _context;

    public PasswordResetRepository(ApplicationContext context)
    {
        _context = context;
    }

    public async Task<bool> CreateAsync(DbPasswordResetRequest request)
    {
        return await WithUserLockAsync(request.UserId, async _ =>
        {
            var now = DateTime.UtcNow;
            var recent = await _context.PasswordResetRequests.AnyAsync(r =>
                r.UserId == request.UserId && !r.IsUsed && r.ExpiresAt > now.AddMinutes(9));
            if (recent)
            {
                return false;
            }

            await _context.PasswordResetRequests.Where(r => r.UserId == request.UserId && !r.IsUsed)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.IsUsed, true));
            await _context.PasswordResetRequests.AddAsync(request);
            await _context.SaveChangesAsync();
            return true;
        });
    }

    public async Task<DbPasswordResetRequest?> VerifyAsync(Guid userId, string codeHash)
    {
        return await WithUserLockAsync<DbPasswordResetRequest?>(userId, async user =>
        {
            var now = DateTime.UtcNow;
            ReplenishAttempts(user, now);
            await _context.SaveChangesAsync();
            if (user.PasswordResetAttemptsRemaining == 0)
            {
                return null;
            }

            var request = await _context.PasswordResetRequests.FirstOrDefaultAsync(r =>
                r.UserId == userId && !r.IsUsed && !r.IsVerified && r.ExpiresAt > now);
            if (request is null)
            {
                return null;
            }

            if (request.CodeHash != codeHash)
            {
                request.FailedAttempts++;
                user.PasswordResetAttemptsRemaining--;
                user.PasswordResetAttemptsRefillAt ??= now.AddMinutes(30);
                await _context.SaveChangesAsync();
                return null;
            }

            request.IsVerified = true;
            await _context.SaveChangesAsync();
            return request;
        });
    }

    public async Task<bool> ResetPasswordAsync(Guid requestId, Guid userId, string passwordHash)
    {
        return await WithUserLockAsync(userId, async _ =>
        {
            var now = DateTime.UtcNow;
            var updated = await _context.PasswordResetRequests.Where(r =>
                    r.Id == requestId && r.UserId == userId && r.IsVerified && !r.IsUsed && r.ExpiresAt > now)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.IsUsed, true));
            if (updated == 0)
            {
                return false;
            }

            await _context.Users.Where(u => u.Id == userId)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.PasswordHash, passwordHash));
            return true;
        });
    }

    public async Task CancelAsync(Guid requestId)
    {
        await _context.PasswordResetRequests.Where(r => r.Id == requestId)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.IsUsed, true));
    }

    private static void ReplenishAttempts(DbUser user, DateTime now)
    {
        var refillAt = user.PasswordResetAttemptsRefillAt;
        if (refillAt is null || refillAt > now)
        {
            return;
        }

        var periods = (long)((now - refillAt.Value).Ticks / TimeSpan.FromMinutes(30).Ticks) + 1;
        user.PasswordResetAttemptsRemaining = (int)Math.Min(3, user.PasswordResetAttemptsRemaining + periods * 2);
        user.PasswordResetAttemptsRefillAt = user.PasswordResetAttemptsRemaining == 3
            ? null
            : refillAt.Value.AddMinutes(periods * 30);
    }

    private async Task<T> WithUserLockAsync<T>(Guid userId, Func<DbUser, Task<T>> action)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            _context.ChangeTracker.Clear();
            await using var transaction = await _context.Database.BeginTransactionAsync();
            var users = await _context.Users
                .FromSqlInterpolated($"SELECT * FROM \"Users\" WHERE \"Id\" = {userId} FOR UPDATE")
                .IgnoreQueryFilters().ToListAsync();
            if (users.Count == 0 || users[0].IsDeleted || !users[0].IsActivated)
            {
                return default!;
            }

            var result = await action(users[0]);
            await transaction.CommitAsync();
            return result;
        });
    }
}
