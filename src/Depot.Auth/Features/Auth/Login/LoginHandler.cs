namespace Depot.Auth.Features.Auth.Login;

using System.Reactive.Linq;
using Domain.Auth;
using Domain.Errors;
using Domain.Interfaces;
using ErrorOr;
using Mestra.Abstractions;
using Microsoft.EntityFrameworkCore;
using Persistence;

public class LoginHandler : IMessageHandler<LoginCommand, ErrorOr<LoginResponse>>
{
    private readonly IDbContextFactory<AuthDbContext> _factory;

    private readonly ISecretHasher _hasher;

    private readonly ITimeProvider _time;

    private readonly ITokenGenerator _tokens;

    public LoginHandler(
        IDbContextFactory<AuthDbContext> factory,
        ITimeProvider time,
        ISecretHasher hasher,
        ITokenGenerator tokens)
    {
        _factory = factory;
        _time = time;
        _hasher = hasher;
        _tokens = tokens;
    }

    public IObservable<ErrorOr<LoginResponse>> Handle(LoginCommand message)
    {
        return Observable.FromAsync(token => Handle(message, token));
    }

    private async Task<ErrorOr<LoginResponse>> Handle(LoginCommand message, CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var user = await db.Users
            .Include(x => x.Memberships)
            .ThenInclude(x => x.Role)
            .ThenInclude(x => x.Permissions)
            .ThenInclude(x => x.Permission)
            .Include(x => x.Memberships)
            .ThenInclude(x => x.Tenant)
            .Include(x => x.Sessions)
            .Where(x => x.Username == message.Username || x.Email == message.Email)
            .SingleOrDefaultAsync(ct);

        if (user is null || !_hasher.Verify(user.Password, message.Password))
        {
            return Errors.UserNotFound();
        }

        var now = _time.UtcNow;

        var result = user.CreateSession(_tokens.GenerateRefreshToken(now));

        if (result.Value is not { } session)
        {
            return ErrorOr<LoginResponse>.From(result.Errors);
        }

        await db.SaveChangesAsync(ct);

        return new LoginResponse
        {
            AccessToken = _tokens.GenerateAccessToken(user, now),
            RefreshToken = session.RefreshToken
        };
    }
}