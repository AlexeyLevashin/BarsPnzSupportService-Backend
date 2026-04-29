using Domain.Enums;
using Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Hubs;

[Authorize]
public class RequestHub : Hub
{
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
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Request_{requestId}");
    
        if (hasInternalAccess)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Request_{requestId}_Internal");
        }
    }

    public async Task LeaveRequestGroup(string requestId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Request_{requestId}");
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Request_{requestId}_Internal");
    }
}