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
}