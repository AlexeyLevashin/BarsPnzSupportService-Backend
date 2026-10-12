using Domain.Enums;
using Infrastructure.Extensions;
using Application.Extensions;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Hubs;

[Authorize]
public class RequestHub : Hub
{
    private readonly IRequestRepository _requestRepository;

    public RequestHub(IRequestRepository requestRepository)
    {
        _requestRepository = requestRepository;
    }

    public override async Task OnConnectedAsync()
    {
        var userRole = Context.User!.GetUserRole();

        if (userRole == UserRole.Operator || userRole == UserRole.SuperAdmin)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "OperatorsRoom");
        }

        await base.OnConnectedAsync();
    }
    
    public async Task JoinRequestGroup(string requestId, bool hasInternalAccess) 
    {
        if (!Guid.TryParse(requestId, out var id))
        {
            throw new HubException("Некорректный ID заявки");
        }

        var request = await _requestRepository.GetByIdAsync(id);
        if (request is null)
        {
            throw new HubException("Заявка не найдена");
        }

        var user = Context.User!;
        var userRole = user.GetUserRole();
        if (!request.CanBeReadBy(user.GetUserId(), userRole))
        {
            throw new HubException("Нет доступа к заявке");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, $"Request_{id}");

        if (userRole == UserRole.Operator || userRole == UserRole.SuperAdmin)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Request_{id}_Internal");
        }
    }

    public async Task LeaveRequestGroup(string requestId)
    {
        if (!Guid.TryParse(requestId, out var id))
        {
            throw new HubException("Некорректный ID заявки");
        }

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Request_{id}");
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Request_{id}_Internal");
    }
}
