namespace Depot.Auth.Features.Users.Me;

using System.Reactive.Linq;
using Domain.Auth;
using Domain.Users.Errors;
using ErrorOr;
using Mestra.Abstractions;
using Microsoft.EntityFrameworkCore;
using Middleware;
using Persistence;

public class MeHandler : IMessageHandler<MeQuery, ErrorOr<MeResponse>>
{
    private readonly AuthDbContext _context;

    private readonly IUserContext _user;

    public MeHandler(AuthDbContext context, IUserContext user)
    {
        _context = context;
        _user = user;
    }

    public IObservable<ErrorOr<MeResponse>> Handle(MeQuery message)
    {
        return Observable.FromAsync(token => Handle(message, token));
    }

    private async Task<ErrorOr<MeResponse>> Handle(MeQuery _, CancellationToken token)
    {
        var user = await _context.Users
            .Where(x => x.Id == new UserId(_user.UserId))
            .SingleOrDefaultAsync(token);

        if (user is null)
        {
            return Errors.UserNotFound();
        }

        return new MeResponse
        {
            Username = user.Username
        };
    }
}