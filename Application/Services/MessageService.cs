using Application.Common.Pagination;
using Application.Dto.Messages.Requests;
using Application.Dto.EmailMessages.Requests;
using Application.Dto.Messages.Responses;
using Application.Exceptions.Abstractions;
using Application.Exceptions.Messages;
using Application.Exceptions.Requests;
using Application.Exceptions.Users;
using Application.Extensions;
using Application.Interfaces;
using Application.Interfaces.Hubs;
using Application.Interfaces.Repositories;
using Domain.DbModels;
using Domain.Enums;
using Domain.Interfaces;
using Hangfire;
using Mapster;

namespace Application.Services;

public class MessageService : IMessageService
{
    private readonly IMessageRepository _messageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRequestRepository _requestRepository;
    private readonly IRequestNotificationService _notificationService;
    private readonly IUserRepository _userRepository;
    private readonly IAttachmentRepository _attachmentRepository;
    
    public MessageService(IMessageRepository messageRepository, IUnitOfWork unitOfWork, IRequestRepository requestRepository, IRequestNotificationService notificationService, IUserRepository userRepository, IAttachmentRepository attachmentRepository)
    {
        _messageRepository = messageRepository;
        _unitOfWork = unitOfWork;
        _requestRepository = requestRepository;
        _notificationService = notificationService;
        _userRepository = userRepository;
        _attachmentRepository = attachmentRepository;
    }

    public async Task<GetMessageResponse> AddAsync(Guid requestId, CreateMessageRequest request, Guid senderId, UserRole userRole)
    {
        var requestCheck = await _requestRepository.GetByIdAsync(requestId);

        if (requestCheck is null)
        {
            throw new RequestNotFoundException();
        }

        if (requestCheck.ClientId != senderId && (userRole == UserRole.User || userRole == UserRole.UserAdmin))
        {
            throw new UnacceptableRequestException();
        }
        
        var dbMessage = request.Adapt<DbMessage>();
        dbMessage.RequestId = requestId;
        dbMessage.SenderId = senderId;
        
        if (userRole == UserRole.User || userRole == UserRole.UserAdmin)
        {
            dbMessage.Type = MessageType.Public;
        }
        
        if (userRole == UserRole.Operator || userRole == UserRole.SuperAdmin)
        {
            bool checkUserAssignToRequest = await _requestRepository.CheckAlreadyAssigned(requestId, senderId);
            
            if(!checkUserAssignToRequest && request.Type == MessageType.Public)
                throw new NoOperatorAssignedToRequest();
        }

        var oldStatus = requestCheck.Status;
        requestCheck.Status = ChangeStatusAsync(requestCheck.Status, request.Status, userRole, request.Type);

        if ((requestCheck.Status == RequestStatus.Canceled || requestCheck.Status == RequestStatus.Closed) && request.Type == MessageType.Public)
        {
            requestCheck.ClosedAt = DateTime.UtcNow;
        }

        if (requestCheck.Status != RequestStatus.Canceled && requestCheck.Status != RequestStatus.Closed && request.Type == MessageType.Public)
        {
            requestCheck.ClosedAt = null;
        }
        
        await _messageRepository.CreateAsync(dbMessage);
        await _unitOfWork.SaveChangesAsync();

        if (request.AttachmentIds != null && request.AttachmentIds.Any())
        {
            await _attachmentRepository.AttachFilesToMessageAsync(request.AttachmentIds, dbMessage.Id);
        }
        
        var fullMessage = await _messageRepository.GetByIdAsync(dbMessage.Id);

        var response = fullMessage.Adapt<GetMessageResponse>();

        if (requestCheck.Type == RequestType.Email && request.Type == MessageType.Public &&
            (userRole == UserRole.Operator || userRole == UserRole.SuperAdmin))
        {
            var email = new EmailMessageRequest
            {
                ToEmail = requestCheck.Client.Email,
                Subject = $"Заявка: {requestCheck.Theme}",
                Message = fullMessage.Text ?? string.Empty,
                IsPlainText = true,
                ThreadRequestId = requestId,
                Attachments = fullMessage.Attachments.Select(attachment => new EmailAttachmentRequest
                {
                    FileName = attachment.FileName,
                    StorageKey = attachment.StorageKey,
                    ContentType = attachment.ContentType
                }).ToList()
            };
            BackgroundJob.Enqueue<IEmailService>(x => x.SendEmailAsync(email));
        }

        await _notificationService.NotifyNewMessageAsync(requestId, response);

        if (requestCheck.Type != RequestType.Email && senderId != requestCheck.ClientId && oldStatus != requestCheck.Status)
        {
            BackgroundJob.Schedule<IRequestService>(
                x => x.CheckAndSendEmailAsync(requestId, requestCheck.ClientId, DateTime.UtcNow),
                TimeSpan.FromMinutes(10));
        }
        
        return response;
    }

    public async Task<PagedResponse<GetMessageResponse>> GetAllMessagesAsync(int pageNumber, int pageSize, Guid? requestId, Guid userId, UserRole userRole)
    {
        if (pageNumber < 1) pageNumber = 1;
        
        if (pageSize < 1) pageSize = 20;
        
        if (pageSize > 50) pageSize = 50;

        var requestCheck = await _requestRepository.GetByIdAsync(requestId);
        if (requestCheck is null)
        {
            throw new RequestNotFoundException();
        }

        if (!requestCheck.CanBeReadBy(userId, userRole))
        {
            throw new UnacceptableRequestException();
        }

        var messagesInfo = await _messageRepository.GetAllAsync(pageNumber, pageSize, requestId, MessageType.Public);
        var messages = messagesInfo.Messages.Adapt<List<GetMessageResponse>>();

        return new PagedResponse<GetMessageResponse>
        {
            Items = messages,
            PageInfo = new PageViewModel(pageNumber, messagesInfo.totalCount, pageSize)
        };
    }
    
    public async Task<PagedResponse<GetMessageResponse>> GetAllCommentsAsync(int pageNumber, int pageSize, Guid? requestId, UserRole userRole)
    {
        if (userRole != UserRole.Operator && userRole != UserRole.SuperAdmin)
        {
            throw new ForbiddenException("Внутренние комментарии доступны только операторам и суперадминистраторам");
        }

        if (pageNumber < 1) pageNumber = 1;
        
        if (pageSize < 1) pageSize = 20;
        
        if (pageSize > 50) pageSize = 50;
        
        var request = await _requestRepository.GetByIdAsync(requestId);

        if (request is null)
        {
            throw new RequestNotFoundException();
        }

        var messagesInfo = await _messageRepository.GetAllAsync(pageNumber, pageSize, requestId, MessageType.Internal);
        var messages = messagesInfo.Messages.Adapt<List<GetMessageResponse>>();

        return new PagedResponse<GetMessageResponse>
        {
            Items = messages,
            PageInfo = new PageViewModel(pageNumber, messagesInfo.totalCount, pageSize)
        };
    }
    
    private RequestStatus ChangeStatusAsync(RequestStatus currentStatus, RequestStatus status, UserRole userRole, MessageType type)
    {
        if ((userRole == UserRole.User || userRole == UserRole.UserAdmin) && status != RequestStatus.Closed)
        {
            return currentStatus == RequestStatus.New ? RequestStatus.New : RequestStatus.InProgress;
        }
        
        if ((userRole == UserRole.Operator || userRole == UserRole.SuperAdmin) && type == MessageType.Internal)
        {
            return currentStatus;
        }

        if (status == RequestStatus.New)
        {
            return currentStatus;
        }

        if (currentStatus != status)
        {
            
        }
        
        return status;
    }
}
