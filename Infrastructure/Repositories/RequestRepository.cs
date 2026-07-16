using System.Security.Claims;
using Domain.DbModels;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.VisualBasic.CompilerServices;
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
        return await _context.Requests
            .IgnoreQueryFilters()
            .Include(r => r.Client)
                .ThenInclude(u => u.Employee)
                    .ThenInclude(e => e.EmployeeInstitutions)
                        .ThenInclude(ei => ei.Institution)

            .Include(r => r.Client)
                .ThenInclude(u => u.Employee)
                    .ThenInclude(e => e.EmployeeInstitutions)
                        .ThenInclude(ei => ei.JobTitle)
            .Include(o => o.Operators)
            .FirstOrDefaultAsync(r => r.Id == id);
    }
    
    public async Task<DbRequest?> GetByIdForAssignmentAsync(Guid? id)
    {
        return await _context.Requests
            .Include(o => o.Operators)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<(List<DbRequest> Requests, int totalCount)> GetAllAsync(int pageNumber, int pageSize, Guid? userId = null)
    {
        IQueryable<DbRequest> query = _context.Requests
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(r => r.Client)
                .ThenInclude(u => u.Employee)
                    .ThenInclude(e => e.EmployeeInstitutions)
                        .ThenInclude(ei => ei.Institution)

            .Include(r => r.Client)
                .ThenInclude(u => u.Employee)
                    .ThenInclude(e => e.EmployeeInstitutions)
                        .ThenInclude(ei => ei.JobTitle)
            
            .Include(i => i.Institution)
            
            .Include(o => o.Operators)
                .ThenInclude(e => e.Employee);
        
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
    
    public async Task<bool> CheckAlreadyAssigned(Guid requestId, Guid operatorId)
    {
         return await _context.Requests
            .AnyAsync(r => r.Id == requestId && r.Operators.Any(o => o.Id == operatorId));
    }
    
    public async Task<List<DbRequest>> GetStaleRequestsAsync(DateTime deadline)
    {
        return await _context.Requests
            .Where(r => r.Status == RequestStatus.PendingReview || r.Status == RequestStatus.ClientDataRequest)
            .Where(r => r.Messages.Max(m => m.CreatedAt) <= deadline)
            .ToListAsync();
    }
}