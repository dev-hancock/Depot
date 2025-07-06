namespace Depot.Auth.Domain.Errors;

using Auth;
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