namespace Depot.Auth.Extensions;

using ErrorOr;

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
            return Results.InternalServerError();
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

        return error.Type switch
        {
            ErrorType.NotFound => Results.NotFound(),
            ErrorType.Unauthorized => Results.Unauthorized(),
            ErrorType.Forbidden => Results.Forbid(),
            ErrorType.Conflict => Results.Conflict(
                new
                {
                    error = error.Description
                }),
            _ => Results.Problem(
                title: error.Description,
                statusCode: 500)
        };
    }
}