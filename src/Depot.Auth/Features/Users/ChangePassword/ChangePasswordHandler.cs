using System.Reactive.Linq;
using Depot.Auth.Domain.Interfaces;
using Depot.Auth.Domain.Users;
using Depot.Auth.Domain.Users.Errors;
using Depot.Auth.Mappings;
using Depot.Auth.Middleware;
using Depot.Auth.Persistence;
using ErrorOr;
using Mestra.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Depot.Auth.Features.Users.ChangePassword;

public class ChangePasswordHandler : IMessageHandler<ChangePasswordCommand, ErrorOr<ChangePasswordResponse>>
{
    private readonly AuthDbContext _context;

    private readonly ISecretHasher _hasher;

    private readonly ITimeProvider _time;

    private readonly ITokenGenerator _tokens;

    private readonly IUserContext _user;

    public ChangePasswordHandler(
        AuthDbContext context,
        IUserContext user,
        ISecretHasher hasher,
        ITokenGenerator tokens,
        ITimeProvider time)
    {
        _context = context;
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
        var user = await _context.Users
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

        var result = user.CreateSession(_tokens.GenerateRefreshToken(now).ToRefreshToken());

        if (result.Value is not { } session)
        {
            return ErrorOr<ChangePasswordResponse>.From(result.Errors);
        }

        await _context.SaveChangesAsync(ct);

        return new ChangePasswordResponse
        {
            AccessToken = _tokens
                .GenerateAccessToken(
                    user.Id.Value,
                    session.Id.Value,
                    [],
                    now)
                .ToAccessToken(),
            RefreshToken = session.RefreshToken
        };
    }
}