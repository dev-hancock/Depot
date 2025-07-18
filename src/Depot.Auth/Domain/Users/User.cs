namespace Depot.Auth.Domain.Users;

using Auth;
using Common;
using ErrorOr;
using Events;
using Tenants;

public class User : Entity
{
    private User(string username, Email email, Password password, DateTime now)
    {
        Username = username;
        Email = email;
        Password = password;
        CreatedAt = now;
    }

    public UserId Id { get; set; }

    public string Username { get; }

    public Password Password { get; private set; }

    public Email Email { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public List<Membership> Memberships { get; set; } = [];

    public List<Session> Sessions { get; set; } = [];

    public static User Create(string username, Email email, Password password, DateTime now)
    {
        return new User(username, email, password, now);
    }

    public void AddTenant(Tenant tenant, Role role)
    {
        var membership = new Membership();
    }

    public void AddSession(Session session)
    {
        Sessions.Add(session);
    }

    public void ChangePassword(string current, Password updated)
    {
        Password = updated;

        Events.Add(new PasswordChangedEvent(this));
    }

    public void ChangeEmail(Email email)
    {
        Email = email;

        Events.Add(new EmailChangedEvent(this));
    }

    public Session? FindSession(string token)
    {
        return Sessions.SingleOrDefault(t => t.RefreshToken == token);
    }

    public ErrorOr<Success> Logout(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            Sessions.Clear();
        }
        else
        {
            var session = FindSession(token);

            if (session == null)
            {
                return Error.NotFound();
            }

            Sessions.Remove(session);
        }

        return Result.Success;
    }


    public bool CanCreateOrganisation()
    {
        return true;
    }

    public bool IsAuthenticated(Session session)
    {
        return session.UserId == Id && session.IsValid();
    }
}