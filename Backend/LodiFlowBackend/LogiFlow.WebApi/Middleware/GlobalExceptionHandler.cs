using LogiFlow.Application.Common.Exceptions;
using LogiFlow.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace LogiFlow.WebApi.Middleware;

public static class GlobalExceptionHandler
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

                var (statusCode, title) = exception switch
                {
                    BadRequestException or DomainRuleException => (StatusCodes.Status400BadRequest, "Bad request"),
                    NotFoundException => (StatusCodes.Status404NotFound, "Not found"),
                    ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
                    _ => (StatusCodes.Status500InternalServerError, "Server error")
                };

                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/problem+json";

                var problem = new ProblemDetails
                {
                    Status = statusCode,
                    Title = title,
                    Detail = exception?.Message ?? "An unexpected error occurred.",
                    Instance = context.Request.Path
                };

                await context.Response.WriteAsJsonAsync(problem);
            });
        });
    }
}
