namespace Depot.Auth.Handlers;

using System.Reactive.Linq;
using Domain;
using ErrorOr;
using Mestra.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Options;
using Persistence;

public class RegisterHandler : IMessageHandler<RegisterHandler.Request, ErrorOr<RegisterHandler.Response>>
{
    private readonly IDbContextFactory<AuthDbContext> _factory;

    private readonly ISecretHasher _hasher;

    private readonly JwtOptions _options;

    private readonly ISecureRandom _random;

    private readonly TimeProvider _time;

    private readonly ITokenGenerator _tokens;

    public RegisterHandler(IOptions<JwtOptions> options, IDbContextFactory<AuthDbContext> factory, TimeProvider time,
        ISecretHasher hasher, ISecureRandom random, ITokenGenerator tokens)
    {
        _options = options.Value;
        _factory = factory;
        _time = time;
        _hasher = hasher;
        _random = random;
        _tokens = tokens;
    }

    public IObservable<ErrorOr<Response>> Handle(Request message)
    {
        return Observable.FromAsync(token => Handle(message, token));
    }

    private async Task<ErrorOr<Response>> Handle(Request message, CancellationToken token)
    {
        await using var context = await _factory.CreateDbContextAsync(token);

        var exists = await context.Users.AnyAsync(x => x.Username == message.Username, token);

        if (exists)
        {
            return Errors.UserAlreadyExists();
        }

        var result = Password
            .New(message.Password, _hasher)
            .Then(password => User
                .New(message.Username, password, _time));

        if (result.IsError)
        {
            return ErrorOr<Response>.From(result.Errors);
        }

        var user = result.Value;

        var roles = await context.Roles
            .Where(x => message.Roles.Contains(x.Name))
            .ToListAsync(token);

        user.AssignRoles(roles);

        var session = user.IssueSession(_random, _hasher, _time, _tokens, _options.RefreshTokenLifetime);

        context.Users.Add(user);

        await context.SaveChangesAsync(token);

        return new Response(user, session);
    }

    public record Request(string Username, string Password, string[] Roles) : IRequest<ErrorOr<Response>>;

    public record Response(User User, Session Session);
}