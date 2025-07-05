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
}

public class Policy
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }


    public string Name { get; set; }


    // Rules
    public bool Immutable { get; set; }

    public int? RetentionDays { get; set; }

    public int? RetentionVersions { get; set; }


    public string FileTypes { get; set; }


    public DateTimeOffset CreatedAt { get; set; }

    public string CreatedBy { get; set; }
}