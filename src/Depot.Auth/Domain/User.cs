namespace Depot.Auth.Domain;

using System.Reactive;
using ErrorOr;

public sealed class User
{
    internal User(string username, Password password, DateTimeOffset createdAt)
    {
        Id = Guid.NewGuid();
        Username = username;
        Password = password;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public string Username { get; private set; }

    public Password Password { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public List<UserRole> UserRoles { get; } = [];

    public List<Token> Tokens { get; } = [];

    public static ErrorOr<User> New(string username, Password password, TimeProvider time)
    {
        return new User(username, password, time.GetUtcNow());
    }

    public Session IssueSession(ISecureRandom random, ISecretHasher hasher, TimeProvider time, ITokenGenerator tokens,
        TimeSpan lifetime)
    {
        var now = time.GetUtcNow().UtcDateTime;

        var access = tokens.CreateAccessToken(this, now);

        var refresh = RefreshToken.New(random);

        Tokens.Add(Token.New(
            refresh.Id,
            this,
            TokenType.Refresh,
            refresh.Secret,
            now,
            lifetime,
            hasher
        ));

        return Session.New(access, refresh, now.Add(lifetime));
    }

    public ErrorOr<Session> RefreshSession(RefreshToken token, ISecureRandom random, ISecretHasher hasher, TimeProvider time,
        ITokenGenerator tokens, TimeSpan lifetime)
    {
        var result = RevokeSession(token, hasher, time);

        if (result.IsError)
        {
            return ErrorOr<Session>.From(result.Errors);
        }

        return IssueSession(random, hasher, time, tokens, lifetime);
    }

    public bool HasSession(RefreshToken token)
    {
        return FindToken(token) != null;
    }

    private Token? FindToken(RefreshToken token)
    {
        return Tokens.FirstOrDefault(t => t.Id == token.Id);
    }

    public ErrorOr<Unit> RevokeSession(RefreshToken token, ISecretHasher hasher, TimeProvider time)
    {
        var current = FindToken(token);

        if (current is null)
        {
            return Errors.TokenInvalid(TokenType.Refresh);
        }

        var result = current.Verify(token.Secret, hasher, time);

        if (result.IsError)
        {
            return result;
        }

        Tokens.Remove(current);

        return Unit.Default;
    }

    public ErrorOr<Unit> ClearSessions()
    {
        var count = Tokens.RemoveAll(x => x.Type == TokenType.Refresh);

        if (count == 0)
        {
            return Errors.TokenInvalid(TokenType.Refresh);
        }

        return Unit.Default;
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