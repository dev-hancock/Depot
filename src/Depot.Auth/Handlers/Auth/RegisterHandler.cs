namespace Depot.Auth.Handlers.Auth;

using System.Reactive.Linq;
using Depot.Auth.Domain.Auth;
using Depot.Auth.Domain.Errors;
using Depot.Auth.Domain.Interfaces;
using Depot.Auth.Domain.Tenants;
using Depot.Auth.Domain.Users;
using Depot.Auth.Options;
using Depot.Auth.Persistence;
using ErrorOr;
using Mestra.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

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

        var password = Password.New(message.Password, _hasher);

        var email = Email.New(message.Email);

        var result = User.New(message.Username, email, password, _time);

        if (result.IsError)
        {
            return ErrorOr<Response>.From(result.Errors);
        }

        var user = result.Value;

        var tenant = Tenant.Personal(user, _time);

        user.AddTenant(tenant, Role.Admin());

        var session = user.IssueSession(_random, _hasher, _time, _tokens, _options.RefreshTokenLifetime);

        context.Users.Add(user);

        await context.SaveChangesAsync(token);

        return new Response(user, session);
    }

    public record Request(string Username, string Email, string Password, string[] Roles) : IRequest<ErrorOr<Response>>;

    public record Response(User User, Session Session);
}