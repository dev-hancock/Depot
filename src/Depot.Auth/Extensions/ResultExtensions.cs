namespace Depot.Auth.Extensions;

using ErrorOr;

public static class ResultExtensions
{
    public async static Task<IResult> ToOkAsync<T>(this Task<ErrorOr<T>> result)
    {
        return (await result).Match(Results.Ok, ToProblem);
    }

    public async static Task<IResult> ToCreatedAsync<T>(this Task<ErrorOr<T>> result, Func<T, string> at)
    {
        return (await result).Match(x => Results.Created(at(x), x), ToProblem);
    }

    private static IResult ToProblem(List<Error> errors)
    {
        return errors.ToProblem();
    }
}