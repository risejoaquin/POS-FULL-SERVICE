using PosDomain.Entities;

namespace PosApplication.DTOs.Local
{
    public class LoginResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public User? User { get; set; }
        public string? Token { get; set; }
        public string? TenantId { get; set; }
        public string? Username { get; set; }
        public string? Role { get; set; }
        public string? CurrentUserId { get; set; }
    }
}
