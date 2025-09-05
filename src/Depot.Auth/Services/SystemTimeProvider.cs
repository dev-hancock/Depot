namespace Depot.Auth.Services;

public interface ITimeProvider
{
    DateTime UtcNow => TimeProvider.System.GetUtcNow().UtcDateTime;
}

public class SystemTimeProvider : ITimeProvider;