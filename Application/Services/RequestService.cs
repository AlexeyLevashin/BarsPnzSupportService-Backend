using Application.Common.Pagination;
using Application.Dto.EmailMessages.Requests;
using Application.Dto.Requests.Requests;
using Application.Dto.Requests.Responses;
using Application.Exceptions.Abstractions;
using Application.Exceptions.Employees;
using Application.Exceptions.Institutions;
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
using Microsoft.AspNetCore.SignalR;

namespace Application.Services;

public class RequestService : IRequestService
{
    private readonly IRequestRepository _requestRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IInstitutionRepository _institutionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;
    private readonly IRequestNotificationService _notificationService;
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IEmailTemplateBuilder _emailTemplateBuilder;

    public RequestService(IRequestRepository requestRepository, IMessageRepository messageRepository,
        IUnitOfWork unitOfWork, IUserRepository userRepository, IRequestNotificationService notificationService,
        IAttachmentRepository attachmentRepository, IEmailTemplateBuilder emailTemplateBuilder,
        IInstitutionRepository institutionRepository)
    {
        _requestRepository = requestRepository;
        _messageRepository = messageRepository;
        _institutionRepository = institutionRepository;
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _notificationService = notificationService;
        _attachmentRepository = attachmentRepository;
        _emailTemplateBuilder = emailTemplateBuilder;
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
        
        if (request.InstitutionId is not null)
        {
            var institution = await _institutionRepository.GetByIdAsync(request.InstitutionId.Value);
            if (institution is null)
            {
                throw new InstitutionNotFoundException();
            }
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

        await _requestRepository.UpsertViewAsync(dbRequest.Id, userId, dbRequest.CreatedAt);
        
        var fullRequest = await _requestRepository.GetByIdAsync(dbRequest.Id);

        var getRequestResponse = fullRequest.Adapt<GetRequestResponse>();
    
        await _notificationService.NotifyNewRequestAsync(getRequestResponse);
        
        return new CreateRequestResponse
        {
            Id = dbRequest.Id,
            CreatedAt = dbRequest.CreatedAt,
            Status = dbRequest.Status,
            Type = dbRequest.Type
        };
    }

    public async Task<CreateRequestResponse> AddPhoneRequestAsync(CreatePhoneRequestRequest request, Guid createdByUserId)
    {
        var creator = await _userRepository.GetByIdAsync(createdByUserId);
        if (creator is null || creator.Role != UserRole.Operator && creator.Role != UserRole.SuperAdmin)
        {
            throw new ForbiddenException("Создавать заявки по звонку может только оператор или суперадминистратор");
        }

        if (request.ClientId == Guid.Empty)
        {
            throw new BadRequestException("ID клиента не может быть пустым");
        }

        var client = await _userRepository.GetByIdAsync(request.ClientId);
        if (client is null || !client.IsActivated ||
            client.Role != UserRole.User && client.Role != UserRole.UserAdmin)
        {
            throw new UserNotFoundException("Активный клиент не найден");
        }

        var institutionId = request.InstitutionId ??
            (client.Employee.EmployeeInstitutions.Count == 1
                ? client.Employee.EmployeeInstitutions[0].InstitutionId
                : null);

        if (institutionId is not null)
        {
            var institution = await _institutionRepository.GetByIdAsync(institutionId.Value);
            if (institution is null)
            {
                throw new InstitutionNotFoundException();
            }

            if (!client.Employee.EmployeeInstitutions.Any(ei => ei.InstitutionId == institutionId.Value))
            {
                throw new ForbiddenException("Клиент не числится в выбранном учреждении");
            }
        }

        var phoneRequest = new DbRequest
        {
            Theme = request.Theme,
            ClientId = client.Id,
            Client = client,
            InstitutionId = institutionId,
            Status = RequestStatus.New,
            Type = RequestType.Phone,
            Priority = request.Priority
        };
        var firstMessage = request.Message.Adapt<DbMessage>();
        firstMessage.RequestId = phoneRequest.Id;
        firstMessage.SenderId = creator.Id;
        firstMessage.Type = MessageType.Public;
        phoneRequest.Messages.Add(firstMessage);

        await _requestRepository.CreateAsync(phoneRequest);
        await _unitOfWork.SaveChangesAsync();

        if (request.Message.AttachmentIds is { Count: > 0 })
        {
            await _attachmentRepository.AttachFilesToMessageAsync(request.Message.AttachmentIds, firstMessage.Id);
        }

        await _requestRepository.UpsertViewAsync(phoneRequest.Id, createdByUserId, phoneRequest.CreatedAt);

        var fullRequest = await _requestRepository.GetByIdAsync(phoneRequest.Id);
        var response = fullRequest.Adapt<GetRequestResponse>();
        await _notificationService.NotifyNewRequestAsync(response);

        return new CreateRequestResponse
        {
            Id = phoneRequest.Id,
            CreatedAt = phoneRequest.CreatedAt,
            Status = phoneRequest.Status,
            Type = phoneRequest.Type
        };
    }

    public async Task<GetRequestResponse> UpdateAsync(UpdateRequestRequest request, Guid requestId)
    {
        if (request.InstitutionId is not null)
        {
            var institution = await _institutionRepository.GetByIdAsync(request.InstitutionId.Value);
            if (institution is null)
            {
                throw new InstitutionNotFoundException();
            }
        }

        var dbRequest = await _requestRepository.GetByIdAsync(requestId);
        if (dbRequest is null)
        {
            throw new RequestNotFoundException();
        }

        if (dbRequest.Status is RequestStatus.Closed or RequestStatus.Canceled or RequestStatus.Merged)
        {
            throw new BadRequestException("Нельзя редактировать закрытую, отменённую или объединённую заявку");
        }

        if (request.InstitutionId is not null &&
            dbRequest.Client.Employee.EmployeeInstitutions.All(ei => ei.InstitutionId != request.InstitutionId.Value))
        {
            throw new ForeignInstitutionException("Заявку нельзя привязать к учреждению, в котором не числится клиент");
        }

        dbRequest.Theme = request.Theme;
        dbRequest.Priority = request.Priority;
        dbRequest.InstitutionId = request.InstitutionId;
        _requestRepository.UpdateAsync(dbRequest);
        await _unitOfWork.SaveChangesAsync();

        return dbRequest.Adapt<GetRequestResponse>();
    }

    public async Task<PagedResponse<GetRequestResponse>> GetAllAsync(int pageNumber, int pageSize, Guid userId)
    {
        if (pageNumber < 1) pageNumber = 1;

        if (pageSize < 1) pageSize = 20;

        if (pageSize > 50) pageSize = 50;

        var requestsInfo = await  _requestRepository.GetAllAsync(pageNumber, pageSize);
        var requests = requestsInfo.Requests.Adapt<List<GetRequestResponse>>();
        await FillUnreadFlagsAsync(requests, userId, includeInternalMessages: true);

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
        await FillUnreadFlagsAsync(requests, userId.Value, includeInternalMessages: false);

        return new PagedResponse<GetRequestResponse>
        {
            Items = requests,
            PageInfo = new PageViewModel(pageNumber, requestsInfo.totalCount, pageSize)
        };
    }

    private async Task FillUnreadFlagsAsync(List<GetRequestResponse> requests, Guid userId, bool includeInternalMessages)
    {
        if (!requests.Any())
        {
            return;
        }

        var unreadFlags = await _requestRepository.GetUnreadFlagsAsync(
            requests.Select(r => r.Id).ToList(),
            userId,
            includeInternalMessages);

        foreach (var request in requests)
        {
            request.IsUnread = unreadFlags.GetValueOrDefault(request.Id);
        }
    }

    public async Task<GetRequestResponse> GetRequestByIdAsync(Guid? id, Guid userId, UserRole userRole)
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

        if (!request.CanBeReadBy(userId, userRole))
        {
            throw new UnacceptableRequestException();
        }

        return request.Adapt<GetRequestResponse>();
    }

    public async Task<GetRequestResponse> MergeAsync(Guid parentRequestId, MergeRequestsRequest request)
    {
        if (Guid.Empty == parentRequestId)
        {
            throw new BadRequestException("ID заявки не может быть пустым");
        }

        if (request.DuplicateRequestIds.Count == 0)
        {
            throw new BadRequestException("Не выбраны заявки для объединения");
        }

        if (request.DuplicateRequestIds.Contains(parentRequestId))
        {
            throw new BadRequestException("Нельзя объединить заявку с самой собой");
        }

        if (request.DuplicateRequestIds.Count != request.DuplicateRequestIds.Distinct().Count())
        {
            throw new BadRequestException("Заявки для объединения выбраны повторно");
        }

        var parent = await _requestRepository.GetByIdForAssignmentAsync(parentRequestId);
        if (parent is null)
        {
            throw new RequestNotFoundException();
        }

        if (parent.Status == RequestStatus.Merged)
        {
            throw new ConflictException("Нельзя объединять заявки в заявку, которая уже объединена");
        }

        var duplicates = new List<DbRequest>();

        foreach (var duplicateId in request.DuplicateRequestIds)
        {
            var duplicate = await _requestRepository.GetByIdForAssignmentAsync(duplicateId);
            if (duplicate is null)
            {
                throw new RequestNotFoundException();
            }

            if (duplicate.Status == RequestStatus.Merged)
            {
                throw new ConflictException("Заявка уже объединена с другой заявкой");
            }

            if (duplicate.ClientId != parent.ClientId)
            {
                throw new ForbiddenException("Нельзя объединять заявки разных клиентов");
            }

            duplicates.Add(duplicate);
        }

        foreach (var duplicate in duplicates)
        {
            parent.InstitutionId ??= duplicate.InstitutionId;

            foreach (var duplicateOperator in duplicate.Operators.ToList())
            {
                duplicate.Operators.Remove(duplicateOperator);

                if (parent.Operators.All(o => o.Id != duplicateOperator.Id))
                {
                    parent.Operators.Add(duplicateOperator);
                }
            }

            duplicate.Status = RequestStatus.Merged;
            duplicate.MergedIntoRequestId = parent.Id;

            await _messageRepository.MoveToRequestAsync(duplicate.Id, parent.Id);
        }

        await _unitOfWork.SaveChangesAsync();

        var mergedRequest = await _requestRepository.GetByIdAsync(parent.Id);
        var response = mergedRequest.Adapt<GetRequestResponse>();
        await _notificationService.NotifyRequestsMergedAsync(response, request.DuplicateRequestIds);

        return response;
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

        if (request.Status == RequestStatus.Merged)
        {
            throw new ConflictException("Нельзя назначать операторов на объединённую заявку");
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

        if (request.Status == RequestStatus.Closed || request.Status == RequestStatus.Canceled || request.Status == RequestStatus.Merged)
        {
            throw new ConflictException("Нельзя удалить оператора с закрытой или объединённой заявки");
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

        var oldStatus = requestCheck.Status;
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

        if (userId != requestCheck.ClientId && oldStatus != requestCheck.Status)
        {
            if (requestCheck.Type == RequestType.Email)
            {
                var statusName = requestCheck.Status switch
                {
                    RequestStatus.InProgress => "В работе",
                    RequestStatus.Closed => "Закрыта",
                    RequestStatus.Canceled => "Отменена",
                    RequestStatus.ClientDataRequest => "Запрошены данные клиента",
                    RequestStatus.PendingReview => "Ожидает проверки",
                    _ => "Новая"
                };
                var email = new EmailMessageRequest
                {
                    ToEmail = requestCheck.Client.Email,
                    Subject = $"Заявка: {requestCheck.Theme}",
                    Message = $"Статус заявки «{requestCheck.Theme}» изменён: {statusName}.",
                    IsPlainText = true,
                    ThreadRequestId = requestId
                };
                BackgroundJob.Enqueue<IEmailService>(x => x.SendEmailAsync(email));
            }
            else
            {
                BackgroundJob.Schedule<IRequestService>(
                    x => x.CheckAndSendEmailAsync(requestId, requestCheck.ClientId, DateTime.UtcNow),
                    TimeSpan.FromMinutes(10));
            }
        }
    }

    public async Task MarkAsViewedAsync(Guid requestId, Guid userId, UserRole userRole)
    {
        var request = await _requestRepository.GetByIdForAssignmentAsync(requestId);
        if (request is null)
        {
            throw new RequestNotFoundException();
        }

        if (!request.CanBeReadBy(userId, userRole))
        {
            throw new UnacceptableRequestException();
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
        {
            throw new UserNotFoundException();
        }

        await _requestRepository.UpsertViewAsync(requestId, userId, DateTime.UtcNow);
    }

    public async Task CheckAndSendEmailAsync(Guid requestId, Guid userId, DateTime eventTime)
    {
        var request = await _requestRepository.GetByIdAsync(requestId);
        if (request is null)
        {
            throw new RequestNotFoundException();
        }
        
        var view = await _requestRepository.GetViewAsync(requestId, userId);
    
        if (view is null || view.LastViewedAt < eventTime)
        {
            var emailContent = _emailTemplateBuilder.BuildNewRequestStatus(request.Theme);
    
            var email = new EmailMessageRequest
            {
                Message = emailContent.Html,
                Subject = emailContent.Subject,
                ToEmail = request.Client.Email,
                ThreadRequestId = requestId
            };
    
            BackgroundJob.Enqueue<IEmailService>(x => x.SendEmailAsync(email));
        }
    }
}
