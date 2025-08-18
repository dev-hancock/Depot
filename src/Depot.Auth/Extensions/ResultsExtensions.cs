namespace Depot.Auth.Extensions;

using ErrorOr;
using Microsoft.AspNetCore.WebUtilities;

public static class ResultsExtensions
{
    public static IResult ToProblem(this List<Error> errors)
    {
        return errors.ToResult();
    }

    public static IResult ToResult(this List<Error> errors)
    {
        if (errors.Count == 0)
        {
            return Results.Problem(
                title: ReasonPhrases.GetReasonPhrase(500),
                statusCode: 500);
        }

        if (errors.All(x => x.Type == ErrorType.Validation))
        {
            return Results.ValidationProblem(
                errors
                    .GroupBy(x => x.Code)
                    .ToDictionary(
                        x => x.Key,
                        x => x.Select(y => y.Description).ToArray()));
        }

        var error = errors.First();

        var status = GetStatusCode(error.Type);

        return Results.Problem(
            title: ReasonPhrases.GetReasonPhrase(status),
            statusCode: status,
            detail: error.Description);
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
}