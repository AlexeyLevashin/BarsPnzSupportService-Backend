using Application.Dto.Attachments.Requests;
using Application.Dto.Attachments.Responses;
using Application.Exceptions.Abstractions;
using Application.Exceptions.Attachments;
using Application.Exceptions.Files;
using Application.Exceptions.Messages;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Domain.DbModels;
using Domain.Interfaces;
using Mapster;

namespace Application.Services;

public class AttachmentService : IAttachmentService
{
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IMinioFileStorageService _storage;
    private readonly IUnitOfWork _unitOfWork;

    public AttachmentService(IAttachmentRepository attachmentRepository, IMessageRepository messageRepository,
        IMinioFileStorageService storage, IUnitOfWork unitOfWork)
    {
        _attachmentRepository = attachmentRepository;
        _messageRepository = messageRepository;
        _storage = storage;
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

    public async Task DeleteAsync(Guid attachmentId, Guid currentUserId)
    {
        var attachment = await _attachmentRepository.GetByIdAsync(attachmentId);

        if (attachment is null)
        {
            return;
        }

        // Привязанный файл может удалить только автор сообщения
        if (attachment.MessageId is { } messageId)
        {
            var message = await _messageRepository.GetByIdAsync(messageId);

            if (message is not null && message.SenderId != currentUserId)
            {
                throw new ForbiddenException("Нельзя удалять файл из чужого сообщения");
            }
        }

        await _storage.DeleteAsync(attachment.StorageKey, "attachments");
        _attachmentRepository.DeleteAsync(attachment);
        await _unitOfWork.SaveChangesAsync();
    }
}