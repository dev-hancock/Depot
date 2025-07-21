namespace Depot.Auth.Features.Auth.Register;

using System.Reactive.Linq;
using Domain.Auth;
using Domain.Errors;
using Domain.Interfaces;
using Domain.Users;
using ErrorOr;
using Mestra.Abstractions;
using Microsoft.EntityFrameworkCore;
using Persistence;

public class RegisterHandler : IMessageHandler<RegisterCommand, ErrorOr<RegisterResponse>>
{
    private readonly IDbContextFactory<AuthDbContext> _factory;

    private readonly ISecretHasher _hasher;

    private readonly ITimeProvider _time;

    private readonly ITokenGenerator _tokens;

    public RegisterHandler(
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

    public IObservable<ErrorOr<RegisterResponse>> Handle(RegisterCommand message)
    {
        return Observable.FromAsync(token => Handle(message, token));
    }

    private async Task<ErrorOr<RegisterResponse>> Handle(RegisterCommand message, CancellationToken token)
    {
        await using var db = await _factory.CreateDbContextAsync(token);

        var exists = await db.Users
            .AsNoTracking()
            .Where(x => x.Email == message.Email || x.Username == message.Username)
            .AnyAsync(token);

        if (exists)
        {
            return Errors.UserAlreadyExists();
        }

        var now = _time.UtcNow;

        var user = User.Create(
            message.Username,
            Email.Create(message.Email),
            Password.Create(_hasher.Hash(message.Password)),
            now);

        var result = user.CreateSession(_tokens.GenerateRefreshToken(now));

        if (result.Value is not { } session)
        {
            return ErrorOr<RegisterResponse>.From(result.Errors);
        }

        db.Users.Add(user);

        await db.SaveChangesAsync(token);

        return new RegisterResponse
        {
            AccessToken = _tokens.GenerateAccessToken(user, now),
            RefreshToken = session.RefreshToken
        };
    }
}