using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace UserManagerApi.Data;

public class AuditLog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long AuditLogId { get; set; }

    public DateTime InsertedDate { get; set; }

    [StringLength(255)]
    public string EntityType { get; set; } = null!;

    [Column("TableName")]
    [StringLength(255)]
    public string TableName { get; set; } = null!;

    public string PrimaryKey { get; set; } = null!;

    [StringLength(50)]
    public string Action { get; set; } = null!;

    public int? UserId { get; set; }

    public string Changes { get; set; } = null!;

    [StringLength(100)]
    public string TraceId { get; set; } = null!;

    [StringLength(100)]
    public string TransactionId { get; set; } = null!;
}
