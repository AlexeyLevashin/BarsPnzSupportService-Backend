using Application.Dto.Messages.Responses;
using Application.Dto.Requests.Responses;

namespace Application.Interfaces.Hubs;

public interface IRequestNotificationService
{
    public Task NotifyNewMessageAsync(Guid requestId, GetMessageResponse response);
    public Task NotifyNewRequestAsync(GetRequestResponse response);
}