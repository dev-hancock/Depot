namespace Depot.Auth.Tests.Data;

using Builders;

public static class Arrange
{
    public static UserSeeder User(Action<UserBuilder>? configure = null)
    {
        var builder = new UserBuilder();

        configure?.Invoke(builder);

        return new UserSeeder(builder);
    }
}