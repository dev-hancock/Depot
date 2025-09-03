using System.Reactive.Linq;
using Depot.Auth.Domain.Interfaces;
using Depot.Auth.Domain.Users;
using Depot.Auth.Mappings;
using Depot.Auth.Persistence;
using ErrorOr;
using Mestra.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Depot.Auth.Features.Auth.Login;

public class LoginHandler : IMessageHandler<LoginCommand, ErrorOr<LoginResponse>>
{
    private readonly AuthDbContext _context;

    private readonly ISecretHasher _hasher;

    private readonly ITimeProvider _time;

    private readonly ITokenGenerator _tokens;

    public LoginHandler(AuthDbContext context, ITimeProvider time, ISecretHasher hasher, ITokenGenerator tokens)
    {
        _context = context;
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
        var username = Username
            .TryCreate(message.Username)
            .Match(u => u.Normalized, _ => null!);

        var email = Email
            .TryCreate(message.Email)
            .Match(e => e.Normalized, _ => null!);

        var user = await _context.Users
            .Include(x => x.Memberships).ThenInclude(x => x.Role).ThenInclude(x => x.Permissions).ThenInclude(x => x.Permission)
            .Include(x => x.Memberships).ThenInclude(x => x.Tenant)
            .Include(x => x.Sessions)
            .Where(x => x.Username.Normalized == username || x.Email.Normalized == email)
            .SingleOrDefaultAsync(ct);

        if (user is null || !_hasher.Verify(user.Password, message.Password))
        {
            return Error.Unauthorized();
        }

        var now = _time.UtcNow;

        var result = user.CreateSession(_tokens.GenerateRefreshToken(now).ToRefreshToken());

        if (result.Value is not { } session)
        {
            return ErrorOr<LoginResponse>.From(result.Errors);
        }

        await _context.SaveChangesAsync(ct);

        return new LoginResponse
        {
            AccessToken = _tokens
                .GenerateAccessToken(
                    user.Id,
                    session.Id,
                    [], // TODO: Add roles and permissions
                    now,
                    session.Version)
                .ToAccessToken(),
            RefreshToken = session.RefreshToken
        };
    }
}