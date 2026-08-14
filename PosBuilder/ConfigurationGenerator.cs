using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PosBuilder.Models;

namespace PosBuilder
{
    public class ConfigurationGenerator
    {
        public string GenerateAppSettings(ConfigModel model)
        {
            var config = new
            {
                Api = new
                {
                    BaseUrl = model.ApiBaseUrl
                },
                Tenant = new
                {
                    Id = model.TenantId
                },
                Device = new
                {
                    Id = Guid.NewGuid().ToString()
                },
                License = new 
                {
                    Key = model.LicenseKey
                }
            };
            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(config, options);
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
    }
}
