using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PosBuilder.Models;
using PosApplication.Models;

namespace PosBuilder
{
    public class ConfigurationGenerator
    {
        public string GenerateAppSettings(ConfigModel model)
        {
            var config = BuildAppSettings(model);
            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(config, options);
        }

        public AppSettings BuildAppSettings(ConfigModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            ValidateModel(model);

            return new AppSettings
            {
                ApiSettings = new ApiSettings
                {
                    BaseUrl = NormalizeBaseUrl(model.ApiBaseUrl)
                },
                DatabaseSettings = new DatabaseSettings
                {
                    ConnectionString = "Data Source=pos_local.db;Default Timeout=30;"
                },
                WhiteLabel = new WhiteLabelSettings
                {
                    CompanyName = model.CompanyName.Trim(),
                    PrimaryColor = model.PrimaryColor.Trim(),
                    LogoPath = model.LogoPath?.Trim() ?? string.Empty
                },
                Modules = new ModuleSettings
                {
                    EnableInventoryControl = model.EnableInventoryControl,
                    EnableTableManagement = model.EnableMultiStore,
                    EnableCoupons = model.EnableCredit,
                    EnableLoyalty = false
                },
                Tenant = new TenantSettings
                {
                    CurrentTenantId = model.TenantId.Trim()
                },
                Printer = new PrinterSettings
                {
                    PortName = "POS-80",
                    PrintLogo = !string.IsNullOrWhiteSpace(model.LogoPath)
                },
                License = new LicenseSettings
                {
                    LicenseKey = model.LicenseKey.Trim()
                },
                Security = new SecuritySettings
                {
                    ManagerPin = string.Empty
                },
                Tax = new TaxSettings
                {
                    DefaultTaxRate = 0.16m,
                    TaxId = string.Empty,
                    BusinessAddress = string.Empty,
                    ReceiptFooter = "Gracias por su compra!"
                },
                PaymentMethods = new PaymentMethodSettings
                {
                    EnableCash = true,
                    EnableCard = true,
                    EnableTransfer = false
                }
            };
        }

        public void ValidateGeneratedAppSettings(string json)
        {
            var settings = JsonSerializer.Deserialize<AppSettings>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new InvalidOperationException("Generated appsettings.json is not a valid AppSettings document.");

            if (string.IsNullOrWhiteSpace(settings.ApiSettings.BaseUrl))
                throw new InvalidOperationException("Generated appsettings.json is missing ApiSettings.BaseUrl.");

            if (string.IsNullOrWhiteSpace(settings.DatabaseSettings.ConnectionString))
                throw new InvalidOperationException("Generated appsettings.json is missing DatabaseSettings.ConnectionString.");

            if (string.IsNullOrWhiteSpace(settings.Tenant.CurrentTenantId))
                throw new InvalidOperationException("Generated appsettings.json is missing Tenant.CurrentTenantId.");

            if (string.IsNullOrWhiteSpace(settings.License.LicenseKey))
                throw new InvalidOperationException("Generated appsettings.json is missing License.LicenseKey.");
        }

        public async Task<bool> WriteWithIntegrityValidationAsync(string path, string content, int retries = 3)
        {
            int attempt = 0;
            while (attempt < retries)
            {
                try
                {
                    var dir = Path.GetDirectoryName(path);
                    if (!string.IsNullOrEmpty(dir))
                        Directory.CreateDirectory(dir);
                    await File.WriteAllTextAsync(path, content, Encoding.UTF8);
                    
                    // Validate hash
                    string writtenHash = ComputeSha256(await File.ReadAllTextAsync(path, Encoding.UTF8));
                    string expectedHash = ComputeSha256(content);
                    
                    if (writtenHash == expectedHash)
                    {
                        return true;
                    }
                }
                catch (Exception)
                {
                    // Log or handle exception if needed
                }
                attempt++;
            }
            return false;
        }

        private string ComputeSha256(string text)
        {
            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(text));
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        private static void ValidateModel(ConfigModel model)
        {
            if (string.IsNullOrWhiteSpace(model.ApiBaseUrl))
                throw new InvalidOperationException("ApiBaseUrl is required.");

            if (string.IsNullOrWhiteSpace(model.TenantId))
                throw new InvalidOperationException("TenantId is required.");

            if (string.IsNullOrWhiteSpace(model.LicenseKey))
                throw new InvalidOperationException("LicenseKey is required.");

            if (string.IsNullOrWhiteSpace(model.CompanyName))
                throw new InvalidOperationException("CompanyName is required.");

            if (string.IsNullOrWhiteSpace(model.PrimaryColor))
                throw new InvalidOperationException("PrimaryColor is required.");
        }

        private static string NormalizeBaseUrl(string baseUrl)
        {
            var normalized = baseUrl.Trim();
            if (!normalized.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !normalized.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                normalized = "https://" + normalized.TrimStart('/');
            }

            return normalized.EndsWith("/", StringComparison.Ordinal) ? normalized : normalized + "/";
        }
    }
}
