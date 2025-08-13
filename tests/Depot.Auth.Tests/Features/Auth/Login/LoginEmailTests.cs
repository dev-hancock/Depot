namespace Depot.Auth.Tests.Features.Auth.Login;

using System.Net;
using System.Net.Http.Json;
using Data;
using Depot.Auth.Features.Auth.Login;
using Factories;
using Fixtures;

public sealed class LoginEmailTests : IClassFixture<InfraFixture>
{
    private readonly HttpClient _client;

    private readonly TestAppFactory _factory;

    public LoginEmailTests(InfraFixture fixture)
    {
        _factory = new TestAppFactory(fixture);

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithExactEmail_ShouldReturnSession()
    {
        var user = await Arrange.User().SeedAsync(_factory.Services);

        var payload = new LoginCommand
        {
            Email = user.Email,
            Password = user.Password
        };

        var result = await _client.PostAsJsonAsync("/auth/login", payload);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(session);
    }

    [Fact]
    public async Task Login_WithOnlyEmail_ShouldReturnBadRequest()
    {
        var user = await Arrange.User().SeedAsync(_factory.Services);

        var payload = new LoginCommand
        {
            Email = user.Email
        };

        var result = await _client.PostAsJsonAsync("api/v1/auth/login", payload);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.Null(session);
    }

    [Fact]
    public async Task Login_WithUpperCaseEmail_ShouldReturnSession()
    {
        var user = await Arrange.User().SeedAsync(_factory.Services);

        var payload = new LoginCommand
        {
            Email = user.Email.ToUpperInvariant(),
            Password = user.Password
        };

        var result = await _client.PostAsJsonAsync("/auth/login", payload);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(session);
    }

    [Fact]
    public async Task Login_WithLowerCaseEmail_ShouldReturnSession()
    {
        var user = await Arrange.User().SeedAsync(_factory.Services);

        var payload = new LoginCommand
        {
            Email = user.Email.ToLowerInvariant(),
            Password = user.Password
        };

        var result = await _client.PostAsJsonAsync("/auth/login", payload);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(session);
    }

    [Fact]
    public async Task Login_WithPaddedEmail_ShouldReturnSession()
    {
        var user = await Arrange.User().SeedAsync(_factory.Services);

        var payload = new LoginCommand
        {
            Email = $" {user.Email} ",
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
    public async Task Login_WithInvalidEmail_ShouldReturnBadRequest(string? email)
    {
        var user = await Arrange.User().SeedAsync(_factory.Services);

        var payload = new LoginCommand
        {
            Email = email,
            Password = user.Password
        };

        var result = await _client.PostAsJsonAsync("/auth/login", payload);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);

        var session = await result.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.Null(session);
    }
}