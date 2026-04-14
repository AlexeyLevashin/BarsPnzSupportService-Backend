using Application.Dto.Messages.Requests;
using Application.Dto.Messages.Responses;
using Application.Exceptions.Requests;
using Application.Interfaces;
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
    
    public MessageService(IMessageRepository messageRepository, IUnitOfWork unitOfWork, IRequestRepository requestRepository)
    {
        _messageRepository = messageRepository;
        _unitOfWork = unitOfWork;
        _requestRepository = requestRepository;
    }

    public async Task<GetMessageResponse> AddAsync(CreateMessageRequest request, Guid senderId, UserRole userRole)
    {
        var requestCheck = await _requestRepository.GetByIdAsync(request.RequestId);

        if (requestCheck is null)
        {
            throw new RequestNotFoundException();
        }
        
        var message = request.Adapt<DbMessage>();
        message.SenderId = senderId;
        
        if (userRole == UserRole.User || userRole == UserRole.UserAdmin)
        {
            message.Type = MessageType.Public;
        }

        await _messageRepository.CreateAsync(message);
        await _unitOfWork.SaveChangesAsync();

        return new GetMessageResponse
        {
            Id = message.Id,
            Text = message.Text,
            CreatedAt = message.CreatedAt,
            Type = message.Type,
            SenderId = message.SenderId,
            RequestId = message.RequestId
        };
    }
}