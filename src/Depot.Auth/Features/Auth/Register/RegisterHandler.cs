using System.Reactive.Linq;
using Depot.Auth.Domain.Interfaces;
using Depot.Auth.Domain.Users;
using Depot.Auth.Domain.Users.Errors;
using Depot.Auth.Mappings;
using Depot.Auth.Persistence;
using ErrorOr;
using Mestra.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Depot.Auth.Features.Auth.Register;

public class RegisterHandler : IMessageHandler<RegisterCommand, ErrorOr<RegisterResponse>>
{
    private readonly AuthDbContext _context;

    private readonly ISecretHasher _hasher;

    private readonly ITimeProvider _time;

    private readonly ITokenGenerator _tokens;

    public RegisterHandler(
        AuthDbContext context,
        ITimeProvider time,
        ISecretHasher hasher,
        ITokenGenerator tokens)
    {
        _context = context;
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
        var username = Username
            .TryCreate(message.Username)
            .Match(u => u.Normalized, _ => null!);

        var email = Email
            .TryCreate(message.Email)
            .Match(e => e.Normalized, _ => null!);

        var exists = await _context.Users
            .AsNoTracking()
            .Where(x => x.Email.Normalized == email || x.Username.Normalized == username)
            .AnyAsync(token);

        if (exists)
        {
            return Errors.UserAlreadyExists();
        }

        var now = _time.UtcNow;

        var user = User.Create(
            Username.Create(message.Username),
            Email.Create(message.Email),
            Password.Create(_hasher.Hash(message.Password)),
            now);

        var result = user.CreateSession(_tokens.GenerateRefreshToken(now).ToRefreshToken());

        if (result.Value is not { } session)
        {
            return ErrorOr<RegisterResponse>.From(result.Errors);
        }

        _context.Users.Add(user);

        await _context.SaveChangesAsync(token);

        return new RegisterResponse
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