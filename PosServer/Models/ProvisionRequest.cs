using System.ComponentModel.DataAnnotations;
namespace PosServer.Models
{
    public class ExtraUserDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }

    public class ProvisionRequest
    {
        [Required]
        public string ProvisionKey { get; set; } = string.Empty;
        
        [Required]
        public string TenantId { get; set; } = string.Empty;
        
        [Required]
        public string AdminUsername { get; set; } = string.Empty;
        
        [Required]
        public string AdminPassword { get; set; } = string.Empty;
        
        public string EmpUsername { get; set; } = string.Empty;
        public string EmpPassword { get; set; } = string.Empty;
        public System.Collections.Generic.List<ExtraUserDto> ExtraUsers { get; set; } = new();
    }
}
