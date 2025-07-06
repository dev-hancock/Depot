namespace Depot.Repository.Domain;

public class Repository
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid PolicyId { get; set; }


    public string Name { get; set; }


    public Policy Policy { get; set; }


    public DateTimeOffset CreatedAt { get; set; }

    public string CreatedBy { get; set; }

    public List<Artifact> Artifacts { get; set; } = [];

    public void AddArtifact(Artifact artifact)
    {
        Artifacts.Add(artifact);
    }

    public void AddPolicy(Policy policy)
    {
        Policy = policy;
    }

    public void RemoveArtifact(Artifact artifact)
    {
        Artifacts.Remove(artifact);
    }
}