namespace UserManagerApi.Data;

public class UserRole : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }

    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}
