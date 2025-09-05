using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Depot.Auth.Tests.Setup;

public class RequestBuilder(HttpMethod method, string uri) : IDisposable
{
    private readonly HttpRequestMessage _request = new(method, uri);

    public RequestBuilder Authorize(Action<AccessTokenBuilder> configure)
    {
        var builder = new AccessTokenBuilder();

        configure(builder);

        var token = builder.Build();

        return Authorize(token);
    }

    public RequestBuilder Authorize(string token)
    {
        _request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return this;
    }

    public void Dispose()
    {
        _request.Dispose();
    }

    public async Task<HttpResponseMessage> SendAsync()
    {
        return await Global.Client.SendAsync(_request);
    }

    public RequestBuilder WithContent(object payload)
    {
        _request.Content = JsonContent.Create(payload);

        return this;
    }

    public RequestBuilder WithHeader(string name, string value)
    {
        _request.Headers.Add(name, value);

        return this;
    }

    public RequestBuilder WithUserAgent(string value)
    {
        _request.Headers.UserAgent.ParseAdd(value);

        return this;
    }
}