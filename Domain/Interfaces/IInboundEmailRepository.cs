using Domain.DbModels;

namespace Domain.Interfaces;

public interface IInboundEmailRepository
{
    Task<bool> ExistsAsync(long uidValidity, long uid, CancellationToken cancellationToken = default);
    Task<Guid?> GetRequestIdByMessageIdAsync(string messageId, CancellationToken cancellationToken = default);
    Task AddAsync(DbInboundEmail email, CancellationToken cancellationToken = default);
}
