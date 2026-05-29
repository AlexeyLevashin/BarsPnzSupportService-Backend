using Application.Dto.Attachments.Requests;
using Application.Dto.Attachments.Responses;
using Application.Exceptions.Attachments;
using Application.Exceptions.Files;
using Application.Exceptions.Messages;
using Application.Interfaces;
using Domain.DbModels;
using Domain.Interfaces;
using Mapster;

namespace Application.Services;

public class AttachmentService : IAttachmentService
{
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AttachmentService(IAttachmentRepository attachmentRepository, IUnitOfWork unitOfWork)
    {
        _attachmentRepository = attachmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task AddAsync(AddAttachmentRequest request)
    {
        var dbAttachment = request.Adapt<DbAttachment>();
        await _attachmentRepository.AddAsync(dbAttachment);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<AddAttachmentResponse?> GetByIdAsync(Guid id)
    {
        var dbAttachment = await _attachmentRepository.GetByIdAsync(id);

        if (dbAttachment is null)
        {
            throw new AttachmentNotFoundException();
        }
        
        var res = dbAttachment.Adapt<AddAttachmentResponse>();
        return res;
    }

    public async Task AttachFilesToMessageAsync(List<Guid>? fileIds, Guid? messageId)
    {
        if (fileIds is null)
        {
            throw new FileIsEmptyException();
        }

        if (messageId is null)
        {
            throw new MessageNotFoundException();
        }
        
        await _attachmentRepository.AttachFilesToMessageAsync(fileIds, messageId.Value);
    }

    public async Task DeleteAsync(Guid id)
    {
        var attachment = await _attachmentRepository.GetByIdAsync(id);

        if (attachment is null)
        {
            return;
        }
        
        _attachmentRepository.DeleteAsync(attachment);
        await _unitOfWork.SaveChangesAsync();
    }
}