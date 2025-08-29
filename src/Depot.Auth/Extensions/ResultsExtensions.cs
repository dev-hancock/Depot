using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace Depot.Auth.Extensions;

public static class ResultsExtensions
{
    private static readonly int BadRequest = StatusCodes.Status400BadRequest;

    public static IResult ToResult(this List<Error> errors)
    {
        if (errors.Count == 0)
        {
            return errors.ToInternalServerError();
        }

        if (errors.All(x => x.Type == ErrorType.Validation))
        {
            return errors.ToBadRequest();
        }

        return errors.ToProblem();
    }

    private static int GetStatusCode(ErrorType type)
    {
        return type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };
    }


    private static IResult ToBadRequest(this List<Error> errors)
    {
        var problem = new ProblemDetails
        {
            Title = ReasonPhrases.GetReasonPhrase(BadRequest),
            Status = BadRequest,
            Detail = "One or more validation errors occurred.",
            Extensions =
            {
                ["errors"] = errors
                    .GroupBy(x => x.Code)
                    .ToDictionary(
                        x => x.Key,
                        x => x.Select(y => y.Description).ToArray())
            }
        };

        return Results.Json(problem);
    }

    private static IResult ToInternalServerError(this List<Error> _)
    {
        return Results.Problem(
            title: ReasonPhrases.GetReasonPhrase(500),
            statusCode: 500);
    }

    private static IResult ToProblem(this List<Error> errors)
    {
        var error = errors.First();

        var status = GetStatusCode(error.Type);

        return Results.Problem(
            title: ReasonPhrases.GetReasonPhrase(status),
            statusCode: status,
            detail: error.Description);
    }
}