using Depot.Auth.Services;

namespace Depot.Auth.Tests.Setup;

public class UserBuilder
{
    private static readonly Faker Faker = new();

    private readonly ISecretHasher _hasher = Global.GetService<ISecretHasher>();

    private readonly List<string> _roles = [];

    private readonly List<SessionBuilder> _sessions = [];

    public Guid Id { get; private set; } = Faker.Random.Guid();

    public DateTime CreatedAt { get; private set; } = Faker.Date.Past(1, DateTime.UtcNow);

    public string Email { get; private set; } = Faker.Internet.Email();

    public string Password { get; private set; } = Faker.Internet.StrongPassword();

    public string Username { get; private set; } = Faker.Internet.UserName();

    public IReadOnlyList<SessionBuilder> Sessions => _sessions;

    public IReadOnlyList<string> Roles => _roles;

    public User Build()
    {
        return new User(
            new UserId(Id),
            new Username(Username),
            new Email(Email),
            new Password(_hasher.Hash(Password)),
            _sessions.Select(x => x.Build()).ToArray(),
            CreatedAt
        );
    }

    public UserBuilder WithCreatedAt(DateTime timestamp)
    {
        CreatedAt = timestamp;

        return this;
    }

    public UserBuilder WithEmail(string email)
    {
        Email = email;

        return this;
    }

    public UserBuilder WithId(Guid id)
    {
        Id = id;

        return this;
    }

    public UserBuilder WithPassword(string password)
    {
        Password = password;

        return this;
    }

    public UserBuilder WithRole(string role)
    {
        _roles.Add(role);

        return this;
    }

    public UserBuilder WithSession(Action<SessionBuilder>? configure = null)
    {
        var builder = new SessionBuilder(this);

        configure?.Invoke(builder);

        _sessions.Add(builder);

        return this;
    }

    public UserBuilder WithSessions(int count)
    {
        for (var i = 0; i < count; i++)
        {
            _sessions.Add(new SessionBuilder(this));
        }

        return this;
    }

    public UserBuilder WithUsername(string username)
    {
        Username = username;

        return this;
    }
}