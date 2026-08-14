using System;

namespace PosDomain.Entities;

public class User
{
    public int Id { get; set; }
    public string? Username { get; set; } = string.Empty;
    public string? PasswordHash { get; set; } = string.Empty;
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string? Pin { get; set; } = string.Empty;
    public string? TenantId { get; set; } = string.Empty;
    public string? Role { get; set; } = "Admin";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    [System.ComponentModel.DataAnnotations.Timestamp]
    public uint RowVersion { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
