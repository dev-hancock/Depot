using System.Security.Cryptography;

namespace Depot.Auth.Tests.Setup;

public class RefreshTokenBuilder
{
    private int _length = 64;

    public string Build()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        var token = new char[_length];
        var bytes = RandomNumberGenerator.GetBytes(_length);

        for (var i = 0; i < _length; i++)
        {
            token[i] = chars[bytes[i] % chars.Length];
        }

        return new string(token);
    }

    public RefreshTokenBuilder WithLength(int length)
    {
        _length = length;

        return this;
    }
}
