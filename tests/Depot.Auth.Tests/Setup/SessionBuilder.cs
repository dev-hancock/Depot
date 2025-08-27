namespace Depot.Auth.Tests.Setup;

using Bogus;
using Domain.Auth;

public class SessionBuilder(UserBuilder user)
{
    private static readonly Faker Faker = new();

    public Guid Id { get; } = Faker.Random.Guid();

    public DateTime Expiry { get; private set; } = Faker.Date.Soon(1, DateTime.UtcNow);

    public bool IsRevoked { get; private set; }

    public string RefreshToken { get; private set; } = Faker.Random.AlphaNumeric(32);

    public Session Build()
    {
        return new Session(
            new SessionId(Id),
            new UserId(user.Id),
            new RefreshToken(RefreshToken, Expiry),
            IsRevoked
        );
    }

    public SessionBuilder WithRevoked()
    {
        IsRevoked = true;

        return this;
    }

    public SessionBuilder WithExpiry(DateTime expiry)
    {
        Expiry = expiry;

        return this;
    }

    public SessionBuilder WithRefreshToken(string token)
    {
        RefreshToken = token;

        return this;
    }
}