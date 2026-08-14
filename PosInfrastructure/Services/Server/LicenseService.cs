using PosDomain.Interfaces;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PosInfrastructure.Data.Server;
using PosDomain.Entities;

using PosApplication.Interfaces.Server;

namespace PosInfrastructure.Services.Server
{
    public class LicenseService : ILicenseService
    {
        private readonly CentralDbContext _context;

        public LicenseService(CentralDbContext context)
        {
            _context = context;
        }

        public async Task<(bool IsValid, string Error, License? License)> ValidateLicenseAsync(string licenseKey)
        {
            if (string.IsNullOrWhiteSpace(licenseKey))
            {
                return (false, "Clave de licencia vacía.", null);
            }

            var license = await _context.Licenses
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(l => l.LicenseKey == licenseKey);

            if (license == null)
            {
                return (false, "Licencia no encontrada.", null);
            }

            if (!license.IsActive)
            {
                return (false, "La licencia está desactivada.", null);
            }

            if (license.ValidUntil.ToUniversalTime() < DateTime.UtcNow)
            {
                return (false, "La licencia ha expirado.", null);
            }

            return (true, string.Empty, license);
        }

        public async Task<License> GenerateLicenseAsync(string tenantId, string description, int maxTerminals, int durationDays)
        {
            if (string.IsNullOrEmpty(tenantId))
            {
                throw new ArgumentException("TenantId is required to generate a license.");
            }

            var newLicense = new License
            {
                LicenseKey = "VAL-" + Guid.NewGuid().ToString("N").Substring(0, 16).ToUpper(),
                TenantId = tenantId,
                Description = description ?? "Licencia Generada Manualmente",
                IsActive = true,
                MaxTerminals = maxTerminals > 0 ? maxTerminals : 1,
                ValidUntil = durationDays > 0 ? DateTime.UtcNow.AddDays(durationDays) : DateTime.UtcNow.AddYears(1),
                CreatedAt = DateTime.UtcNow
            };

            _context.Licenses.Add(newLicense);
            await _context.SaveChangesAsync();

            return newLicense;
        }
    }
}
