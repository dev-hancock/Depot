using System.Security.Cryptography;
using Depot.Auth.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Depot.Auth.Services;

public interface ISecurityKeyProvider
{
    SecurityKey GetKey();
}

public class SecurityKeyProvider(IOptions<JwtOptions> options) : ISecurityKeyProvider
{
    private readonly JwtOptions _options = options.Value;

    public SecurityKey GetKey()
    {
        var key = ECDsa.Create();

        key.ImportFromPem(File.ReadAllText(_options.KeyPath));

        return new ECDsaSecurityKey(key)
        {
            KeyId = "depot-key"
        };
    }
}