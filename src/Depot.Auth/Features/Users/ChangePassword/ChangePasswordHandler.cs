namespace Depot.Auth.Features.Users.ChangePassword;

using System.Reactive.Linq;
using Domain.Interfaces;
using Domain.Users;
using Domain.Users.Errors;
using ErrorOr;
using Mestra.Abstractions;
using Microsoft.EntityFrameworkCore;
using Middleware;
using Persistence;

public class ChangePasswordHandler : IMessageHandler<ChangePasswordCommand, ErrorOr<ChangePasswordResponse>>
{
    private readonly IDbContextFactory<AuthDbContext> _factory;

    private readonly ISecretHasher _hasher;

    private readonly ITimeProvider _time;

    private readonly ITokenGenerator _tokens;

    private readonly IUserContext _user;

    public ChangePasswordHandler(
        IDbContextFactory<AuthDbContext> factory,
        IUserContext user,
        ISecretHasher hasher,
        ITokenGenerator tokens,
        ITimeProvider time)
    {
        _factory = factory;
        _user = user;
        _hasher = hasher;
        _tokens = tokens;
        _time = time;
    }

    public IObservable<ErrorOr<ChangePasswordResponse>> Handle(ChangePasswordCommand message)
    {
        return Observable.FromAsync(token => Handle(message, token));
    }

    private async Task<ErrorOr<ChangePasswordResponse>> Handle(ChangePasswordCommand message, CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var user = await db.Users
            .Where(x => x.Id.Equals(_user.UserId))
            .SingleOrDefaultAsync(ct);

        if (user is null)
        {
            return Errors.UserNotFound();
        }

        if (!_hasher.Verify(user.Password, message.OldPassword))
        {
            return Error.Unauthorized();
        }

        user.ChangePassword(Password.Create(_hasher.Hash(message.NewPassword)));

        user.RevokeSession();

        var now = _time.UtcNow;

        var result = user.CreateSession(_tokens.GenerateRefreshToken(now));

        if (result.Value is not { } session)
        {
            return ErrorOr<ChangePasswordResponse>.From(result.Errors);
        }

        await db.SaveChangesAsync(ct);

        var token = _tokens.GenerateAccessToken(user, session.Id, now);

        return new ChangePasswordResponse
        {
            AccessToken = token.Value,
            RefreshToken = session.RefreshToken,
            ExpiresAt = token.ExpiresAt
        };
    }
}