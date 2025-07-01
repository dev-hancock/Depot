namespace Depot.Auth.Tests;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Domain;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Services;

public sealed class TokenServiceTests
{
    [Fact]
    public void IssueToken_Should_Produce_Valid_Jwt_With_Expected_Claims()
    {
        // Arrange
        using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var key = new ECDsaSecurityKey(ecdsa);

        const int duration = 30;

        var opts = Options.Create(new JwtOptions(
            "Depot",
            "DepotClient",
            TimeSpan.FromMinutes(duration)));

        var now = DateTime.UtcNow;
        var time = new Mock<TimeProvider>();

        time.Setup(x => x.GetUtcNow())
            .Returns(now);

        var sut = new TokenService(opts, key, time.Object);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "alice",
            UserRoles =
            {
                new UserRole
                {
                    Role = new Role
                    {
                        Id = Guid.NewGuid(),
                        Name = "Administrator"
                    }
                }
            }
        };

        // Act
        var jwt = sut.IssueToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var parameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = true,
            ValidIssuer = "Depot",
            ValidateAudience = true,
            ValidAudience = "DepotClient",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        var principal = handler.ValidateToken(jwt, parameters, out var token);

        Assert.NotNull(token);
        Assert.IsType<JwtSecurityToken>(token);

        Assert.Equal(user.Id.ToString(), principal.FindFirstValue(ClaimTypes.NameIdentifier));
        Assert.Equal("alice", principal.FindFirstValue(ClaimTypes.Name));
        Assert.True(principal.IsInRole("Administrator"));

        var delta = (token.ValidTo - now.AddMinutes(duration)).TotalSeconds;

        Assert.InRange(delta, -1, 1);
    }
}