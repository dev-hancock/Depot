using Depot.Auth.Domain.Auth;
using Depot.Auth.Domain.Common;
using Depot.Auth.Domain.Tenants;
using Depot.Auth.Domain.Users.Events;
using ErrorOr;

namespace Depot.Auth.Domain.Users;

public class User : Root
{
    // EF Core
    private User() { }

    internal User(UserId id, Username username, Email email, Password password, Session[] sessions, DateTimeOffset createdAt)
    {
        Id = id;
        Username = username;
        Email = email;
        Password = password;
        Sessions = sessions.ToList();
        CreatedAt = createdAt;
    }

    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
    public UserId Id { get; private init; }

    public Username Username { get; private set; } = null!;

    public Password Password { get; private set; } = null!;

    public Email Email { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; private init; }

    public List<Membership> Memberships { get; set; } = [];

    public List<Session> Sessions { get; set; } = [];

    public static User Create(Username username, Email email, Password password, DateTime now)
    {
        return new User(
            new UserId(Guid.NewGuid()),
            username,
            email,
            password,
            [],
            now
        );
    }

    public void AddTenant(Tenant tenant, Role role)
    {
        var membership = new Membership();
    }

    public bool CanCreateOrganisation()
    {
        return true;
    }

    public void ChangeEmail(Email email)
    {
        Email = email;

        Raise(new EmailChangedEvent(this));
    }

    public ErrorOr<Success> ChangePassword(Password updated)
    {
        Password = updated;

        Raise(new PasswordChangedEvent(this));

        return Result.Success;
    }

    public ErrorOr<Session> CreateSession(RefreshToken token)
    {
        if (FindSession(token).Value is { } _)
        {
            return Error.Conflict();
        }

        var session = Session.Create(Id, token);

        Sessions.Add(session);

        Raise(new SessionCreatedEvent(session.Id, session.ExpiresAt));

        return session;
    }

    public ErrorOr<Session> FindSession(string token)
    {
        var session = Sessions.SingleOrDefault(t => t.RefreshToken.Value == token);

        if (session is null)
        {
            return Error.NotFound();
        }

        return session;
    }

    public Session? FindSession(SessionId? id = null)
    {
        return Sessions.SingleOrDefault(t => t.Id == id);
    }

    public ErrorOr<Session> RefreshSession(string current, RefreshToken updated, DateTime now)
    {
        if (FindSession(current).Value is not { } session)
        {
            return Error.NotFound();
        }

        if (!session.IsValid(now))
        {
            return Error.Unauthorized();
        }

        session.Refresh(updated);

        Raise(new SessionRefreshedEvent(session.Id, session.ExpiresAt));

        return session;
    }

    public ErrorOr<Success> RevokeSession(string? token = null)
    {
        var sessions = string.IsNullOrWhiteSpace(token)
            ? Sessions.Where(x => !x.IsRevoked).ToList()
            : Sessions.Where(x => !x.IsRevoked && x.RefreshToken == token).ToList();

        if (sessions.Count == 0)
        {
            return Error.NotFound();
        }

        foreach (var session in sessions)
        {
            session.Revoke();

            Raise(new SessionRevokedEvent(session.Id));
        }

        return Result.Success;
    }
}