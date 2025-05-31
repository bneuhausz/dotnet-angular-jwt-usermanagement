using System.ComponentModel.DataAnnotations;

namespace UserManagerApi.Data;

public class User : AuditableEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string UserName { get; set; } = null!;

    [Required]
    [StringLength(256)]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    public string PasswordHash { get; set; } = null!;

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
