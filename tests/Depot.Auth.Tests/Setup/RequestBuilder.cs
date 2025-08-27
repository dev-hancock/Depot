namespace Depot.Auth.Tests.Setup;

using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Options;

public class RequestBuilder(HttpMethod method, string uri) : IDisposable
{
    private readonly SecurityKey _key = Global.Key;

    private readonly JwtOptions _options = Global.Service<IOptions<JwtOptions>>().Value;

    private readonly HttpRequestMessage _request = new(method, uri);

    public void Dispose()
    {
        _request.Dispose();
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
        var builder = new AccessTokenBuilder(_options, _key);

        configure(builder);

        var token = builder.Build();

        return WithAuthorization(token);
    }

    public async Task<HttpResponseMessage> SendAsync()
    {
        return await Global.Client.SendAsync(_request);
    }
}