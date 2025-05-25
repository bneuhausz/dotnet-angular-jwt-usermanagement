namespace AuthService.Data;

public class Permission : AuditableEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public Guid? ParentPermissionId { get; set; }
    public string Type { get; set; } = null!; // 'Menu', 'SubMenu', or 'Action'

    public Permission? ParentPermission { get; set; }
    public ICollection<Permission> ChildPermissions { get; set; } = new List<Permission>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
