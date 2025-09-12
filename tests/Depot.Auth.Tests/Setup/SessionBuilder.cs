namespace Depot.Auth.Tests.Setup;

public class SessionBuilder
{
    private static readonly Faker Faker = new();

    private Guid? _id;

    public Guid Id => _id ?? Faker.Random.Guid();

    public Guid UserId { get; private set; }

    public DateTime Expiry { get; private set; } = Faker.Date.Soon(1, DateTime.UtcNow);

    public bool IsRevoked { get; private set; }

    public int Version { get; private set; } = 1;

    public string RefreshToken { get; private set; } = Faker.Random.AlphaNumeric(32);

    public Session Build()
    {
        return new Session(
            new SessionId(Id),
            new UserId(UserId),
            new RefreshToken(RefreshToken, Expiry),
            IsRevoked,
            Version
        );
    }

    public SessionBuilder WithUser(Guid id)
    {
        UserId = id;

        return this;
    }

    public SessionBuilder WithExpiry(DateTime expiry)
    {
        Expiry = expiry;

        return this;
    }

    public SessionBuilder WithId(Guid id)
    {
        _id = id;

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