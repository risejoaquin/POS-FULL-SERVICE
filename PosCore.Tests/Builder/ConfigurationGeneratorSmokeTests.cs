using System.Text.Json;
using PosApplication.Models;
using PosBuilder;
using PosBuilder.Models;
using Xunit;

namespace PosCore.Tests.Builder;

public class ConfigurationGeneratorSmokeTests
{
    [Fact]
    public void GenerateAppSettings_Should_Produce_PosCore_Compatible_Config()
    {
        var generator = new ConfigurationGenerator();
        var model = new ConfigModel
        {
            ApiBaseUrl = "https://pos.example.com",
            TenantId = "tenant-test",
            LicenseKey = "LIC-test-123",
            CompanyName = "Tienda Demo",
            PrimaryColor = "#123456",
            LogoPath = "Assets/logo.png",
            EnableInventoryControl = true,
            EnableReports = true,
            EnableCredit = false,
            EnableMultiStore = false
        };

        var json = generator.GenerateAppSettings(model);
        generator.ValidateGeneratedAppSettings(json);

        var settings = JsonSerializer.Deserialize<AppSettings>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(settings);
        Assert.Equal("https://pos.example.com/", settings!.ApiSettings.BaseUrl);
        Assert.Equal("Data Source=pos_local.db;Default Timeout=30;", settings.DatabaseSettings.ConnectionString);
        Assert.Equal("tenant-test", settings.Tenant.CurrentTenantId);
        Assert.Equal("LIC-test-123", settings.License.LicenseKey);
        Assert.Equal("Tienda Demo", settings.WhiteLabel.CompanyName);
        Assert.Equal("#123456", settings.WhiteLabel.PrimaryColor);
        Assert.True(settings.Modules.EnableInventoryControl);
    }

    [Fact]
    public void GenerateAppSettings_Should_Reject_Missing_LicenseKey()
    {
        var generator = new ConfigurationGenerator();
        var model = new ConfigModel
        {
            ApiBaseUrl = "https://pos.example.com",
            TenantId = "tenant-test",
            CompanyName = "Tienda Demo",
            PrimaryColor = "#123456"
        };

        var exception = Assert.Throws<InvalidOperationException>(() => generator.GenerateAppSettings(model));
        Assert.Contains("LicenseKey", exception.Message);
    }
}
