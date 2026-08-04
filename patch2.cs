                MainOverlay.Show("Registrando usuarios en la nube...");
                try 
                {
                    using var client = new System.Net.Http.HttpClient();
                    client.BaseAddress = new Uri(config.ApiUrl);
                    var payload = new {
                        ProvisionKey = config.JwtSecret,
                        TenantId = config.TenantId,
                        AdminUsername = config.AdminUser,
                        AdminPassword = config.AdminPassword,
                        EmpUsername = config.EmployeeUser,
                        EmpPassword = config.EmployeePassword
                    };
                    var response = await client.PostAsJsonAsync("api/auth/provision", payload);
                    if (!response.IsSuccessStatusCode)
                    {
                        var err = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Advertencia: No se pudo registrar usuarios en la nube automáticamente: {err}", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Advertencia: No se pudo conectar a la nube: {ex.Message}", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
