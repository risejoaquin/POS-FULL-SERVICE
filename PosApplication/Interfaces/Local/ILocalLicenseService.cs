using System.Threading.Tasks;
using PosDomain.Entities;

namespace PosApplication.Interfaces.Local
{
    public interface ILocalLicenseService
    {
        Task<License?> GetCurrentLicenseAsync();
        Task<bool> ValidateLicenseKeyAsync(string licenseKey);
        Task<bool> ActivateLicenseAsync(string licenseKey);
    }
}
