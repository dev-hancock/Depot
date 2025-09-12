using System.Reactive.Linq;
using Depot.Auth.Domain.Users;
using Depot.Auth.Persistence;
using Depot.Auth.Services;
using ErrorOr;
using Mestra.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Depot.Auth.Features.Auth.Login;

public class LoginHandler : IMessageHandler<LoginCommand, ErrorOr<LoginResponse>>
{
    private readonly AuthDbContext _context;

    private readonly ISecretHasher _hasher;

    private readonly ITimeProvider _time;

    private readonly ITokenService _tokens;

    public LoginHandler(AuthDbContext context, ITimeProvider time, ISecretHasher hasher, ITokenService tokens)
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

        var session = user.CreateSession(_tokens.GetRefreshToken(now));

        await _context.SaveChangesAsync(ct);

        return new LoginResponse
        {
            AccessToken = _tokens.GetAccessToken(user, session, now),
            RefreshToken = session.RefreshToken
        };
    }
}