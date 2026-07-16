using Application.Common.Pagination;
using Application.Dto.Requests.Requests;
using Application.Dto.Requests.Responses;
using Application.Exceptions.Abstractions;
using Application.Exceptions.Institutions;
using Application.Exceptions.Messages;
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
    private readonly IAttachmentRepository _attachmentRepository;

    public RequestService(IRequestRepository requestRepository, IMessageRepository messageRepository,
        IUnitOfWork unitOfWork, IUserRepository userRepository, IRequestNotificationService notificationService,
        IAttachmentRepository attachmentRepository)
    {
        _requestRepository = requestRepository;
        _messageRepository = messageRepository;
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _notificationService = notificationService;
        _attachmentRepository = attachmentRepository;
    }
    
    public async Task<CreateRequestResponse> AddAsync(CreateRequestRequest request, Guid userId, UserRole userRole, List<Guid> institutionIds)
    {
        bool isRestrictedRole = userRole == UserRole.User || userRole == UserRole.UserAdmin;

        if (isRestrictedRole && request.InstitutionId == null)
        {
            throw new NoInstitutionSelected();
        }

        if (isRestrictedRole && !institutionIds.Contains(request.InstitutionId!.Value))
        {
            throw new ForbiddenException("Вы не можете создать заявку для учреждения, в котором не числитесь");
        }
        
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
        
        if (request.Message.AttachmentIds != null && request.Message.AttachmentIds.Any())
        {
            await _attachmentRepository.AttachFilesToMessageAsync(request.Message.AttachmentIds, dbMessage.Id);
        }
        
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
    
    public async Task AssignToOperatorAsync(Guid requestId, Guid operatorId)
    {
        if (Guid.Empty == requestId)
        {
            throw new BadRequestException("ID заявки не может быть пустым");
        }

        var request = await _requestRepository.GetByIdForAssignmentAsync(requestId);

        if (request is null)
        {
            throw new RequestNotFoundException();
        }

        if (request.Status == RequestStatus.New)
        {
            request.Status = RequestStatus.InProgress;
        }
        
        var dbOperator = await _userRepository.GetByIdAsync(operatorId);

        if (dbOperator is null)
        {
            throw new UserNotFoundException();  
        }

        if (dbOperator.Role == UserRole.User || dbOperator.Role == UserRole.UserAdmin)
        {
            throw new ForbiddenException("Пользователя с данной ролью нельзя назначить оператором");
        }
        
        if (await _requestRepository.CheckAlreadyAssigned(requestId, operatorId))
        {
            throw new OperatorAlreadyAssignedException();
        }
        
        request.Operators.Add(dbOperator);
        
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteOperatorAsync(Guid requestId, Guid operatorId)
    {
        if (Guid.Empty == requestId)
        {
            throw new BadRequestException("ID заявки не может быть пустым");
        }

        var request = await _requestRepository.GetByIdForAssignmentAsync(requestId);

        if (request is null)
        {
            throw new RequestNotFoundException();
        }

        if (request.Status == RequestStatus.Closed || request.Status == RequestStatus.Canceled)
        {
            throw new ConflictException("Нельзя удалить оператора с выполненной заявки");
        }
        
        var dbOperator = request.Operators.FirstOrDefault(op => op.Id == operatorId);

        if (dbOperator is null)
        {
            throw new OperatorIsNotAssignedException(); 
        }
        
        request.Operators.Remove(dbOperator);
        
        if (request.Operators.Count == 0 && request.Status != RequestStatus.New)
        {
            request.Status = RequestStatus.New;
        }
        
        await _unitOfWork.SaveChangesAsync();
    }
    
    public async Task ChangeStatusAsync(Guid requestId, UpdateStatusRequest request, Guid userId, UserRole userRole)
    {
        var requestCheck = await _requestRepository.GetByIdAsync(requestId);

        if (requestCheck is null)
        {
            throw new RequestNotFoundException();
        }

        if (request.Status != RequestStatus.Canceled && request.Status != RequestStatus.Closed && request.Status != RequestStatus.InProgress)
        {
            throw new UnacceptableStatusException();
        }

        if (requestCheck.ClientId != userId && (userRole == UserRole.User || userRole == UserRole.UserAdmin))
        {
            throw new UnacceptableRequestException();
        }
        
        if (userRole == UserRole.Operator || userRole == UserRole.SuperAdmin)
        {
            bool checkUserAssignToRequest = await _requestRepository.CheckAlreadyAssigned(requestId, userId);
            
            if(!checkUserAssignToRequest)
                throw new NoOperatorAssignedToRequest();
        }

        if (request.Status == RequestStatus.Canceled && (userRole == UserRole.User || userRole == UserRole.UserAdmin))
        {
            throw new ForbiddenException("Пользователям с данной ролью доступно только закрытие заявки");
        }
        
        requestCheck.Status = request.Status;
        if (requestCheck.Status == RequestStatus.Canceled || requestCheck.Status == RequestStatus.Closed)
        {
            requestCheck.ClosedAt = DateTime.UtcNow;
        }

        if (requestCheck.Status != RequestStatus.Canceled && requestCheck.Status != RequestStatus.Closed)
        {
            requestCheck.ClosedAt = null;
        }
        
        await _unitOfWork.SaveChangesAsync();
    }
}