namespace Depot.Auth.Features.Auth.Register;

using System.Reactive.Linq;
using Domain.Errors;
using Domain.Interfaces;
using Domain.Tenants;
using Domain.Users;
using ErrorOr;
using Mestra.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Options;
using Persistence;

public class RegisterHandler : IMessageHandler<RegisterCommand, ErrorOr<RegisterResponse>>
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

    public IObservable<ErrorOr<RegisterResponse>> Handle(RegisterCommand message)
    {
        return Observable.FromAsync(token => Handle(message, token));
    }

    private async Task<ErrorOr<RegisterResponse>> Handle(RegisterCommand message, CancellationToken token)
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
            return ErrorOr<RegisterResponse>.From(result.Errors);
        }

        var user = result.Value;

        var tenant = Tenants.Personal(user.Id, _time);

        user.AddTenant(tenant, Roles.Admin());

        var session = user.IssueSession(_random, _hasher, _time, _tokens, _options.RefreshTokenLifetime);

        context.Users.Add(user);

        await context.SaveChangesAsync(token);

        return new RegisterResponse
        {
            AccessToken = session.AccessToken.Value,
            RefreshToken = session.RefreshToken.Combined,
            ExpiresAt = session.ExpiresAt
        };
    }
}