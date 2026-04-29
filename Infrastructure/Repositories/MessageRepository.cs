using Application.Common.Pagination;
using Application.Dto.Messages.Responses;
using Domain.DbModels;
using Domain.Interfaces;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Minio.DataModel.Select;
using Persistence;
using MessageType = Domain.Enums.MessageType;

namespace Infrastructure.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly ApplicationContext _context;

    public MessageRepository(ApplicationContext context)
    {
        _context = context;
    }
    
    public async Task CreateAsync(DbMessage message)
    {
        await _context.Messages.AddAsync(message);
    }

    public async Task<(List<DbMessage> Messages, int totalCount)> GetAllAsync(int pageNumber, int pageSize, Guid? requestId, MessageType type)
    {
        IQueryable<DbMessage> query = _context.Messages
            .AsNoTracking()
            .Include(m => m.Sender)
            .Where(m => m.RequestId == requestId)
            .Where(m => m.Type == type);

        var count = await query.CountAsync();

        var requests = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (requests, count);
    }

    public async Task<DbMessage?> GetByIdAsync(Guid messageId)
    {
        return await _context.Messages
            .Include(u => u.Sender)
            .FirstOrDefaultAsync(m => m.Id == messageId);
    } 
}