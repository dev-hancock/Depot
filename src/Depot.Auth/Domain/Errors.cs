namespace Depot.Auth.Domain;

using ErrorOr;

public class Errors
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