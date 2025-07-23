namespace Depot.Auth.Domain.Users;

using Auth;
using Common;
using ErrorOr;
using Events;
using Tenants;

public class User : Root
{
    private User()
    {
    }

    public UserId Id { get; set; }

    public string Username { get; private init; }

    public Password Password { get; private set; }

    public Email Email { get; private set; }

    public DateTimeOffset CreatedAt { get; private init; }

    public List<Membership> Memberships { get; set; } = [];

    public List<Session> Sessions { get; set; } = [];

    public static User Create(string username, Email email, Password password, DateTime now)
    {
        return new User
        {
            Id = UserId.Next(),
            Username = username,
            Email = email,
            Password = password,
            CreatedAt = now
        };
    }

    public void AddTenant(Tenant tenant, Role role)
    {
        var membership = new Membership();
    }

    public ErrorOr<Session> CreateSession(RefreshToken token)
    {
        if (FindSession(token) is not null)
        {
            return Error.Conflict();
        }

        var session = Session.Create(Id, token);

        Sessions.Add(session);

        Raise(new SessionCreatedEvent(session.Id, session.ExpiresAt));

        return session;
    }

    public ErrorOr<Success> RefreshSession(RefreshToken token, DateTime now)
    {
        if (FindSession(token) is not { } session)
        {
            return Error.NotFound();
        }

        if (!session.IsValid(now))
        {
            return Error.Unauthorized();
        }

        var index = Sessions.IndexOf(session);

        Sessions[index] = session.Refresh(token);

        Raise(new SessionRefreshedEvent(session.Id, session.ExpiresAt));

        return Result.Success;
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
            var index = Sessions.IndexOf(session);

            Sessions[index] = session.Revoke();

            Raise(new SessionRevokedEvent(session.Id));
        }

        return Result.Success;
    }

    public ErrorOr<Success> ChangePassword(Password updated)
    {
        Password = updated;

        Raise(new PasswordChangedEvent(this));

        return Result.Success;
    }

    public void ChangeEmail(Email email)
    {
        Email = email;

        Raise(new EmailChangedEvent(this));
    }

    public Session? FindSession(string token)
    {
        return Sessions.SingleOrDefault(t => t.RefreshToken == token);
    }

    public Session? FindSession(SessionId? id = null)
    {
        return Sessions.SingleOrDefault(t => t.Id == id);
    }

    public bool CanCreateOrganisation()
    {
        return true;
    }
}