namespace Depot.Auth.Handlers;

using System.Reactive.Linq;
using Domain;
using ErrorOr;
using Mestra.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Options;
using Persistence;
using Services;

public class RefreshHandler : IMessageHandler<RefreshHandler.Request, ErrorOr<Session>>
{
    private readonly IDbContextFactory<AuthDbContext> _factory;

    private readonly ISecretHasher _hasher;

    private readonly JwtOptions _options;

    private readonly ISecureRandom _random;

    private readonly TimeProvider _time;

    private readonly ITokenGenerator _tokens;

    public RefreshHandler(IDbContextFactory<AuthDbContext> factory, IOptions<JwtOptions> options, ISecureRandom random,
        ISecretHasher hasher, TimeProvider time, ITokenGenerator tokens)
    {
        _factory = factory;
        _random = random;
        _hasher = hasher;
        _time = time;
        _tokens = tokens;
        _options = options.Value;
    }

    public IObservable<ErrorOr<Session>> Handle(Request message)
    {
        return Observable.FromAsync(token => Handle(message, token));
    }

    private async Task<ErrorOr<Session>> Handle(Request message, CancellationToken token)
    {
        await using var context = await _factory.CreateDbContextAsync(token);

        var user = await context.Users
            .Include(x => x.Tokens)
            .Where(x => x.Id == message.UserId)
            .SingleOrDefaultAsync(token);

        if (user is null)
        {
            return Errors.UserNotFound();
        }

        var result = RefreshToken
            .Parse(message.Token)
            .Then(refresh => user
                .RefreshSession(refresh, _random, _hasher, _time, _tokens, _options.RefreshTokenLifetime));

        if (result.IsError)
        {
            return ErrorOr<Session>.From(result.Errors);
        }

        await context.SaveChangesAsync(token);

        return result.Value;
    }

    public sealed record Request(Guid UserId, string Token) : IRequest<ErrorOr<Session>>;
}