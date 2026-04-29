using Application.Common.Pagination;
using Application.Dto.Requests.Requests;
using Application.Dto.Requests.Responses;
using Application.Exceptions.Abstractions;
using Application.Exceptions.Requests;
using Application.Exceptions.Users;
using Application.Interfaces;
using Application.Interfaces.Hubs;
using Domain.DbModels;
using Domain.Enums;
using Domain.Interfaces;
using Mapster;
using Microsoft.AspNetCore.SignalR;

namespace Application.Services;

public class RequestService : IRequestService
{
    private readonly IRequestRepository _requestRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;
    private readonly IRequestNotificationService _notificationService;

    public RequestService(IRequestRepository requestRepository, IMessageRepository messageRepository,
        IUnitOfWork unitOfWork, IUserRepository userRepository, IRequestNotificationService notificationService)
    {
        _requestRepository = requestRepository;
        _messageRepository = messageRepository;
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _notificationService = notificationService;
    }
    
    public async Task<CreateRequestResponse> AddAsync(CreateRequestRequest request, Guid userId)
    {
        var dbRequest = request.Adapt<DbRequest>();
        var dbMessage = request.Message.Adapt<DbMessage>();
        dbRequest.Status = RequestStatus.New;
        dbRequest.ClientId = userId;
        dbMessage.Type = MessageType.Public;
        dbMessage.RequestId = dbRequest.Id;
        dbMessage.SenderId = userId;
        
        dbRequest.Messages.Add(dbMessage);
        await _requestRepository.CreateAsync(dbRequest);
        await _unitOfWork.SaveChangesAsync();

        var fullRequest = await _requestRepository.GetByIdAsync(dbRequest.Id);

        var getRequestResponse = fullRequest.Adapt<GetRequestResponse>();
    
        await _notificationService.NotifyNewRequestAsync(getRequestResponse);
        
        return new CreateRequestResponse
        {
            Id = dbRequest.Id,
            CreatedAt = dbRequest.CreatedAt,
            Status = dbRequest.Status
        };
    }

    public async Task<PagedResponse<GetRequestResponse>> GetAllAsync(int pageNumber, int pageSize)
    {
        if (pageNumber < 1) pageNumber = 1;

        if (pageSize < 1) pageSize = 20;

        if (pageSize > 50) pageSize = 50;

        var requestsInfo = await  _requestRepository.GetAllAsync(pageNumber, pageSize);
        var requests = requestsInfo.Requests.Adapt<List<GetRequestResponse>>();

        return new PagedResponse<GetRequestResponse>
        {
            Items = requests,
            PageInfo = new PageViewModel(pageNumber, requestsInfo.totalCount, pageSize)
        };
    }
    
    public async Task<PagedResponse<GetRequestResponse>> GetMyAsync(int pageNumber, int pageSize, Guid? userId)
    {
        if (pageNumber < 1) pageNumber = 1;

        if (pageSize < 1) pageSize = 20;

        if (pageSize > 50) pageSize = 50;

        if (userId is null)
        {
            throw new UserNotFoundException();
        }
        var requestsInfo = await _requestRepository.GetAllAsync(pageNumber, pageSize, userId);
        var requests = requestsInfo.Requests.Adapt<List<GetRequestResponse>>();

        return new PagedResponse<GetRequestResponse>
        {
            Items = requests,
            PageInfo = new PageViewModel(pageNumber, requestsInfo.totalCount, pageSize)
        };
    }

    public async Task<GetRequestResponse> GetRequestByIdAsync(Guid? id)
    {
        if (Guid.Empty == id)
        {
            throw new BadRequestException("ID заявки не может быть пустым");
        }

        var request = await _requestRepository.GetByIdAsync(id);

        if (request is null)
        {
            throw new RequestNotFoundException();
        }

        return request.Adapt<GetRequestResponse>();
    }
    
    public async Task AssignToOperatorAsync(Guid? requestId, Guid operatorId)
    {
        if (Guid.Empty == requestId)
        {
            throw new BadRequestException("ID заявки не может быть пустым");
        }

        var request = await _requestRepository.GetByIdAsync(requestId);

        if (request is null)
        {
            throw new RequestNotFoundException();
        }
        
        if (request.OperatorId is not null)
        {
            throw new RequestAlreadyAssignedException();
        }

        request.OperatorId = operatorId;
        
        await _unitOfWork.SaveChangesAsync();
    }
}