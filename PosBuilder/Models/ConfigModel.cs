using System.Collections.Generic;

namespace PosBuilder.Models
{
    public class ConfigModel
    {
        public string ApiBaseUrl { get; set; } = "";
        public string CompanyName { get; set; } = "";
        public string PrimaryColor { get; set; } = "";
        public string LogoPath { get; set; } = "";
        public string TenantId { get; set; } = "";
        public string LicenseKey { get; set; } = string.Empty; // PHASE 7F PosBuilder nullability hygiene applied
        public string BusinessType { get; set; } = "";
        
        public string DbType { get; set; } = "";
        public string DbHost { get; set; } = "";
        public string DbPort { get; set; } = "";
        public string DbUser { get; set; } = "";
        public string DbPassword { get; set; } = "";
        public string DbName { get; set; } = "";
        public string ProvisionKey { get; set; } = "";
        
        public string AdminUser { get; set; } = "";
        public string AdminPassword { get; set; } = "";
        public string EmployeeUser { get; set; } = "";
        public string EmployeePassword { get; set; } = "";
        public string Environment { get; set; } = "";
        public List<UserModel> ExtraUsers { get; set; } = new List<UserModel>();
    }
}
