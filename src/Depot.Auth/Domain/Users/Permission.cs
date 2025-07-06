namespace Depot.Auth.Domain.Users;

public class Permission
{
    public Guid Id { get; set; }


    public string Name { get; set; } = null!;


    public List<RolePermission> RolePermissions { get; set; } = [];
}