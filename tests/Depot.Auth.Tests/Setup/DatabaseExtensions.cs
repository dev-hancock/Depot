namespace Depot.Auth.Tests.Setup;

public static partial class Database
{
    public static Task<Session?> FindSessionAsync(string id)
    {
        return FindAsync<Session>(new SessionId(Guid.Parse(id)));
    }

    public static Task<User?> FindUserAsync(string id)
    {
        return FindAsync<User>(new UserId(Guid.Parse(id)));
    }
}