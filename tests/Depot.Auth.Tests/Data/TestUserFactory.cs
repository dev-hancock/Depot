namespace Depot.Auth.Tests.Data;

using Bogus;
using Extensions;

public class TestUserFactory
{
    private static readonly Faker Faker = new();

    public static TestUser CreateUser()
    {
        return new TestUser(
            Guid.NewGuid(),
            Faker.Internet.UserName(),
            Faker.Internet.Email(),
            Faker.Internet.StrongPassword(),
            Faker.Date.Past(1, DateTime.UtcNow));
    }
}