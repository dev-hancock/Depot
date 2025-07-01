namespace Depot.Auth.Endpoints;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Extensions;
using Services;

public static class LoginEndpoint
{
    public async static Task<IResult> Handle(LoginRequest request, IUserService users, ITokenService tokens, HttpContext context)
    {
        var user = await users.LoginAsync(request.Username, request.Password, context.RequestAborted);

        if (user.IsError)
        {
            return user.Errors.ToResult();
        }

        var result = await tokens.IssueTokenAsync(user.Value, context.RequestAborted);

        return result
            .Match(
                token => Results.Ok(new LoginResponse
                {
                    AccessToken = token.Access,
                    RefreshToken = token.Refresh.Token
                }),
                errors => errors.ToResult());
    }

    public sealed class LoginRequest
    {
        [Required]
        [JsonPropertyName("username")]
        public string Username { get; init; } = null!;

        [Required]
        [JsonPropertyName("password")]
        public string Password { get; init; } = null!;
    }

    public sealed class LoginResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; init; } = null!;

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; init; } = null!;
    }
}