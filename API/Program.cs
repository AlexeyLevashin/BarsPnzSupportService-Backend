using API.Middlewares;
using API.Extensions;
using Infrastructure.Extensions;
using Persistence.Extensions;
using Application.Extensions;
using Hangfire;
using Infrastructure.Hubs;
using Minio;
using Minio.DataModel.Args;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCustomControllers();
builder.Services.AddSwaggerConfiguration();
builder.Services.AddHangfireConfiguration(builder.Configuration);

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPersistence(builder.Configuration);

builder.Services.AddTransient<ExceptionHandlingMiddlewares>();

builder.Services.AddSignalR();

var app = builder.Build();

await app.UseDbInitializer();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BarsPnz API v1"));
    // app.UseCors("AllowLocalFront"); // Раскомментируешь, когда понадобится CORS
}

app.UseMiddleware<ExceptionHandlingMiddlewares>();

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard();

app.MapControllers();
app.MapHub<RequestHub>("/hubs/requests");

using (var scope = app.Services.CreateScope())
{
    var minioClient = scope.ServiceProvider.GetRequiredService<IMinioClient>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    var bucketName = "attachments";
    
    try 
    {
        bool found = await minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName));
        if (!found)
        {
            await minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName));
            logger.LogInformation($"Бакет '{bucketName}' успешно создан в MinIO!");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, $"Ошибка при инициализации бакета '{bucketName}' в MinIO.");
    }
}

app.UseHangfireJobs();

app.Run();

//todo добавить .HasFilter("\"Status\" IN (2, 3)"); в modelBuilder.Entity<DbRequest> в классе ApplicationContext, когда добавится история изменения статусов