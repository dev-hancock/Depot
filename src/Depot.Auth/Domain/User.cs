namespace Depot.Auth.Domain;

using System.Reactive;
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
            Id = Guid.NewGuid(),
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
        var refresh = Tokens.SingleOrDefault(t => t.Id == token.Id);

        if (refresh is null)
        {
            
        }
    }

    public ErrorOr<Unit> RevokeToken(RefreshToken token)
    {
        var refresh = Tokens.SingleOrDefault(t => t.Id == token.Id);

        if (refresh == null)
        {
            return Errors.TokenInvalid(TokenType.Refresh);
        }

        Tokens.Remove(refresh);

        return Unit.Default;
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