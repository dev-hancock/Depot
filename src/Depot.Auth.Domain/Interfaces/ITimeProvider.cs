namespace Depot.Auth.Domain.Interfaces;

public interface ITimeProvider
{
    DateTime UtcNow => TimeProvider.System.GetUtcNow().UtcDateTime;
}