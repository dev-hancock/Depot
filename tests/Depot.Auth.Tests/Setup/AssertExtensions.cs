using TUnit.Assertions.AssertConditions.Interfaces;
using TUnit.Assertions.AssertionBuilders.Wrappers;

namespace Depot.Auth.Tests.Setup;

public static class AssertExtensions
{
    public static GenericEqualToAssertionBuilderWrapper<HttpStatusCode> IsBadRequest(this IValueSource<HttpStatusCode> source)
    {
        return source.IsEqualTo(HttpStatusCode.BadRequest);
    }

    public static GenericEqualToAssertionBuilderWrapper<HttpStatusCode> IsNoContent(this IValueSource<HttpStatusCode> source)
    {
        return source.IsEqualTo(HttpStatusCode.NoContent);
    }

    public static GenericEqualToAssertionBuilderWrapper<HttpStatusCode> IsOk(this IValueSource<HttpStatusCode> source)
    {
        return source.IsEqualTo(HttpStatusCode.OK);
    }

    public static GenericEqualToAssertionBuilderWrapper<HttpStatusCode> IsUnauthorized(this IValueSource<HttpStatusCode> source)
    {
        return source.IsEqualTo(HttpStatusCode.Unauthorized);
    }
}