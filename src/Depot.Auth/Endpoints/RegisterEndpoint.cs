namespace Depot.Auth.Endpoints;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Extensions;
using Microsoft.AspNetCore.Mvc;
using Services;

public static class RegisterEndpoint
{
    public async static Task<IResult> Handle([FromBody] RegisterRequest request, IUserService users)
    {
        var result = await users.RegisterAsync(request.Username, request.Password, request.Roles);

        return result.Match(
            user => Results.Created("me",
                new RegisterResponse
                {
                    Username = user.Username
                }),
            errors => errors.ToResult());
    }

    public sealed class RegisterRequest
    {
        [JsonPropertyName("username")]
        [Required]
        public string Username { get; init; } = null!;

        [JsonPropertyName("password")]
        [Required]
        public string Password { get; init; } = null!;

        [JsonPropertyName("roles")]
        public string[] Roles { get; init; } = [];
    }

    private sealed class RegisterResponse
    {
        [JsonPropertyName("username")]
        public string Username { get; init; } = null!;
    }
}