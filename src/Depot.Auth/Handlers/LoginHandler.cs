namespace Depot.Auth.Handlers;

using System.Reactive.Linq;
using Domain;
using ErrorOr;
using FluentValidation;
using Mestra.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Persistence;
using Services;

public class LoginHandler : IMessageHandler<LoginHandler.Request, ErrorOr<TokenPair>>
{
    private readonly IDbContextFactory<AuthDbContext> _factory;

    private readonly ISecretHasher _hasher;

    private readonly JwtOptions _options;

    private readonly ISecureRandom _random;

    private readonly TimeProvider _time;

    private readonly ITokenGenerator _tokens;

    public LoginHandler(IOptions<JwtOptions> options, IDbContextFactory<AuthDbContext> factory, TimeProvider time,
        ISecretHasher hasher, ISecureRandom random, ITokenGenerator tokens)
    {
        _options = options.Value;
        _factory = factory;
        _time = time;
        _hasher = hasher;
        _random = random;
        _tokens = tokens;
    }

    public IObservable<ErrorOr<TokenPair>> Handle(Request message)
    {
        return Observable.FromAsync(token => Handle(message, token));
    }

    private async Task<ErrorOr<TokenPair>> Handle(Request request, CancellationToken token)
    {
        await using var context = await _factory.CreateDbContextAsync(token);

        var user = await context.Users
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .Include(x => x.Tokens)
            .Where(x => x.Username == request.Username)
            .SingleOrDefaultAsync(token);

        if (user is null || !_hasher.Verify(user!.PasswordHash, request.Password))
        {
            return Error.Unauthorized(
                "USER_NOT_FOUND",
                "Invalid username or password.");
        }

        var pair = user.IssueToken(_random, _hasher, _time, _tokens, _options.RefreshTokenLifetime);

        await context.SaveChangesAsync(token);

        return pair;
    }

    public class Validator : AbstractValidator<Request>
    {
    }

    public record Request(string Username, string Password) : IRequest<ErrorOr<TokenPair>>;
}