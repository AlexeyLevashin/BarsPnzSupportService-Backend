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
    
    public async Task NotifyNewMessageAsync(Guid requestId, Guid clientId, GetMessageResponse response)
    {
        string targetGroup = response.Type == MessageType.Internal 
            ? $"Request_{requestId}_Internal" 
            : $"Request_{requestId}";   
        
        await _hub.Clients.Group(targetGroup).SendAsync("ReceiveMessage", response);

        var activity = new { requestId, senderId = response.SenderId };
        await _hub.Clients.Group("OperatorsRoom").SendAsync("RequestActivity", activity);

        if (response.Type == MessageType.Public)
        {
            await _hub.Clients.Group($"User_{clientId}").SendAsync("RequestActivity", activity);
        }

    }

    public async Task NotifyNewRequestAsync(GetRequestResponse response)
    {
        await _hub.Clients.Group("OperatorsRoom").SendAsync("NewRequestCreated", response);
    }

    public async Task NotifyRequestsMergedAsync(GetRequestResponse parentRequest, List<Guid> duplicateRequestIds)
    {
        var notification = new
        {
            parentRequestId = parentRequest.Id,
            duplicateRequestIds,
            parentRequest
        };

        foreach (var duplicateRequestId in duplicateRequestIds)
        {
            await _hub.Clients.Group($"Request_{duplicateRequestId}")
                .SendAsync("RequestMerged", notification);
        }

        await _hub.Clients.Group($"Request_{parentRequest.Id}")
            .SendAsync("RequestMerged", notification);
        await _hub.Clients.Group("OperatorsRoom")
            .SendAsync("RequestMerged", notification);
    }
}
