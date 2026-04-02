using Domain.DbModels;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationContext _context;

    public UserRepository(ApplicationContext context)
    {
        _context = context;
    }
    
    public async Task AddAsync(DbUser dbUser)
    {
        await _context.Users.AddAsync(dbUser);
    }

    public void UpdateAsync(DbUser dbUser)
    {
        _context.Users.Update(dbUser);
    }

    public void DeleteAsync(DbUser dbUser)
    {
        _context.Users.Remove(dbUser);
    }

    public async Task<DbUser?> GetByIdAsync(Guid? userId)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<DbUser?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<(List<DbUser> Users, int totalCount)> GetAllAsync(int pageNumber, int pageSize, Guid? institutionId = null)
    {
        var query = _context.Users.AsNoTracking();
        
        if (institutionId.HasValue)
        {
            query = query.Where(u => u.InstitutionId == institutionId);
        }

        var count = await query.CountAsync();
        
        var users = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        return (users, count);
    }

    public async Task<bool> HasAdminAsync()
    {
        return await _context.Users.AnyAsync(u => u.Role == UserRole.SuperAdmin);
    }
    
    public async Task<bool> CheckUserExistByInstitutionId(Guid institutionId)
    {
        return await _context.Users.AnyAsync(u => u.InstitutionId == institutionId);
    }
}