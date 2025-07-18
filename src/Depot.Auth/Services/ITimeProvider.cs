namespace Depot.Auth.Domain.Auth;

public interface ITimeProvider
{
    DateTime UtcNow => TimeProvider.System.GetUtcNow().UtcDateTime;
}