using Domain.DbModels;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Repositories;

public class InstitutionRepository : IInstitutionRepository
{
    private readonly ApplicationContext _context;

    public InstitutionRepository(ApplicationContext context)
    {
        _context = context;
    }

    public async Task AddAsync(DbInstitution dbInstitution)
    {
        await _context.Institutions.AddAsync(dbInstitution);
    }

    public void UpdateAsync(DbInstitution dbInstitution)
    {
        _context.Institutions.Update(dbInstitution);
    }

    public void DeleteAsync(DbInstitution dbInstitution)
    {
        _context.Institutions.Remove(dbInstitution);
    }

    public async Task<List<DbInstitution>> GetAllByEmployeeIdAsync(Guid employeeId)
    {
        return await _context.EmployeeInstitutions.AsNoTracking().Where(i => i.EmployeeId == employeeId).Select(i => i.Institution).ToListAsync();
    }
    
    public async Task<DbInstitution?> GetByIdAsync(Guid id)
    {
        return await _context.Institutions
            .Include(ei => ei.Head)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<int> GetCountByIdsAsync(List<Guid> institutionIds)
    {
        return await _context.Institutions
            .AsNoTracking() 
            .Where(i => institutionIds.Contains(i.Id))
            .CountAsync();
    }
    
    public async Task<DbInstitution?> GetByInnAsync(string inn)
    {
        return await _context.Institutions.FirstOrDefaultAsync(i => i.INN == inn);
    }

    public async Task<(List <DbInstitution> Institutions, int totalCount)> GetAllAsync(int pageNumber, int pageSize)
    {
        var query = _context.Institutions
            .Include(h => h.Head)
            .AsNoTracking();
        var count = await query.CountAsync();
        var institutions = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (institutions, count);
    }
}