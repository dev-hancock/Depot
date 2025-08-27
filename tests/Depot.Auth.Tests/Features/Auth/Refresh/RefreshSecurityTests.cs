// namespace Depot.Auth.Tests.Features.Auth.Refresh;
//
// using System.Net;
// using Depot.Auth.Features.Auth.Refresh;
// using Microsoft.IdentityModel.Tokens;
//
// [ClassDataSource(typeof(IntegrationFixture))]
// public class RefreshSecurityTests : IntegrationTest
// {
//     [Test]
//     public async Task Refresh_WithRevokedToken_ShouldReturnUnauthorized()
//     {
//         var user = Fixture.Arrange.User.WithSession(x => x.WithRevoked()).Build();
//
//         await Fixture.Database.SeedAsync(user);
//
//         var payload = new RefreshCommand
//         {
//             RefreshToken = user.Sessions[0].RefreshToken
//         };
//
//         var response = await Fixture.Client.Post("api/v1/auth/refresh", payload)
//             .WithAuthorization(x => x.WithUser(user.Id.Value))
//             .SendAsync();
//
//         await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
//     }
//
//     [Test]
//     public async Task? Refresh_WithInvalidToken_ShouldReturnNotFound()
//     {
//         var user = Fixture.Arrange.User.WithSession(x => x.WithRevoked()).Build();
//
//         await Fixture.Database.SeedAsync(user);
//
//         var payload = new RefreshCommand
//         {
//             RefreshToken = Base64UrlEncoder.Encode(Fixture.Faker.Random.Bytes(32))
//         };
//
//         var response = await Fixture.Client.Post("api/v1/auth/refresh", payload)
//             .WithAuthorization(x => x.WithUser(user.Id.Value))
//             .SendAsync();
//
//         await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
//     }
// }

