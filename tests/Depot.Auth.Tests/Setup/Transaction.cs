namespace Depot.Auth.Tests.Setup;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

public class Transaction(DbContext context) : IDisposable
{
    private readonly IDbContextTransaction _instance = context.Database.BeginTransaction();

    public void Dispose()
    {
        _instance.Rollback();
        _instance.Dispose();
    }
}