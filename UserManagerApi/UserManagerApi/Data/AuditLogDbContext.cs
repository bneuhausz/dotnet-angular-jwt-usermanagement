using Microsoft.EntityFrameworkCore;

namespace UserManagerApi.Data;

public class AuditLogDbContext : DbContext
{
    public AuditLogDbContext(DbContextOptions<AuditLogDbContext> options) : base(options) {}

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.HasKey(e => e.AuditLogId);
            entity.Property(e => e.AuditLogId).ValueGeneratedOnAdd();
            entity.Property(e => e.InsertedDate).IsRequired();
        });
    }
}
