using System.Reflection;
using Application.Common.Minio;
using Application.Common.Validators;
using Application.Common.Validators.Interfaces;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Services;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IInstitutionService, InstitutionService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRequestService, RequestService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IAttachmentService, AttachmentService>();
        services.AddScoped<IAutoCloseStaleRequestsJob, AutoCloseStaleRequestsJob>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IWorkplaceValidationService, WorkplaceValidationService>();
        services.Configure<FileUploadOptions>(configuration.GetSection("FileUpload"));
        
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(Assembly.GetExecutingAssembly()); 
        
        return services;
    }
}