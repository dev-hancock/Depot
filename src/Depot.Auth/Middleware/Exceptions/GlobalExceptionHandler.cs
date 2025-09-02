using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace Depot.Auth.Middleware.Exceptions;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken token)
    {
        var status = GetStatusCode(exception);

        var problem = new ProblemDetails
        {
            Status = status,
            Title = ReasonPhrases.GetReasonPhrase(status)
        };

        var env = context.RequestServices.GetService<IWebHostEnvironment>();

        if (env != null && env.IsDevelopment())
        {
            problem.Extensions["exception"] = new
            {
                message = exception.Message,
                trace = exception.StackTrace,
                inner = exception.InnerException?.Message
            };
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = problem.Status.Value;

        await context.Response.WriteAsJsonAsync(problem, token);

        return true;
    }

    private int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            _ => StatusCodes.Status500InternalServerError
        };
    }
}