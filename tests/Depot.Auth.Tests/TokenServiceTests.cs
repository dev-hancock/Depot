namespace Depot.Auth.Tests;

using System.Reactive.Linq;
using AutoFixture;
using AutoFixture.AutoMoq;
using Domain;
using Handlers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Options;
using Moq;
using Persistence;
using Services;

public sealed class TokenServiceTests
{
    private static IOptions<JwtOptions> ValidOptions()
    {
        return Options.Create(new JwtOptions
        {
            Issuer = "Depot",
            Audience = "DepotClients",
            AccessTokenLifetime = TimeSpan.FromHours(1),
            RefreshTokenLifetime = TimeSpan.FromDays(30)
        });
    }

    [Fact]
    public async Task IssueToken_AddsTokenRow()
    {
        // Arrange
        var fixture = new Fixture().Customize(new AutoMoqCustomization
        {
            ConfigureMembers = true
        });

        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var now = DateTime.Now;

        var time = new Mock<TimeProvider>();
        time.Setup(x => x.GetUtcNow())
            .Returns(now);

        var random = new Mock<ISecureRandom>();
        random.Setup(x => x.Next(It.IsAny<int>()))
            .Returns(fixture.Create<string>());

        var hasher = new Mock<ISecretHasher>();
        hasher.Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);
        hasher.Setup(x => x.Hash(It.IsAny<string>()))
            .Returns(fixture.Create<string>());

        var tokens = new Mock<ITokenGenerator>();
        tokens.Setup(x => x.CreateAccessToken(It.IsAny<User>(), It.IsAny<DateTime>()))
            .Returns(fixture.Create<AccessToken>());

        var factory = new PooledDbContextFactory<AuthDbContext>(options);

        await using var seed = await factory.CreateDbContextAsync();

        var username = fixture.Create<string>();
        var password = fixture.Create<string>();

        var user = User.New(username, SecurePassword.New(password, hasher.Object), now);

        await seed.Users.AddAsync(user);

        await seed.SaveChangesAsync();

        var sut = new LoginHandler(
            ValidOptions(),
            factory,
            time.Object,
            hasher.Object,
            random.Object,
            tokens.Object);

        // Act
        var result = await sut.Handle(new LoginHandler.Request(user.Username, fixture.Create<string>()));

        // Assert
        await using var context = await factory.CreateDbContextAsync();

        Assert.False(result.IsError);
        Assert.Single(context.Tokens);
        Assert.Equal(user.Id, context.Users.First().Id);
    }
}