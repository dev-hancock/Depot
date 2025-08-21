namespace Depot.Auth.Tests.Data.Abstractions;

public interface IBuilder<out T> where T : class
{
    T Build();
}