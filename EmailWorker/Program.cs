using Application.Interfaces;
using Application.Services;
using Application.Interfaces.Hubs;
using Domain.Interfaces;
using EmailWorker;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Infrastructure.Common;
using Infrastructure.Extensions;
using Persistence.Extensions;
using Mapster;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Application.Common.Minio;
using Application.Interfaces.Repositories;

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.Configure<ImapOptions>(builder.Configuration.GetSection("ImapSettings"));
builder.Services.Configure<FileUploadOptions>(builder.Configuration.GetSection("FileUpload"));
builder.Services.AddMinio(builder.Configuration);
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(
        builder.Configuration.GetConnectionString("DefaultConnection"))));
builder.Services.AddScoped<IInboundEmailService, InboundEmailService>();
TypeAdapterConfig.GlobalSettings.Scan(typeof(InboundEmailService).Assembly);
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IRequestRepository, RequestRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IInboundEmailRepository, InboundEmailRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IMinioFileStorageService, MinioFileStorageService>();
builder.Services.AddScoped<IRequestNotificationService, WorkerRequestNotifier>();
builder.Services.AddHostedService<MailWorker>();

await builder.Build().RunAsync();
