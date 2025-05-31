namespace AuthService.Data;

public abstract class AuditableEntity
{
    public bool IsDeleted { get; set; }
}
