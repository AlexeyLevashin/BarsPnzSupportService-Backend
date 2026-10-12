using Domain.DbModels;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Repositories;

public class InboundEmailRepository : IInboundEmailRepository
{
    private readonly ApplicationContext _context;

    public InboundEmailRepository(ApplicationContext context)
    {
        _context = context;
    }

    public Task<bool> ExistsAsync(long uidValidity, long uid, CancellationToken cancellationToken = default)
    {
        return _context.InboundEmails.AnyAsync(e => e.UidValidity == uidValidity && e.Uid == uid,
            cancellationToken);
    }

    public Task<Guid?> GetRequestIdByMessageIdAsync(string messageId, CancellationToken cancellationToken = default)
    {
        return _context.InboundEmails
            .Where(e => e.MessageId == messageId)
            .Select(e => (Guid?)e.RequestId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task AddAsync(DbInboundEmail email, CancellationToken cancellationToken = default)
    {
        return _context.InboundEmails.AddAsync(email, cancellationToken).AsTask();
    }
}
