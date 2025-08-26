namespace Depot.Auth.Tests.Fixtures.Builders;

using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.IdentityModel.Tokens;
using Options;

public class RequestBuilder(HttpClient client, HttpMethod method, string uri, JwtOptions options, SecurityKey key)
{
    private readonly HttpRequestMessage _request = new(method, uri);

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

    public RequestBuilder WithAuthorization(string token)
    {
        _request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return this;
    }

    public RequestBuilder WithUserAgent(string value)
    {
        _request.Headers.UserAgent.ParseAdd(value);

        return this;
    }

    public RequestBuilder WithAuthorization(Action<AccessTokenBuilder> configure)
    {
        var builder = new AccessTokenBuilder(options, key);

        configure(builder);

        var token = builder.Build();

        return WithAuthorization(token);
    }

    public async Task<HttpResponseMessage> SendAsync()
    {
        return await client.SendAsync(_request);
    }
}