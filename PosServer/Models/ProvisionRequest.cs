using System.ComponentModel.DataAnnotations;
namespace PosServer.Models
{
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
    }
}
