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
            .Include(r => r.Institution)
            .Include(o => o.Operators)
                .ThenInclude(e => e.Employee)
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

    public async Task<DbRequestView?> GetViewAsync(Guid requestId, Guid userId)
    {
        return await _context.RequestViews
            .FirstOrDefaultAsync(v => v.RequestId == requestId && v.UserId == userId);
    }

    public async Task AddViewAsync(DbRequestView view)
    {
        await _context.RequestViews.AddAsync(view);
    }

    public async Task UpsertViewAsync(Guid requestId, Guid userId, DateTime viewedAt)
    {
        await _context.Database.ExecuteSqlAsync($@"
            INSERT INTO ""RequestViews"" (""RequestId"", ""UserId"", ""LastViewedAt"")
            VALUES ({requestId}, {userId}, {viewedAt})
            ON CONFLICT (""RequestId"", ""UserId"")
            DO UPDATE SET ""LastViewedAt"" = EXCLUDED.""LastViewedAt""");
    }

    public async Task<Dictionary<Guid, bool>> GetUnreadFlagsAsync(List<Guid> requestIds, Guid userId)
    {
        if (!requestIds.Any())
        {
            return new Dictionary<Guid, bool>();
        }

        var views = await _context.RequestViews
            .AsNoTracking()
            .Where(v => v.UserId == userId && requestIds.Contains(v.RequestId))
            .ToDictionaryAsync(v => v.RequestId, v => v.LastViewedAt);

        var lastForeignActivity = await _context.Messages
            .AsNoTracking()
            .Where(m => requestIds.Contains(m.RequestId) && m.SenderId != userId)
            .GroupBy(m => m.RequestId)
            .Select(g => new { RequestId = g.Key, LastAt = g.Max(m => m.CreatedAt) })
            .ToDictionaryAsync(x => x.RequestId, x => x.LastAt);

        var result = new Dictionary<Guid, bool>();
        foreach (var requestId in requestIds)
        {
            if (!views.TryGetValue(requestId, out var lastViewedAt))
            {
                // Никогда не открывал — непрочитано (новая заявка / не заходил)
                result[requestId] = true;
                continue;
            }

            if (!lastForeignActivity.TryGetValue(requestId, out var lastActivityAt))
            {
                result[requestId] = false;
                continue;
            }

            result[requestId] = lastActivityAt > lastViewedAt;
        }

        return result;
    }
}