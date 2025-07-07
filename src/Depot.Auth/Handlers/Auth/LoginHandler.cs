namespace Depot.Auth.Handlers.Auth;

using System.Reactive.Linq;
using Depot.Auth.Domain.Auth;
using Depot.Auth.Domain.Errors;
using Depot.Auth.Domain.Interfaces;
using Depot.Auth.Options;
using Depot.Auth.Persistence;
using ErrorOr;
using Mestra.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

public class LoginHandler : IMessageHandler<LoginHandler.Request, ErrorOr<Session>>
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

    public IObservable<ErrorOr<Session>> Handle(Request message)
    {
        return Observable.FromAsync(token => Handle(message, token));
    }

    private async Task<ErrorOr<Session>> Handle(Request request, CancellationToken token)
    {
        await using var context = await _factory.CreateDbContextAsync(token);

        var user = await context.Users
            .Include(x => x.Memberships)
            .ThenInclude(x => x.Role)
            .ThenInclude(x => x.Permissions)
            .ThenInclude(x => x.Permission)
            .Include(x => x.Memberships)
            .ThenInclude(x => x.Tenant)
            .Include(x => x.Tokens)
            .Where(x => x.Username == request.Username)
            .SingleOrDefaultAsync(token);

        if (user is null || !user.Password.Verify(request.Password, _hasher))
        {
            return Errors.UserNotFound();
        }

        var session = user.IssueSession(_random, _hasher, _time, _tokens, _options.RefreshTokenLifetime);

        await context.SaveChangesAsync(token);

        return session;
    }

    public record Request(string Username, string Password) : IRequest<ErrorOr<Session>>;
}