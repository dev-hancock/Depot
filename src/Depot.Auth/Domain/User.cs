namespace Depot.Auth.Domain;

using ErrorOr;

public sealed class User
{
    private User()
    {
    }

    public Guid Id { get; init; }

    public string Username { get; private set; } = null!;

    public SecurePassword Password { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; private set; }

    public List<UserRole> UserRoles { get; } = [];

    public List<Token> Tokens { get; } = [];

    public static User New(string username, SecurePassword password, DateTime now)
    {
        return new User
        {
            Username = username,
            Password = password,
            CreatedAt = now
        };
    }

    public Session CreateSession(ISecureRandom random, ISecretHasher hasher, TimeProvider time, ITokenGenerator tokens,
        TimeSpan lifetime)
    {
        var now = time.GetUtcNow().UtcDateTime;

        var access = tokens.CreateAccessToken(this, now);

        var refresh = RefreshToken.New(random);

        Tokens.Add(Token.New(
            refresh.Id,
            this,
            TokenType.Refresh,
            hasher.Hash(refresh.Secret),
            now,
            lifetime
        ));

        return Session.New(access, refresh, now.Add(lifetime));
    }

    public ErrorOr<Session> RefreshSession(RefreshToken token)
    {
        throw new NotImplementedException();
    }

    public void RevokeTokens()
    {
        Tokens.RemoveAll(x => x.Type == TokenType.Refresh);
    }

    public void AssignRoles(IEnumerable<Role> roles)
    {
        foreach (var role in roles)
        {
            if (UserRoles.All(ur => ur.RoleId != role.Id))
            {
                UserRoles.Add(new UserRole
                {
                    User = this,
                    Role = role
                });
            }
        }
    }
}