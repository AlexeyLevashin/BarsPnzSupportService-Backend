using Domain.DbModels;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Repositories;

public class JobTitleRepository : IJobTitleRepository
{
    private readonly ApplicationContext _context;

    public JobTitleRepository(ApplicationContext context)
    {
        _context = context;
    }
    
    public async Task<int> GetCountByIdsAsync(List<Guid> jobTitleIds)
    {
        return await _context.JobTitles
            .AsNoTracking() 
            .Where(j => jobTitleIds.Contains(j.Id))
            .CountAsync();
    }

    public async Task<List<DbJobTitle>> GetAllAsync()
    {
        return await _context.JobTitles
            .AsNoTracking()
            .OrderBy(j => j.Name)
            .ToListAsync();
    }

    public async Task<DbJobTitle?> GetByNameAsync(string name)
    {
        return await _context.JobTitles
            .FirstOrDefaultAsync(j => j.Name.ToLower() == name.ToLower());
    }

    public async Task AddAsync(DbJobTitle jobTitle)
    {
        await _context.JobTitles.AddAsync(jobTitle);
    }
}