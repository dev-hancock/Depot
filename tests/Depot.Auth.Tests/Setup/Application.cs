namespace Depot.Auth.Tests.Setup;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

public class Application : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Default", Global.Auth.GetConnectionString());
        builder.UseSetting("ConnectionStrings:Cache", Global.Cache.GetConnectionString());

        builder.ConfigureServices(services =>
        {
            services.AddSingleton(Global.Key);
        });
    }
}