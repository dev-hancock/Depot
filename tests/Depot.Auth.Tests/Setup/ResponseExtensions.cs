using System.Net.Http.Json;

namespace Depot.Auth.Tests.Setup;

public static class ResponseExtensions
{
    public static Task<T> ReadAsAsync<T>(this HttpResponseMessage response)
    {
        return response.Content.ReadFromJsonAsync<T>()!;
    }
}