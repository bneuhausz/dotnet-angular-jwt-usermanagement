using System.ComponentModel.DataAnnotations.Schema;

namespace AuthService.Data;

public class UserRefreshToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public virtual User User { get; set; } = null!;
    public string Token { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByToken { get; set; }

    [NotMapped]
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    [NotMapped]
    public bool IsRevoked => RevokedAt != null;
    [NotMapped]
    public bool IsActive => !IsRevoked && !IsExpired;
}
