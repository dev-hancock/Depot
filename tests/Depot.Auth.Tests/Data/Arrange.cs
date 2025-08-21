namespace Depot.Auth.Tests.Data;

using Builders;
using Mappings;

public static class Arrange
{
    public static UserBuilder User => new(Mapping.User);

    public static SessionBuilder Session => new(Mapping.Session);
}

public static class Database { }