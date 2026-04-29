using API.Middlewares;
using API.Extensions;
using Infrastructure.Extensions;
using Persistence.Extensions;
using Application.Extensions;
using Infrastructure.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCustomControllers();
builder.Services.AddSwaggerConfiguration();

builder.Services.AddApplication();
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

app.MapControllers();
app.MapHub<RequestHub>("/hubs/requests");
app.Run();
