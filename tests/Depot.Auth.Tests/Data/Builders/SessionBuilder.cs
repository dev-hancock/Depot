namespace Depot.Auth.Tests.Data.Builders;

using Bogus;
using Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Models;

public class SessionBuilder(UserBuilder user)
{
    private static readonly Faker Faker = new();

    public Guid Id { get; } = Faker.Random.Guid();

    public DateTime Expiry { get; private set; } = Faker.Date.Future();

    public bool IsRevoked { get; private set; }

    public TestSession Build(IServiceProvider services)
    {
        var tokens = services.GetRequiredService<ITokenGenerator>();

        return new TestSession(
            Id,
            tokens.GenerateAccessToken(
                user.Id,
                Id,
                user.Roles.ToArray(),
                Expiry).Value,
            tokens.GenerateRefreshToken(Expiry).Value,
            IsRevoked,
            Expiry
        );
    }

    public SessionBuilder WithRevoked(bool revoked)
    {
        IsRevoked = revoked;

        return this;
    }

    public SessionBuilder WithExpiry(DateTime expiry)
    {
        Expiry = expiry;

        return this;
    }
}