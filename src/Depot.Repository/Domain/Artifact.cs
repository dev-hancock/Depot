namespace Depot.Repository.Domain;

using ErrorOr;

public class Artifact
{
    private Artifact()
    {
        Id = Guid.NewGuid();
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public string ContentType { get; private set; }

    public string Extension { get; private set; }

    public string Hash { get; private set; }

    public long Size { get; private set; }

    public string Repository { get; private set; }

    public string Location { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public string CreatedBy { get; private set; }

    public void Move(string location)
    {
        Location = location;
    }

    public static ErrorOr<Artifact> FromStream(string name, string repository, long length, string type, string hash, string user,
        TimeProvider time)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Error.Validation();
        }

        if (length == 0)
        {
            return Error.Validation();
        }

        return new Artifact
        {
            Name = name,
            Repository = repository,
            ContentType = type,
            Extension = Path.GetExtension(name),
            Hash = hash,
            Size = length,
            CreatedAt = time.GetUtcNow(),
            CreatedBy = user
        };
    }
}