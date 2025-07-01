namespace Depot.Auth.Domain;

public sealed class Role
{
    public Guid Id { get; init; }

    public string Name { get; set; } = null!;

    public ICollection<UserRole> UserRoles { get; init; } = new List<UserRole>();
}