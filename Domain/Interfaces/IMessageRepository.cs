using Domain.DbModels;
using Domain.Enums;

namespace Domain.Interfaces;

public interface IMessageRepository
{
    public Task CreateAsync(DbMessage message);
    public Task<(List<DbMessage> Messages, int totalCount)> GetAllAsync(int pageNumber, int pageSize, Guid? requestId, MessageType type);
    public Task<DbMessage?> GetByIdAsync(Guid messageId);
}