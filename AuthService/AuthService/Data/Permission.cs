namespace AuthService.Data;

public enum PermissionType
{
    Menu,
    Action
}

public class Permission
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public PermissionType Type { get; set; }
    public int? ParentPermissionId { get; set; }

    public virtual Permission? ParentPermission { get; set; }
    public virtual ICollection<Permission> ChildPermissions { get; set; } = new List<Permission>();
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
