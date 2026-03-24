using Application.Exceptions.Abstractions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace API.Middlewares;

public sealed class ExceptionHandlingMiddlewares : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception e)
        {
            await ExceptionHandling(context, e);
        }
    }

    public async Task ExceptionHandling(HttpContext context, Exception e)
    {
        context.Response.StatusCode = e switch
        {
            BadRequestException => StatusCodes.Status400BadRequest,
            UnauthorizedException => StatusCodes.Status401Unauthorized,
            ForbiddenException => StatusCodes.Status403Forbidden,
            NotFoundException => StatusCodes.Status404NotFound,
            ConflictException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        await context.Response.WriteAsJsonAsync(new {error = e.Message});
    }
}