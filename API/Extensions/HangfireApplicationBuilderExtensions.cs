using Application.Interfaces;
using Hangfire;

namespace API.Extensions;

public static class HangfireApplicationBuilderExtensions
{
    public static IApplicationBuilder UseHangfireJobs(this IApplicationBuilder app)
    {
        RecurringJob.AddOrUpdate<IAutoCloseStaleRequestsJob>(
            "auto-close-stale-requests",
            service => service.ExecuteAsync(),
            Cron.Hourly()
        );

        return app;
    }
}