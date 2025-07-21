namespace Depot.Auth.Domain.Errors;

using ErrorOr;

public static partial class Errors
{
    public static Error UserNotFound()
    {
        return Error.NotFound("USER_NOT_FOUND", "User not found.");
    }

    public static Error UserAlreadyExists()
    {
        return Error.Conflict("USER_ALREADY_EXISTS", "User already exists.");
    }
}