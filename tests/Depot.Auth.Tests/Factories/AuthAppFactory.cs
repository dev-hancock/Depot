namespace Depot.Auth.Tests.Factories;

using Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

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
            // TODO: Logging
        });
    }
}