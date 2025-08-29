using System.Reactive.Linq;
using Depot.Repository.Domain;
using Depot.Repository.Persistence;
using Depot.Storage;
using ErrorOr;
using Mestra.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Depot.Repository.Handlers;

public class UploadHandler : IMessageHandler<UploadHandler.Request, ErrorOr<Artifact>>
{
    private readonly IDbContextFactory<RepoDbContext> _factory;

    private readonly IStorageHasher _hasher;

    private readonly IStorage _storage;

    private readonly TimeProvider _time;

    public UploadHandler(IDbContextFactory<RepoDbContext> factory, IStorage storage, IStorageHasher hasher, TimeProvider time)
    {
        _factory = factory;
        _storage = storage;
        _hasher = hasher;
        _time = time;
    }

    public IObservable<ErrorOr<Artifact>> Handle(Request message)
    {
        return Observable.FromAsync(token => Handle(message, token));
    }

    private async Task<ErrorOr<Artifact>> Handle(Request message, CancellationToken token)
    {
        await using var context = await _factory.CreateDbContextAsync(token);

        var repo = await context.Repositories
            .Where(x => x.TenantId == message.TenantId)
            .Where(x => x.Name == message.Repository)
            .SingleOrDefaultAsync(token);

        if (repo is null)
        {
            return Error.NotFound();
        }

        var hash = await _hasher.Hash(message.Content, token);

        var result = Artifact.FromStream(
            message.FileName,
            message.Repository,
            message.Content.Length,
            message.ContentType,
            hash,
            message.User,
            _time);

        if (result.IsError)
        {
            return ErrorOr<Artifact>.From(result.Errors);
        }

        var artifact = result.Value;

        var location = await _storage.SaveAsync(artifact.Id, artifact.Repository, artifact.Extension, message.Content, token);

        artifact.Move(location);

        repo.AddArtifact(artifact);

        try
        {
            context.Artifacts.Add(artifact);

            await context.SaveChangesAsync(token);
        }
        catch (Exception)
        {
            await _storage.DeleteAsync(location, CancellationToken.None);

            throw;
        }

        return artifact;
    }

    public sealed record Request(
        Guid TenantId,
        Guid UserId,
        string FileName,
        string Path,
        string Repository,
        Stream Content,
        string ContentType,
        string User)
        : IRequest<ErrorOr<Artifact>>;
}