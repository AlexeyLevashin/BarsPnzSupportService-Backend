using Domain.DbModels;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private ApplicationContext _context;

    public EmployeeRepository(ApplicationContext context)
    {
        _context = context;
    }

    public async Task AddAsync(DbEmployee employee)
    {
        await _context.Employees.AddAsync(employee);
    }

    public void DeleteAsync(DbEmployee employee)
    {
        employee.IsDeleted = true;
    }

    public async Task<bool> CheckByPhoneNumberExistsAsync(string? phoneNumber, Guid? employeeId = null)
    {
       var query = _context.Employees.Where(e => e.PhoneNumber == phoneNumber);

       if (employeeId.HasValue)
       {
           query = query.Where(e => e.Id != employeeId.Value);
       }

       return await query.AnyAsync();
    }

    public async Task<bool> CheckByEmailExistsAsync(string? email, Guid? employeeId = null)
    {
        var query = _context.Employees.Where(e => e.Email == email);

        if (employeeId.HasValue)
        {
            query = query.Where(e => e.Id != employeeId);
        }

        return await query.AnyAsync();
    }

    public async Task<DbEmployee?> GetByIdAsync(Guid employeeId)
    {
        return await _context.Employees
            .Include(e => e.User)
            .Include(ei => ei.EmployeeInstitutions)
            .FirstOrDefaultAsync(e => e.Id == employeeId);
    }
    
    public async Task<(List<DbEmployee> Employees, int totalCount)> GetAllAsync(int pageNumber, int pageSize, List<Guid>? institutionIds)
    {
        var query = _context.Employees
            .Include(e => e.User)
            .Include(e => e.EmployeeInstitutions)
                .ThenInclude(ei => ei.Institution)
            .Include(e => e.EmployeeInstitutions)
                .ThenInclude(ei => ei.JobTitle)
            .AsNoTracking();
        
        if (institutionIds != null && institutionIds.Any())
        {
            query = query.Where(e => e.EmployeeInstitutions.Any(ei => institutionIds.Contains(ei.InstitutionId)));
        }

        var count = await query.CountAsync();
        
        var employees = await query
            .OrderByDescending(e => e.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        return (employees, count);
    }
}