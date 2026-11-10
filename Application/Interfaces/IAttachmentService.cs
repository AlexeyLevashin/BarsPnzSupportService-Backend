using Application.Dto.Attachments.Requests;
using Application.Dto.Attachments.Responses;

namespace Application.Interfaces;

public interface IAttachmentService
{
    public Task AddAsync(AddAttachmentRequest request);
    public Task<AddAttachmentResponse?> GetByIdAsync(Guid fileId);
    public Task AttachFilesToMessageAsync(List<Guid>? fileIds, Guid? messageId);
    public Task DeleteAsync(Guid attachmentId, Guid currentUserId);
}
