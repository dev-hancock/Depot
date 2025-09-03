using Microsoft.EntityFrameworkCore;

namespace Depot.Auth.Tests.Setup;

public static partial class Database
{
    public static Task<Session?> FindSessionAsync(Guid id)
    {
        return FindAsync<Session>(new SessionId(id));
    }

    public static Task<User?> FindUserAsync(string id)
    {
        return FindUserAsync(new UserId(Guid.Parse(id)));
    }

    public static Task<User?> FindUserAsync(UserId id)
    {
        return WithScope<User?>(context =>
            context.Set<User>()
                .Include(x => x.Sessions)
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync()
        );
    }
}