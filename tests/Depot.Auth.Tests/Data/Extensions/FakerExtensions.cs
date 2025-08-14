namespace Depot.Auth.Tests.Extensions;

using Bogus.DataSets;

public static class FakerExtensions
{
    public static string StrongPassword(this Internet internet)
    {
        var password = string.Concat(
            internet.Random.Char('A', 'Z'),
            internet.Random.Char('a', 'z'),
            internet.Random.Int(0, 9),
            internet.Random.ArrayElement("!@#$%^&*()_+-=[]{}|;:,.<>?".ToCharArray()),
            internet.Password(8, false, "")
        );

        return new string(password
            .OrderBy(_ => Guid.NewGuid())
            .ToArray());
    }
}