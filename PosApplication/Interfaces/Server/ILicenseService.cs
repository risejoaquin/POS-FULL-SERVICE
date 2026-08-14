using PosDomain.Interfaces;
using System.Threading.Tasks;
using PosDomain.Entities;
using System;

namespace PosApplication.Interfaces.Server
{
    public interface ILicenseService
    {
        Task<(bool IsValid, string Error, License? License)> ValidateLicenseAsync(string licenseKey);
        Task<License> GenerateLicenseAsync(string tenantId, string description, int maxTerminals, int durationDays);
    }
}
