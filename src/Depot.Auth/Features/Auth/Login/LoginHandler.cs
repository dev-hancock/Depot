namespace Depot.Auth.Features.Auth.Login;

using System.Reactive.Linq;
using Domain.Errors;
using Domain.Interfaces;
using ErrorOr;
using Mestra.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Options;
using Persistence;

public class LoginHandler : IMessageHandler<LoginCommand, ErrorOr<LoginResponse>>
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

    public IObservable<ErrorOr<LoginResponse>> Handle(LoginCommand message)
    {
        return Observable.FromAsync(token => Handle(message, token));
    }

    private async Task<ErrorOr<LoginResponse>> Handle(LoginCommand message, CancellationToken token)
    {
        if (string.IsNullOrEmpty(message.Email) && string.IsNullOrEmpty(message.Username))
        {
            return Error.Validation();
        }

        await using var context = await _factory.CreateDbContextAsync(token);

        var user = await context.Users
            .Include(x => x.Memberships)
            .ThenInclude(x => x.Role)
            .ThenInclude(x => x.Permissions)
            .ThenInclude(x => x.Permission)
            .Include(x => x.Memberships)
            .ThenInclude(x => x.Tenant)
            .Include(x => x.Tokens)
            .Where(x => x.Username == message.Username || x.Email == message.Email)
            .SingleOrDefaultAsync(token);

        if (user is null || !user.Password.Verify(message.Password, _hasher))
        {
            return Errors.UserNotFound();
        }

        var session = user.IssueSession(_random, _hasher, _time, _tokens, _options.RefreshTokenLifetime);

        await context.SaveChangesAsync(token);

        return new LoginResponse
        {
            AccessToken = session.AccessToken.Value,
            RefreshToken = session.RefreshToken.Combined,
            ExpiresAt = session.ExpiresAt
        };
    }
}