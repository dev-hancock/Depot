using Microsoft.EntityFrameworkCore;

namespace Depot.Auth.Tests.Setup;

public static partial class DatabaseExtensions
{
    public static Task<Session?> FindSessionAsync<T>(this Database<T> db, Guid id) where T : DbContext
    {
        return db.With(context =>
        {
            return context.Set<Session>()
                .Where(x => x.Id == new SessionId(id))
                .FirstOrDefaultAsync();
        });
    }

    public static Task<User?> FindUserAsync<T>(this Database<T> db, Guid id) where T : DbContext
    {
        return db.With(context =>
        {
            return context.Set<User>()
                .Include(x => x.Sessions)
                .Where(x => x.Id == new UserId(id))
                .FirstOrDefaultAsync();
        });
    }
}