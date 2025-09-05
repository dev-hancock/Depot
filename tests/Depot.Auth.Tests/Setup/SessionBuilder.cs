namespace Depot.Auth.Tests.Setup;

public class SessionBuilder(UserBuilder user)
{
    private static readonly Faker Faker = new();

    public Guid Id { get; private set; } = Faker.Random.Guid();

    public DateTime Expiry { get; private set; } = Faker.Date.Soon(1, DateTime.UtcNow);

    public bool IsRevoked { get; private set; }

    public int Version { get; private set; } = 1;

    public string RefreshToken { get; private set; } = Faker.Random.AlphaNumeric(32);

    public Session Build()
    {
        return new Session(
            new SessionId(Id),
            new UserId(user.Id),
            new RefreshToken(RefreshToken, Expiry),
            IsRevoked,
            Version
        );
    }


    public SessionBuilder WithExpiry(DateTime expiry)
    {
        Expiry = expiry;

        return this;
    }

    public SessionBuilder WithId(Guid id)
    {
        Id = id;

        return this;
    }

    public SessionBuilder WithRefreshToken(string token)
    {
        RefreshToken = token;

        return this;
    }

    public SessionBuilder WithRevoked(bool revoked = true)
    {
        IsRevoked = revoked;
        Version = 2;

        return this;
    }
}