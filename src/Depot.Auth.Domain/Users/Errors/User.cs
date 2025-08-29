using ErrorOr;

namespace Depot.Auth.Domain.Users.Errors;

public static class Errors
{
    public static Error UserAlreadyExists()
    {
        return Error.Conflict("USER_ALREADY_EXISTS", "User already exists.");
    }

    public static Error UserNotFound()
    {
        return Error.NotFound("USER_NOT_FOUND", "User not found.");
    }
}