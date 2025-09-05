using Depot.Auth.Persistence;
using TUnit.Core.Interfaces;

namespace Depot.Auth.Tests.Setup;

public class TestFixture : IAsyncInitializer
{
    public Database<AuthDbContext> Db { get; private set; } = null!;

    public IDistributedCache Cache => GetService<IDistributedCache>();

    public static Task<TestFixture> Instance => CreateAsync();

    public T GetService<T>() where T : notnull
    {
        return Global.GetService<T>();
    }

    public async Task InitializeAsync()
    {
        Db = await Database<AuthDbContext>.Instance;
    }

    private static async Task<TestFixture> CreateAsync()
    {
        var fixture = new TestFixture();

        await fixture.InitializeAsync();

        return fixture;
    }
}