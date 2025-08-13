namespace Depot.Auth.Tests.Features.Auth.Login;

using System.Net;
using System.Net.Http.Json;
using Data;
using Depot.Auth.Features.Auth.Login;
using Factories;
using Fixtures;

public sealed class LoginPasswordTests : IClassFixture<InfraFixture>
{
    private readonly HttpClient _client;

    private readonly TestAppFactory _factory;

    public LoginPasswordTests(InfraFixture fixture)
    {
        _factory = new TestAppFactory(fixture);

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithExactPassword_ShouldReturnSession()
    {
        var user = await Arrange.User.SeedAsync(_factory.Services);

        var payload = new LoginCommand
        {
            Username = user.Username,
            Email = user.Email,
            Password = user.Password
        };

        var result = await _client.PostAsJsonAsync("/auth/login", payload);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(session);
    }

    [Fact]
    public async Task Login_WithOnlyPassword_ShouldReturnBadRequest()
    {
        var user = await Arrange.User.SeedAsync(_factory.Services);

        var payload = new LoginCommand
        {
            Password = user.Password
        };

        var result = await _client.PostAsJsonAsync("api/v1/auth/login", payload);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.Null(session);
    }

    [Fact]
    public async Task Login_WithPaddedPassword_ShouldReturnBadRequest()
    {
        var user = await Arrange.User.SeedAsync(_factory.Services);

        var payload = new LoginCommand
        {
            Username = user.Username,
            Email = user.Email,
            Password = $" {user.Password} "
        };

        var result = await _client.PostAsJsonAsync("/auth/login", payload);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.Null(session);
    }

    [Theory]
    [InlineData("")]
    [InlineData("password")]
    [InlineData(null)]
    public async Task Login_WithInvalidPassword_ShouldReturnBadRequest(string? password)
    {
        var user = await Arrange.User.SeedAsync(_factory.Services);

        var payload = new LoginCommand
        {
            Username = user.Username,
            Email = user.Email,
            Password = password!
        };

        var result = await _client.PostAsJsonAsync("/auth/login", payload);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.Null(session);
    }
}