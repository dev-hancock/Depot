namespace Depot.Auth.Tests.Data;

using Builders;
using Mappings;

public static class Arrange
{
    public static UserBuilder User => new(Mapping.User);
}