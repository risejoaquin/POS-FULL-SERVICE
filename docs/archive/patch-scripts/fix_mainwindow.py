import re

with open('PosBuilder/MainWindow.xaml.cs', 'r', encoding='utf-8') as f:
    content = f.read()

# Find Generate_Click method
match = re.search(r'private async void Generate_Click\(object sender, RoutedEventArgs e\)\s*\{(.*?)public class StepIndicator', content, re.DOTALL)

if match:
    new_method = """
        private async void Generate_Click(object sender, RoutedEventArgs e)
        {
            var config = new PosBuilder.Models.ConfigModel
            {
                ApiBaseUrl = _viewModel.ApiUrl,
                DbType = _viewModel.DbType,
                DbHost = _viewModel.DbHost,
                DbPort = _viewModel.DbPort,
                DbUser = _viewModel.DbUser,
                DbPassword = _viewModel.DbPassword,
                DbName = _viewModel.DbName,
                ProvisionKey = _viewModel.ProvisionKey,
                TenantId = _viewModel.TenantId,
                CompanyName = _viewModel.BrandingName,
                PrimaryColor = _viewModel.BrandingColor,
                LogoPath = _viewModel.BrandingLogoPath,
                AdminUser = _viewModel.AdminUser,
                AdminPassword = _viewModel.AdminPassword,
                EmployeeUser = _viewModel.EmployeeUser,
                EmployeePassword = _viewModel.EmployeePassword,
                ExtraUsers = _viewModel.ExtraUsers.ToList(),
                BusinessType = _viewModel.BusinessType,
                Environment = _viewModel.Environment
            };

            var generator = new ConfigurationGenerator();
            string outputDir = System.IO.Path.Combine(System.Environment.CurrentDirectory, "Output");
            if (!System.IO.Directory.Exists(outputDir))
                System.IO.Directory.CreateDirectory(outputDir);

            if (!string.IsNullOrWhiteSpace(config.LogoPath) && System.IO.File.Exists(config.LogoPath))
            {
                try
                {
                    string ext = System.IO.Path.GetExtension(config.LogoPath);
                    string destPath = System.IO.Path.Combine(outputDir, "PosClient", "logo" + ext);
                    if (!System.IO.Directory.Exists(System.IO.Path.Combine(outputDir, "PosClient"))) System.IO.Directory.CreateDirectory(System.IO.Path.Combine(outputDir, "PosClient"));
                    System.IO.File.Copy(config.LogoPath, destPath, true);
                    config.LogoPath = "logo" + ext; // Set to relative path
                }
                catch (Exception) { }
            }
            
            string appSettingsPath = System.IO.Path.Combine(outputDir, "appsettings.json");

            MainOverlay.Show("Provisionando Tenant en el servidor...");
            try 
            {
                using var client = new System.Net.Http.HttpClient();
                client.BaseAddress = new Uri(config.ApiBaseUrl);

                var payload = new {
                    ProvisionKey = config.ProvisionKey,
                    TenantId = config.TenantId,
                    AdminUsername = config.AdminUser,
                    AdminPassword = config.AdminPassword,
                    EmpUsername = config.EmployeeUser,
                    EmpPassword = config.EmployeePassword,
                    ExtraUsers = config.ExtraUsers
                };

                client.DefaultRequestHeaders.Add("X-Tenant-Id", config.TenantId);
                var response = await client.PostAsJsonAsync("api/auth/provision", payload);
                
                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    MessageBox.Show(
                        "Fallo al aprovisionar usuarios en la nube (Error " + (int)response.StatusCode + ").\\n" +
                        "Detalles: " + err + "\\n\\n" +
                        "Esto ocurre porque la API remota (Railway) aún no ha sido actualizada con el nuevo código.\\n\\n" +
                        "PASO REQUERIDO:\\nDebe desplegar los cambios de 'PosServer' a Railway.", 
                        "Requiere Actualizar Servidor", MessageBoxButton.OK, MessageBoxImage.Warning);
                    MainOverlay.Hide();
                    return;
                }
                
                var result = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
                string licenseKey = result.GetProperty("licenseKey").GetString();
                config.LicenseKey = licenseKey;
                
                MainOverlay.Show("Generando archivos de configuración local (appsettings.json)...");
                bool ok1 = await generator.WriteWithIntegrityValidationAsync(appSettingsPath, generator.GenerateAppSettings(config));

                if (!ok1) {
                    NotificationService.Instance.ShowError("Error al guardar archivo appsettings.");
                    MainOverlay.Hide();
                    return;
                }
                
                try {
                   string corePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.Environment.CurrentDirectory, "..", "..", "..", "..", "PosCore"));
                   if (!System.IO.Directory.Exists(corePath)) { corePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.Environment.CurrentDirectory, "..", "PosCore")); }
                   if (!System.IO.Directory.Exists(corePath)) { corePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.Environment.CurrentDirectory, "PosCore")); }
                   if (System.IO.Directory.Exists(corePath)) {
                       System.IO.File.Copy(appSettingsPath, System.IO.Path.Combine(corePath, "appsettings.json"), true);
                   }
                } catch {}

                MainOverlay.Hide();
                NotificationService.Instance.ShowSuccess("Instalación Completada Exitosamente.");

                string creds = $"Administrador: {config.AdminUser} / {config.AdminPassword}\\nEmpleado: {config.EmployeeUser} / {config.EmployeePassword}\\nLicencia: {licenseKey}";
                var modal = new SuccessModal(outputDir, creds);
                modal.Owner = this;
                modal.ShowDialog();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"No se pudo conectar a la API para aprovisionar: {ex.Message}\\n\\n" +
                    "PASO REQUERIDO:\\nDebe desplegar la carpeta 'PosServer' a su servidor.", 
                    "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Warning);
                MainOverlay.Hide();
            }
        }
    }

    public class StepIndicator"""

    content = content[:match.start()] + new_method + content[match.end()-28:]
    with open('PosBuilder/MainWindow.xaml.cs', 'w', encoding='utf-8') as f:
        f.write(content)
