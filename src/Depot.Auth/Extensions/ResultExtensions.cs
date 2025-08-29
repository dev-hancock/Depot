using ErrorOr;

namespace Depot.Auth.Extensions;

public static class ResultExtensions
{
    public static async Task<IResult> ToCreatedAsync<T>(this Task<ErrorOr<T>> result, Func<T, string> at)
    {
        return (await result).Match(x => Results.Created(at(x), x), ToProblem);
    }

    public static async Task<IResult> ToNoContentAsync<T>(this Task<ErrorOr<T>> result)
    {
        return (await result).Match(_ => Results.NoContent(), ToProblem);
    }

    public static async Task<IResult> ToOkAsync<T>(this Task<ErrorOr<T>> result)
    {
        return (await result).Match(Results.Ok, ToProblem);
    }

    private static IResult ToProblem(List<Error> errors)
    {
        return errors.ToResult();
    }
}