using Domain.DbModels;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Persistence;

namespace Infrastructure.Repositories;

public class RequestRepository : IRequestRepository
{
    private readonly ApplicationContext _context;

    public RequestRepository(ApplicationContext context)
    {
        _context = context;
    }
    
    public async Task CreateAsync(DbRequest request)
    {
        await _context.Requests.AddAsync(request);
    }

    public async Task<DbRequest?> GetByIdAsync(Guid? id)
    {
        return await _context.Requests.FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<(List<DbRequest> Requests, int totalCount)> GetAllAsync(int pageNumber, int pageSize, Guid? userId = null)
    {
        var query = _context.Requests.AsNoTracking();

        if (userId.HasValue)
        {
            query = query.Where(r => r.ClientId == userId);
        }

        var count = await query.CountAsync();

        var requests = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (requests, count);
    }

    public void UpdateAsync(DbRequest dbRequest)
    {
        _context.Requests.Update(dbRequest);
    }
}