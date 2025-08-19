namespace Depot.Auth.Middleware.Exceptions;

using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

public class ValidationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken token)
    {
        if (exception is not ValidationException ex)
        {
            return false;
        }

        var problem = new ProblemDetails
        {
            Title = ReasonPhrases.GetReasonPhrase(StatusCodes.Status400BadRequest),
            Status = StatusCodes.Status400BadRequest,
            Detail = "One or more validation errors occurred.",
            Extensions =
            {
                ["errors"] = ex.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(
                        x => x.Key,
                        x => x.Select(y => y.ErrorMessage).ToArray())
            }
        };

        context.Response.StatusCode = problem.Status.Value;

        await context.Response.WriteAsJsonAsync(problem, token);

        return true;
    }
}