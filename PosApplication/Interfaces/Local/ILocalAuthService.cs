using System.Threading.Tasks;
using PosApplication.DTOs.Local;

namespace PosApplication.Interfaces.Local
{
    public interface ILocalAuthService
    {
        Task<LoginResult> AuthenticateLocalUserAsync(string username, string passwordOrPin);
        Task<LoginResult> CacheCloudLoginAsync(string username, string passwordOrPin, string tenantId, string role);
        Task<bool> ValidateManagerOverrideAsync(string managerUsername, string managerPin);
        Task MigrateAdminIfNeededAsync();
    }
}
