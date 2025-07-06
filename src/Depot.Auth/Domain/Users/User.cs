namespace Depot.Auth.Domain.Users;

using System.Reactive;
using Depot.Auth.Domain.Auth;
using Depot.Auth.Domain.Common;
using Depot.Auth.Domain.Errors;
using Depot.Auth.Domain.Events;
using Depot.Auth.Domain.Interfaces;
using ErrorOr;

public class User : AggregateRoot
{
    public Guid Id { get; set; }


    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Email { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }


    public List<Membership> Memberships { get; set; } = [];

    public List<Token> Tokens { get; set; } = [];


    public static ErrorOr<User> New(string username, Password password, Email email, TimeProvider time)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return Error.Validation();
        }

        return new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            Password = password.Encoded,
            Email = email.Value,
            CreatedAt = time.GetUtcNow()
        };
    }

    public void ChangePassword(string current, Password updated)
    {
        // Check old password

        Password = updated.Encoded;

        Events.Add(new PasswordChangedEvent(this));
    }

    public void ChangeEmail(Email email)
    {
        Email = email.Value;

        Events.Add(new EmailChangedEvent(this));
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
}