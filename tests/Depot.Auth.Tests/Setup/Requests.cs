namespace Depot.Auth.Tests.Setup;

public static class Requests
{
    public static RequestBuilder Request(HttpMethod method, string uri)
    {
        return new RequestBuilder(method, uri);
    }

    public static RequestBuilder Get(string uri)
    {
        return Request(HttpMethod.Get, uri);
    }

    public static RequestBuilder Post(string uri, object payload)
    {
        return Request(HttpMethod.Post, uri).WithContent(payload);
    }

    public static RequestBuilder Put(string uri, object payload)
    {
        return Request(HttpMethod.Put, uri).WithContent(payload);
    }

    public static RequestBuilder Delete(string uri)
    {
        return Request(HttpMethod.Delete, uri);
    }
}