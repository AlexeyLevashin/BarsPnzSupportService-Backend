using Application.Dto.Messages.Responses;
using Application.Dto.Requests.Responses;
using Application.Interfaces.Hubs;
using Hangfire;

namespace EmailWorker;

public class WorkerRequestNotifier : IRequestNotificationService
{
    private readonly IBackgroundJobClient _jobs;

    public WorkerRequestNotifier(IBackgroundJobClient jobs)
    {
        _jobs = jobs;
    }

    public Task NotifyNewMessageAsync(Guid requestId, GetMessageResponse response)
    {
        _jobs.Enqueue<IRequestNotificationService>(x => x.NotifyNewMessageAsync(requestId, response));
        return Task.CompletedTask;
    }

    public Task NotifyNewRequestAsync(GetRequestResponse response)
    {
        _jobs.Enqueue<IRequestNotificationService>(x => x.NotifyNewRequestAsync(response));
        return Task.CompletedTask;
    }
}
