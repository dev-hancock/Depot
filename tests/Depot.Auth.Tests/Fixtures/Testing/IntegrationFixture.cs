namespace Depot.Auth.Tests.Fixtures.Testing;

using Bogus;
using Microsoft.Extensions.Caching.Distributed;

public class IntegrationFixture : FixtureProvider
{
    public readonly Faker Faker = new();

    [ClassDataSource<ApplicationFixture>(Shared = SharedType.PerTestSession)]
    public required ApplicationFixture Application { get; set; } = null!;

    [ClassDataSource<DatabaseFixture>(Shared = SharedType.PerTestSession)]
    public required DatabaseFixture Database { get; set; } = null!;

    public RequestFixture Client => Get<RequestFixture>();

    public ModelBuilder Arrange => Get<ModelBuilder>();

    public IDistributedCache Cache => Get<IDistributedCache>();

    protected override void Configure()
    {
        // Add(() => new DatabaseFixture(Application));
        Add(() => new RequestFixture(Application));
        Add(() => new ModelBuilder(Application));
        Add(() => Application.GetService<IDistributedCache>());
    }
}