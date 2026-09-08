using Domain.DbModels;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Repositories;

public class PasswordResetCodeRepository : IPasswordResetCodeRepository
{
    private readonly ApplicationContext _context;

    public PasswordResetCodeRepository(ApplicationContext context)
    {
        _context = context;
    } 
    
    public async Task AddAsync(DbPasswordResetCode entity)
    {
        await _context.PasswordResetCodes.AddAsync(entity);
    }

    public async Task InvalidateUnusedByUserIdAsync(Guid userId)
    {
        await _context.PasswordResetCodes
            .Where(c => c.UserId == userId && c.UsedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.UsedAt, DateTime.UtcNow));
    }

    public async Task<DbPasswordResetCode?> GetActiveByUserIdAsync(Guid userId)
    {
        return await _context.PasswordResetCodes.FirstOrDefaultAsync(c => c.UserId == userId && c.UsedAt == null && c.ExpiresAt > DateTime.UtcNow);
    }

    public async Task<DbPasswordResetCode?> GetByResetTokenAsync(Guid resetToken)
    {
        return await _context.PasswordResetCodes.FirstOrDefaultAsync(c => c.ResetToken == resetToken && c.UsedAt == null && c.ExpiresAt > DateTime.UtcNow);
    }
}