using Application.Common.Pagination;
using Application.Dto.Messages.Requests;
using Application.Dto.Messages.Responses;
using Application.Exceptions.Requests;
using Application.Exceptions.Users;
using Application.Interfaces;
using Application.Interfaces.Hubs;
using Domain.DbModels;
using Domain.Enums;
using Domain.Interfaces;
using Mapster;

namespace Application.Services;

public class MessageService : IMessageService
{
    private readonly IMessageRepository _messageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRequestRepository _requestRepository;
    private readonly IRequestNotificationService _notificationService;
    private readonly IUserRepository _userRepository;
    
    public MessageService(IMessageRepository messageRepository, IUnitOfWork unitOfWork, IRequestRepository requestRepository, IRequestNotificationService notificationService, IUserRepository userRepository)
    {
        _messageRepository = messageRepository;
        _unitOfWork = unitOfWork;
        _requestRepository = requestRepository;
        _notificationService = notificationService;
        _userRepository = userRepository;
    }

    public async Task<GetMessageResponse> AddAsync(Guid requestId, CreateMessageRequest request, Guid senderId, UserRole userRole)
    {
        var requestCheck = await _requestRepository.GetByIdAsync(requestId);

        if (requestCheck is null)
        {
            throw new RequestNotFoundException();
        }
        
        var dbMessage = request.Adapt<DbMessage>();
        dbMessage.RequestId = requestId;
        dbMessage.SenderId = senderId;
        
        if (userRole == UserRole.User || userRole == UserRole.UserAdmin)
        {
            dbMessage.Type = MessageType.Public;
        }

        await _messageRepository.CreateAsync(dbMessage);
        await _unitOfWork.SaveChangesAsync();

        var fullMessage = await _messageRepository.GetByIdAsync(dbMessage.Id);

        var response = fullMessage.Adapt<GetMessageResponse>();

        await _notificationService.NotifyNewMessageAsync(requestId, response);
        
        return response;
    }

    public async Task<PagedResponse<GetMessageResponse>> GetAllMessagesAsync(int pageNumber, int pageSize, Guid? requestId)
    {
        if (pageNumber < 1) pageNumber = 1;
        
        if (pageSize < 1) pageSize = 20;
        
        if (pageSize > 50) pageSize = 50;

        var requestCheck = await _requestRepository.GetByIdAsync(requestId);
        if (requestCheck is null)
        {
            throw new RequestNotFoundException();
        }

        var messagesInfo = await _messageRepository.GetAllAsync(pageNumber, pageSize, requestId, MessageType.Public);
        var messages = messagesInfo.Messages.Adapt<List<GetMessageResponse>>();

        return new PagedResponse<GetMessageResponse>
        {
            Items = messages,
            PageInfo = new PageViewModel(pageNumber, messagesInfo.totalCount, pageSize)
        };
    }
    
    public async Task<PagedResponse<GetMessageResponse>> GetAllCommentsAsync(int pageNumber, int pageSize, Guid? requestId)
    {
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
}