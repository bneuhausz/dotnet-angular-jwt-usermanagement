using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace UserManagerApi.Data;

public enum PermissionType
{
    Menu,
    Action
}

public class Permission
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    public int? ParentPermissionId { get; set; }
    [ForeignKey(nameof(ParentPermissionId))]
    public virtual Permission? ParentPermission { get; set; }

    [Required]
    [StringLength(20)]
    public PermissionType Type { get; set; }

    public virtual ICollection<Permission> ChildPermissions { get; set; } = new List<Permission>();
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
