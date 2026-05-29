using Domain.DbModels;

namespace Domain.Interfaces;

public interface IAttachmentRepository
{
    public Task AddAsync(DbAttachment dbAttachment);
    public Task<DbAttachment?> GetByIdAsync(Guid fileId);
    public Task AttachFilesToMessageAsync(List<Guid> fileIds, Guid messageId);
    public void DeleteAsync(DbAttachment dbAttachment);
}