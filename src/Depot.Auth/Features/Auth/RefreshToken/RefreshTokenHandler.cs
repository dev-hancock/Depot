namespace Depot.Auth.Features.Auth.RefreshToken;

using System.Reactive.Linq;
using Domain.Auth;
using Domain.Errors;
using Domain.Interfaces;
using ErrorOr;
using Mestra.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Middleware;
using Options;
using Persistence;

public class RefreshTokenHandler : IMessageHandler<RefreshTokenCommand, ErrorOr<RefreshTokenResponse>>
{
    private readonly IDbContextFactory<AuthDbContext> _factory;

    private readonly ISecretHasher _hasher;

    private readonly JwtOptions _options;

    private readonly ISecureRandom _random;

    private readonly TimeProvider _time;

    private readonly ITokenGenerator _tokens;

    private readonly IUserContext _user;

    public RefreshTokenHandler(IDbContextFactory<AuthDbContext> factory, IOptions<JwtOptions> options, IUserContext user,
        ISecureRandom random, ISecretHasher hasher, TimeProvider time, ITokenGenerator tokens)
    {
        _factory = factory;
        _user = user;
        _random = random;
        _hasher = hasher;
        _time = time;
        _tokens = tokens;
        _options = options.Value;
    }

    public IObservable<ErrorOr<RefreshTokenResponse>> Handle(RefreshTokenCommand message)
    {
        return Observable.FromAsync(token => Handle(message, token));
    }

    private async Task<ErrorOr<RefreshTokenResponse>> Handle(RefreshTokenCommand message, CancellationToken token)
    {
        await using var context = await _factory.CreateDbContextAsync(token);

        var user = await context.Users
            .Include(x => x.Tokens)
            .Where(x => x.Id == _user.UserId)
            .SingleOrDefaultAsync(token);

        if (user is null)
        {
            return Errors.UserNotFound();
        }

        var result = RefreshToken
            .Parse(message.RefreshToken)
            .Then(refresh => user
                .RefreshSession(refresh, _random, _hasher, _time, _tokens, _options.RefreshTokenLifetime));

        if (result.IsError)
        {
            return ErrorOr<RefreshTokenResponse>.From(result.Errors);
        }

        var session = result.Value;

        await context.SaveChangesAsync(token);

        return new RefreshTokenResponse
        {
            AccessToken = session.AccessToken.Value,
            RefreshToken = session.RefreshToken.Combined,
            ExpiresAt = session.ExpiresAt
        };
    }
}