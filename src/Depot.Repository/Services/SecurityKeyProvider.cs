namespace Depot.Repository.Services;

using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

public interface ISecurityKeyProvider
{
    SecurityKey GetSecurityKey(string filepath);
}

public class SecurityKeyProvider : ISecurityKeyProvider
{
    public SecurityKey GetSecurityKey(string filepath)
    {
        var key = ECDsa.Create();

        key.ImportFromPem(File.ReadAllText(filepath));

        return new ECDsaSecurityKey(key)
        {
            KeyId = "depot-key"
        };
    }
}