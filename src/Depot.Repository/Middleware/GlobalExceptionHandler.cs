using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace Depot.Repository.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IHostEnvironment _environment;

    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken token)
    {
        _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        var status = GetStatusCode(exception);

        var problem = new ProblemDetails
        {
            Title = ReasonPhrases.GetReasonPhrase(status), Status = status
        };

        if (_environment.IsDevelopment())
        {
            problem.Extensions["exception"] = new
            {
                message = exception.Message, trace = exception.StackTrace, inner = exception.InnerException?.Message
            };
        }

        context.Response.StatusCode = problem.Status.Value;

        await context.Response.WriteAsJsonAsync(problem, token);

        return true;
    }

    private int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            KeyNotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };
    }
}