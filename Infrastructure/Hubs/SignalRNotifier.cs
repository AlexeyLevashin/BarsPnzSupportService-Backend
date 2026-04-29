using Application.Dto.Messages.Responses;
using Application.Dto.Requests.Responses;
using Application.Interfaces.Hubs;
using Domain.Enums;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Hubs;

public class SignalRNotifier : IRequestNotificationService
{
    private readonly IHubContext<RequestHub> _hub;

    public SignalRNotifier(IHubContext<RequestHub> hub)
    {
        _hub = hub;
    }
    
    public async Task NotifyNewMessageAsync(Guid requestId, GetMessageResponse response)
    {
        string targetGroup = response.Type == MessageType.Internal 
            ? $"Request_{requestId}_Internal" 
            : $"Request_{requestId}";   
        
        await _hub.Clients.Group(targetGroup).SendAsync("ReceiveMessage", response);
    }

    public async Task NotifyNewRequestAsync(GetRequestResponse response)
    {
        await _hub.Clients.Group("OperatorsRoom").SendAsync("NewRequestCreated", response);
    }
}