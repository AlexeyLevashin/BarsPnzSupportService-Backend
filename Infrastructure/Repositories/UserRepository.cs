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

    public async Task<DbUser?> GetByIdAsync(Guid? userId)
    {
        return await _context.Users
            .Include(e => e.Employee)
                .ThenInclude(ei => ei.EmployeeInstitutions)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }
    
    public async Task<DbUser?> GetByIdWithDeyailsAsync(Guid? userId)
    {
        return await _context.Users
            .Include(e => e.Employee)
                .ThenInclude(ei => ei.EmployeeInstitutions)
                    .ThenInclude(i => i.Institution)
            .Include(e => e.Employee)
                .ThenInclude(ei => ei.EmployeeInstitutions)
                    .ThenInclude(i => i.JobTitle)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }
    
    public async Task<DbUser?> GetByEmployeeIdAsync(Guid employeeId)
    {
        return await _context.Users
            .Include(e => e.Employee)
                .ThenInclude(ei => ei.EmployeeInstitutions)
                    .ThenInclude(i => i.Institution)
            .Include(e => e.Employee)
                .ThenInclude(ei => ei.EmployeeInstitutions)
                    .ThenInclude(i => i.JobTitle)
            .FirstOrDefaultAsync(u => u.EmployeeId == employeeId);
    }

    public async Task<DbUser?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.Employee)
                .ThenInclude(e => e.EmployeeInstitutions)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> IsEmailTakenAsync(string email)
    {
        return await _context.Users.IgnoreQueryFilters().AnyAsync(u => u.Email == email);
    }
    
    public async Task<(List<DbUser> Users, int totalCount)> GetAllAsync(int pageNumber, int pageSize, List<Guid>? institutionIds)
    {
        var query = _context.Users
            .Include(u => u.Employee)
                .ThenInclude(e => e.EmployeeInstitutions)
                    .ThenInclude(ei => ei.Institution)
            .Include(u => u.Employee)
                .ThenInclude(e => e.EmployeeInstitutions)
                    .ThenInclude(ei => ei.JobTitle)
            .AsNoTracking();
        
        if (institutionIds != null && institutionIds.Any())
        {
            query = query.Where(u => u.Employee.EmployeeInstitutions.Any(ei => institutionIds.Contains(ei.InstitutionId)));
        }

        var count = await query.CountAsync();
        
        var users = await query
            .OrderByDescending(e => e.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        return (users, count);
    }

    public async Task<List<DbUser>> GetByRolesAsync(List<UserRole> roles)
    {
        var users = _context.Users
            .Include(e => e.Employee)
            .AsNoTracking();
        return await users.Where(u => roles.Contains(u.Role)).ToListAsync();
    }
    
    public async Task<bool> HasAdminAsync()
    {
        return await _context.Users.AnyAsync(u => u.Role == UserRole.SuperAdmin);
    }
    
    public async Task<bool> CheckUserExistByInstitutionId(Guid institutionId)
    {
        return await _context.Users.AnyAsync(u => u.Employee.EmployeeInstitutions.Any(ei => ei.InstitutionId == institutionId));
    }
}