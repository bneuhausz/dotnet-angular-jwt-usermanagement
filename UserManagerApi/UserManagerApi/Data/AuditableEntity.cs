namespace UserManagerApi.Data;

public class AuditableEntity
{
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public Guid? ModifiedBy { get; set; }

    public User CreatedByUser { get; set; } = null!;
    public User? ModifiedByUser { get; set; }
}
