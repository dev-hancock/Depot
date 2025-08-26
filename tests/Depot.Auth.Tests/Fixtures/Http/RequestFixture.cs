namespace Depot.Auth.Tests.Fixtures.Http;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Options;

public sealed class RequestFixture(ApplicationFixture fixture)
{
    private readonly HttpClient _client = fixture.Client;

    private readonly SecurityKey _key = fixture.Key;

    private readonly JwtOptions _options = fixture.GetService<IOptions<JwtOptions>>().Value;

    public RequestBuilder Request(HttpMethod method, string uri)
    {
        return new RequestBuilder(_client, method, uri, _options, _key);
    }

    public RequestBuilder Get(string uri)
    {
        return Request(HttpMethod.Get, uri);
    }

    public RequestBuilder Post(string uri, object payload)
    {
        return Request(HttpMethod.Post, uri).WithContent(payload);
    }

    public RequestBuilder Put(string uri, object payload)
    {
        return Request(HttpMethod.Put, uri).WithContent(payload);
    }

    public RequestBuilder Delete(string uri)
    {
        return Request(HttpMethod.Delete, uri);
    }
}