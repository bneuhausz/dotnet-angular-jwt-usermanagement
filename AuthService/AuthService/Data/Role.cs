namespace AuthService.Data;

public class Role : AuditableEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
