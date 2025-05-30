using Microsoft.EntityFrameworkCore;
using UserManagerApi.Services;

namespace UserManagerApi.Data;

public class UsersDbContext : DbContext
{
    private readonly CurrentUserService _currentUserService;

    public UsersDbContext(DbContextOptions<UsersDbContext> options, CurrentUserService currentUserService) : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
        .Where(e => typeof(AuditableEntity).IsAssignableFrom(e.ClrType)))
        {
            var entity = modelBuilder.Entity(entityType.ClrType);

            entity.Property(nameof(AuditableEntity.IsDeleted)).HasDefaultValue(false);
            entity.Property(nameof(AuditableEntity.CreatedAt)).HasDefaultValueSql("SYSDATETIME()");

            entity.HasOne(typeof(User), nameof(AuditableEntity.CreatedByUser))
            .WithMany()
            .HasForeignKey(nameof(AuditableEntity.CreatedBy))
            .HasConstraintName($"FK_{entityType.GetTableName()}_CreatedBy")
            .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(typeof(User), nameof(AuditableEntity.ModifiedByUser))
                .WithMany()
                .HasForeignKey(nameof(AuditableEntity.ModifiedBy))
                .HasConstraintName($"FK_{entityType.GetTableName()}_ModifiedBy")
                .OnDelete(DeleteBehavior.Restrict);
        }

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");

            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserName).IsUnique().HasDatabaseName("UQ_Users_UserName");
            entity.HasIndex(e => e.Email).IsUnique().HasDatabaseName("UQ_Users_Email");

            entity.Property(e => e.UserName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.PasswordHash).IsRequired();
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");

            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique().HasDatabaseName("UQ_Roles_Name");

            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("UserRoles");

            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.RoleId }).IsUnique().HasDatabaseName("UQ_UserRoles_UserId_RoleId");

            entity.HasOne(e => e.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(e => e.UserId)
                .HasConstraintName("FK_UserRoles_Users");

            entity.HasOne(e => e.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(e => e.RoleId)
                .HasConstraintName("FK_UserRoles_Roles");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("Permissions");

            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique().HasDatabaseName("UQ_Permissions_Name");

            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(20);

            entity.HasOne(e => e.ParentPermission)
                .WithMany(p => p.ChildPermissions)
                .HasForeignKey(e => e.ParentPermissionId)
                .HasConstraintName("FK_Permissions_Parent");
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("RolePermissions");

            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.RoleId, e.PermissionId }).IsUnique().HasDatabaseName("UQ_RolePermissions_RoleId_PermissionId");

            entity.HasOne(e => e.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(e => e.RoleId)
                .HasConstraintName("FK_RolePermissions_Role");

            entity.HasOne(e => e.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(e => e.PermissionId)
                .HasConstraintName("FK_RolePermissions_Permission");
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var userId = _currentUserService.UserId ?? throw new InvalidOperationException("User ID not set");

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.CreatedBy = userId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.ModifiedAt = now;
                entry.Entity.ModifiedBy = userId;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
