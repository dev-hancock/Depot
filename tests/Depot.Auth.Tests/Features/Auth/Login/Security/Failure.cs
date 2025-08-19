namespace Depot.Auth.Tests.Features.Auth.Login.Security;

using System.Net;
using System.Net.Http.Json;
using Data;
using Data.Extensions;
using Depot.Auth.Features.Auth.Login;
using Microsoft.AspNetCore.Mvc;

public class Failure(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Login_WithWrongUsername_ShouldReturnUnauthorized()
    {
        var user = await Arrange.User.SeedAsync(Services);

        var payload = new LoginCommand
        {
            Username = Faker.Internet.UserName(),
            Password = user.Password
        };

        var result = await Client.PostAsJsonAsync("api/v1/auth/login", payload);

        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);

        var content = await result.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(content);
    }

    [Fact]
    public async Task Login_WithWrongEmail_ShouldReturnUnauthorized()
    {
        var user = await Arrange.User.SeedAsync(Services);

        var payload = new LoginCommand
        {
            Email = Faker.Internet.Email(),
            Password = user.Password
        };

        var result = await Client.PostAsJsonAsync("api/v1/auth/login", payload);

        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);

        var content = await result.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(content);
    }

    [Fact]
    public async Task Login_WithWrongPassword_UsingEmail_ShouldReturnUnauthorized()
    {
        var user = await Arrange.User.SeedAsync(Services);

        var payload = new LoginCommand
        {
            Email = user.Email,
            Password = Faker.Internet.StrongPassword()
        };

        var result = await Client.PostAsJsonAsync("api/v1/auth/login", payload);

        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);

        var content = await result.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(content);
    }

    [Fact]
    public async Task Login_WithWrongPassword_UsingUsername_ShouldReturnUnauthorized()
    {
        var user = await Arrange.User.SeedAsync(Services);

        var payload = new LoginCommand
        {
            Username = user.Username,
            Password = Faker.Internet.StrongPassword()
        };

        var result = await Client.PostAsJsonAsync("api/v1/auth/login", payload);

        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);

        var content = await result.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(content);
    }

    [Fact]
    public async Task Login_WithPaddedPassword_ShouldReturnUnauthorized()
    {
        var user = await Arrange.User.SeedAsync(Services);

        var payload = new LoginCommand
        {
            Username = user.Username,
            Email = user.Email,
            Password = $" {user.Password} "
        };

        var result = await Client.PostAsJsonAsync("api/v1/auth/login", payload);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);

        var content = await result.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(content);
    }
}