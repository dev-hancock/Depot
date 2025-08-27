namespace Depot.Auth.Tests.Setup;

using Microsoft.EntityFrameworkCore;

public class ScopedDatabase : IDisposable
{
    private readonly DbContext _context;

    private readonly Transaction _transaction;

    public ScopedDatabase(DbContext context)
    {
        _context = context;
        // _context = Scoped.Service<AuthDbContext>();

        _transaction = new Transaction(context);
    }

    public void Dispose()
    {
        _transaction.Dispose();
        _context.Dispose();
    }

    public async Task SeedAsync(params object[] models)
    {
        _context.AddRange(models);

        await _context.SaveChangesAsync();
    }

    public async Task<T?> FindAsync<T>(params object[] keys) where T : class
    {
        return await _context.FindAsync<T>(keys);
    }
}