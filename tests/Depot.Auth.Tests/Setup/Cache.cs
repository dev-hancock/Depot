namespace Depot.Auth.Tests.Setup;

public static class CacheExtensions
{
    public static async Task<int> GetSessionAsync(this IDistributedCache cache, Guid id)
    {
        return BitConverter.ToInt32(await cache.GetAsync($"{id}:ver"));
    }

    public static async Task SetSessionAsync(this IDistributedCache cache, Guid id, int version)
    {
        await cache.SetAsync($"{id}:ver", BitConverter.GetBytes(version));
    }
}