namespace Depot.Auth.Tests.Fixtures.Builders;

using Tests.Data.Builders;

public class ModelBuilder(ApplicationFixture fixture)
{
    public UserBuilder User => new(fixture);
}