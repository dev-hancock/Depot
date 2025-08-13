namespace Depot.Auth.Tests.Features.Auth.Login;

using System.Net;
using System.Net.Http.Json;
using Data;
using Depot.Auth.Features.Auth.Login;
using Factories;
using Fixtures;

public sealed class LoginUsernameTests : IClassFixture<InfraFixture>
{
    private readonly HttpClient _client;

    private readonly TestAppFactory _factory;

    public LoginUsernameTests(InfraFixture fixture)
    {
        _factory = new TestAppFactory(fixture);

        _client = _factory.CreateClient();
    }


    [Fact]
    public async Task Login_WithExactUsername_ShouldReturnSession()
    {
        var user = await Arrange.User().SeedAsync(_factory.Services);

        var payload = new LoginCommand
        {
            Username = user.Username,
            Password = user.Password
        };

        var result = await _client.PostAsJsonAsync("/auth/login", payload);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(session);
    }


    [Fact]
    public async Task Login_WithOnlyUsername_ShouldReturnBadRequest()
    {
        var user = await Arrange.User().SeedAsync(_factory.Services);

        var payload = new LoginCommand
        {
            Username = user.Username
        };

        var result = await _client.PostAsJsonAsync("api/v1/auth/login", payload);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.Null(session);
    }

    [Fact]
    public async Task Login_WithUpperCaseUsername_ShouldReturnSession()
    {
        var user = await Arrange.User().SeedAsync(_factory.Services);

        var payload = new LoginCommand
        {
            Username = user.Username.ToUpperInvariant(),
            Password = user.Password
        };

        var result = await _client.PostAsJsonAsync("/auth/login", payload);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(session);
    }

    [Fact]
    public async Task Login_WithLowerCaseUsername_ShouldReturnSession()
    {
        var user = await Arrange.User().SeedAsync(_factory.Services);

        var payload = new LoginCommand
        {
            Username = user.Username.ToLowerInvariant(),
            Password = user.Password
        };

        var result = await _client.PostAsJsonAsync("/auth/login", payload);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(session);
    }

    [Fact]
    public async Task Login_WithPaddedUsername_ShouldReturnSession()
    {
        var user = await Arrange.User().SeedAsync(_factory.Services);

        var payload = new LoginCommand
        {
            Username = $" {user.Username} ",
            Password = user.Password
        };

        var result = await _client.PostAsJsonAsync("/auth/login", payload);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(session);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("")]
    [InlineData(null)]
    public async Task Login_WithInvalidUsername_ShouldReturnBadRequest(string? username)
    {
        var user = await Arrange.User().SeedAsync(_factory.Services);

        var payload = new LoginCommand
        {
            Username = username,
            Password = user.Password
        };

        var result = await _client.PostAsJsonAsync("/auth/login", payload);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.Null(session);
    }
}