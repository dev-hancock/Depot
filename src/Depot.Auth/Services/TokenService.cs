// namespace Depot.Auth.Services;
//
// using Domain;
// using ErrorOr;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Options;
// using Persistence;
//
// public interface ITokenService
// {
//     Task<ErrorOr<TokenPair>> IssueTokenAsync(User user, CancellationToken token = default);
//
//     Task RevokeTokenAsync(Guid id, CancellationToken token = default);
//
//     Task<ErrorOr<TokenPair>> RefreshTokenAsync(string refresh, CancellationToken token = default);
// }
//
// public sealed class TokenService : ITokenService
// {
//     private readonly IDbContextFactory<AuthDbContext> _factory;
//
//     private readonly ISecretHasher _hasher;
//
//     private readonly JwtOptions _options;
//
//     private readonly ISecureRandom _random;
//
//     private readonly TimeProvider _time;
//
//     private readonly ITokenGenerator _tokens;
//
//     public TokenService(
//         IOptions<JwtOptions> options,
//         IDbContextFactory<AuthDbContext> factory,
//         ISecureRandom random,
//         ISecretHasher hasher,
//         TimeProvider time,
//         ITokenGenerator tokens)
//     {
//         _options = options.Value;
//         _factory = factory;
//         _random = random;
//         _hasher = hasher;
//         _time = time;
//         _tokens = tokens;
//     }
//
//     public async Task<ErrorOr<TokenPair>> IssueTokenAsync(User user, CancellationToken token = default)
//     {
//         var pair = user.IssueToken(_random, _hasher, _time, _tokens, _options.RefreshTokenLifetime);
//
//         await using var context = await _factory.CreateDbContextAsync(token);
//
//         // context.Attach(user);
//
//         foreach (var t in user.Tokens)
//         {
//             context.Entry(t).State = EntityState.Added;
//         }
//
//         var count = await context.SaveChangesAsync(token);
//
//         return pair;
//     }
//
//     public async Task<ErrorOr<TokenPair>> RefreshTokenAsync(string refresh, CancellationToken token = default)
//     {
//         var current = RefreshToken.Parse(refresh);
//
//         if (current.IsError)
//         {
//             return ErrorOr<TokenPair>.From(current.Errors);
//         }
//
//         var id = current.Value.Id;
//         // var secret = current.Value.;
//         var secret = string.Empty;
//
//         await using var context = await _factory.CreateDbContextAsync(token);
//
//         var stored = await context.Tokens
//             .Include(x => x.User)
//             .Where(x => x.Id == id)
//             .Where(x => !x.IsRevoked)
//             .FirstOrDefaultAsync(token);
//
//         if (stored == null)
//         {
//             return Error.NotFound("TOKEN_INVALID", "Token not found");
//         }
//
//         var valid = RefreshToken.Validate(stored, _hasher, _time, secret);
//
//         if (valid.IsError)
//         {
//             return ErrorOr<TokenPair>.From(valid.Errors);
//         }
//
//         var pair = stored.User.IssueToken(_random, _hasher, _time, _tokens, _options.RefreshTokenLifetime);
//
//         stored.Revoke();
//
//         await context.SaveChangesAsync(token);
//
//         return pair;
//     }
// }

