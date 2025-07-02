namespace Depot.Auth.Domain;

using ErrorOr;

public static partial class Errors
{
    public static Error TokenInvalid(TokenType type)
    {
        return type switch
        {
            TokenType.Refresh => Error.Unauthorized("TOKEN_INVALID", "The  token is invalid."),
            _ => throw new NotImplementedException()
        };
    }
}

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