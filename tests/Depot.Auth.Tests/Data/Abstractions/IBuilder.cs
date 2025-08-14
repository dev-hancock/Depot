namespace Depot.Auth.Tests.Data.Abstractions;

public delegate TEntity MappingDelegate<in T, out TEntity>(IServiceProvider services, T contract) where TEntity : class;

public interface IBuilder<TSpec, out TEntity> where TEntity : class
{
    MappingDelegate<TSpec, TEntity> Mapping { get; }

    TSpec Build(IServiceProvider services);
}