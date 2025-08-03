namespace Depot.Auth.Tests.Factories;

using Data;
using Domain.Interfaces;
using Domain.Users;
using Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Persistence;

public class AuthAppFactory : WebApplicationFactory<Program>
{
    private readonly InfraFixture _fixture;

    public AuthAppFactory(InfraFixture fixture)
    {
        _fixture = fixture;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");
        builder.UseSetting("ConnectionStrings:Auth", _fixture.Auth);
        builder.UseSetting("ConnectionStrings:Cache", _fixture.Cache);

        builder.ConfigureServices(services =>
        {
            SeedDatabase(services.BuildServiceProvider());
        });
    }

    private static void SeedDatabase(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        using var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        context.Database.EnsureDeleted();

        context.Database.EnsureCreated();

        var hasher = scope.ServiceProvider.GetRequiredService<ISecretHasher>();

        var user = User.Create(
            Username.Create(TestData.Username),
            Email.Create(TestData.Email),
            Password.Create(hasher.Hash(TestData.Password)),
            DateTime.UtcNow);

        context.Users.Add(user);

        context.SaveChanges();
    }
}