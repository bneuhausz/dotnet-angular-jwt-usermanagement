using System.ComponentModel.DataAnnotations.Schema;

namespace UserManagerApi.Data;

public abstract class AuditableEntity
{
    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public int? ModifiedBy { get; set; }

    [ForeignKey(nameof(CreatedBy))]
    public virtual User CreatedByUser { get; set; } = null!;

    [ForeignKey(nameof(ModifiedBy))]
    public virtual User? ModifiedByUser { get; set; }
}
