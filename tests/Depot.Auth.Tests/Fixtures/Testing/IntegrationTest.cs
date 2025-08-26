namespace Depot.Auth.Tests.Fixtures.Testing;

public abstract class IntegrationTest
{
    [ClassDataSource<IntegrationFixture>]
    public required IntegrationFixture Fixture { get; set; } = null!;
}