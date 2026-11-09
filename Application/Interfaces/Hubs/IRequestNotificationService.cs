using Application.Dto.Messages.Responses;
using Application.Dto.Requests.Responses;

namespace Application.Interfaces.Hubs;

public interface IRequestNotificationService
{
    public Task NotifyNewMessageAsync(Guid requestId, Guid clientId, GetMessageResponse response);
    public Task NotifyNewRequestAsync(GetRequestResponse response);
    public Task NotifyRequestsMergedAsync(GetRequestResponse parentRequest, List<Guid> duplicateRequestIds);
}
