// namespace Depot.Auth.Services;
//
// using Domain;
// using ErrorOr;
// using Isopoh.Cryptography.Argon2;
// using Microsoft.EntityFrameworkCore;
// using Persistence;
//
// public interface IUserService
// {
//     Task<ErrorOr<User>> LoginAsync(string username, string password, CancellationToken token = default);
//
//
//     Task<ErrorOr<User>> UpdateAsync(Guid id, string password, CancellationToken token = default);
//
//     Task<ErrorOr<User>> DeleteAsync(Guid id, CancellationToken token = default);
//
//     Task<ErrorOr<User>> GetUserAsync(Guid id, CancellationToken token = default);
// }
//
// public class UserService : IUserService
// {
//     private readonly IDbContextFactory<AuthDbContext> _factory;
//
//     private readonly ISecretHasher _hasher;
//
//     public UserService(IDbContextFactory<AuthDbContext> factory, ISecretHasher hasher)
//     {
//         _factory = factory;
//         _hasher = hasher;
//     }
//
//     public async Task<ErrorOr<User>> LoginAsync(string username, string password, CancellationToken token = default)
//     {
//         await using var context = await _factory.CreateDbContextAsync(token);
//
//         var user = await context.Users
//             .Include(x => x.UserRoles)
//             .ThenInclude(x => x.Role)
//             .SingleOrDefaultAsync(x => x.Username == username, token);
//
//         if (user is null || !_hasher.Verify(user!.PasswordHash, password))
//         {
//             return Error.Unauthorized(
//                 "USER_NOT_FOUND",
//                 "Invalid username or password.");
//         }
//
//         return user;
//     }
//
//
//     public async Task<ErrorOr<User>> UpdateAsync(Guid id, string password, CancellationToken token = default)
//     {
//         await using var context = await _factory.CreateDbContextAsync(token);
//
//         var user = await context.Users.SingleOrDefaultAsync(x => x.Id == id, token);
//
//         if (user is null)
//         {
//             return Error.NotFound("USER_NOT_FOUND", "User not found.");
//         }
//
//         user.PasswordHash = Argon2.Hash(password);
//
//         await context.SaveChangesAsync(token);
//
//         return user;
//     }
//
//     public async Task<ErrorOr<User>> DeleteAsync(Guid id, CancellationToken token = default)
//     {
//         await using var context = await _factory.CreateDbContextAsync(token);
//
//         var user = await context.Users.SingleOrDefaultAsync(x => x.Id == id, token);
//
//         if (user is null)
//         {
//             return Error.NotFound("USER_NOT_FOUND", "User not found.");
//         }
//
//         context.Users.Remove(user);
//
//         await context.SaveChangesAsync(token);
//
//         return user;
//     }
// }
//
// public interface IRoleService
// {
// }

