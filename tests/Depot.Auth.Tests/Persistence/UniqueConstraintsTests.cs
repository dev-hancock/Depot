using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Depot.Auth.Tests.Persistence;

public class UniqueConstraintsTests : TestBase
{
    [Test]
    public async Task AddRefreshToken_WhenAlreadyExists_ThrowsException()
    {
        var token = "duplicate";

        var user = Arrange.User
            .WithSession(x => x.WithRefreshToken(token))
            .WithSession(x => x.WithRefreshToken(token))
            .Build();

        var action = Fixture.SeedAsync(user);

        var ex = await Assert.That(action)
            .ThrowsException()
            .And
            .IsNotNull();

        await Assert.That(ex.InnerException)
            .IsNotNull()
            .And
            .HasMessageContaining("unique");
    }

    [Test]
    public async Task AddUsername_WhenAlreadyExists_ThrowsException()
    {
        var factory = Arrange.User.WithUsername("duplicate");

        var action = Fixture.SeedAsync(
            factory.Build(),
            factory.Build());

        var ex = await Assert.That(action)
            .ThrowsException()
            .And
            .IsNotNull();

        await Assert.That(ex.InnerException)
            .IsNotNull()
            .And
            .HasMessageContaining("unique");
    }


    [Test]
    public async Task AddEmail_WhenAlreadyExists_ThrowsException()
    {
        var factory = Arrange.User.WithEmail("duplicate");

        var action = Fixture.SeedAsync(
            factory.Build(),
            factory.Build());

        var ex = await Assert.That(action)
            .ThrowsException()
            .And
            .IsNotNull();

        await Assert.That(ex.InnerException)
            .IsNotNull()
            .And
            .HasMessageContaining("unique");
    }
}