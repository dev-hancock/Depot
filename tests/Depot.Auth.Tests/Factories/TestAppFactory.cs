namespace Depot.Auth.Tests.Factories;

using Features.Auth.Login;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

public class TestAppFactory : WebApplicationFactory<Program>
{
    private readonly IntegrationFixture _fixture;

    public TestAppFactory(IntegrationFixture fixture)
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