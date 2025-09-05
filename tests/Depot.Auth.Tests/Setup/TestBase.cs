namespace Depot.Auth.Tests.Setup;

public abstract class TestBase
{
    [ClassDataSource<TestFixture>]
    public required TestFixture Fixture { get; set; } = null!;
}