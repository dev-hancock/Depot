namespace Depot.Auth.Tests.Data.Builders;

using Abstractions;
using Bogus;
using Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Models;

public class SessionBuilder(UserBuilder user) : IBuilder<TestSession>
{
    private static readonly Faker Faker = new();

    public Guid Id { get; } = Faker.Random.Guid();

    public DateTime Expiry { get; private set; } = Faker.Date.Soon(1, DateTime.UtcNow);

    public bool IsRevoked { get; private set; }

    public string RefreshToken { get; private set; } = Faker.Random.AlphaNumeric(32);

    public TestSession Build(IServiceProvider services)
    {
        var tokens = services.GetRequiredService<ITokenGenerator>();

        return new TestSession(
            Id,
            user.Id,
            tokens.GenerateAccessToken(
                user.Id,
                Id,
                user.Roles.ToArray(),
                Expiry).Value,
            RefreshToken,
            IsRevoked,
            Expiry
        );
    }

    public SessionBuilder WithRevoked()
    {
        IsRevoked = true;

        return this;
    }

    public SessionBuilder WithExpiry(DateTime expiry)
    {
        Expiry = expiry;

        return this;
    }

    public SessionBuilder WithRefreshToken(string token)
    {
        RefreshToken = token;

        return this;
    }
}