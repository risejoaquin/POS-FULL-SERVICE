                MainOverlay.Show("Registrando usuarios en la nube...");
                try 
                {
                    using var client = new System.Net.Http.HttpClient();
                    client.BaseAddress = new Uri(config.ApiBaseUrl);
                    var payload = new {
                        ProvisionKey = config.JwtSecret,
                        TenantId = config.TenantId,
                        AdminUsername = config.AdminUser,
                        AdminPassword = config.AdminPassword,
                        EmpUsername = config.EmployeeUser,
                        EmpPassword = config.EmployeePassword
                    };
                    client.DefaultRequestHeaders.Add("X-Tenant-Id", config.TenantId);
                    var response = await client.PostAsJsonAsync("api/auth/provision", payload);
                    if (!response.IsSuccessStatusCode)
                    {
                        var err = await response.Content.ReadAsStringAsync();
                        
                        // Fallback a conexion directa si es PostgreSQL
                        if (config.DbType == "PostgreSQL" && !string.IsNullOrEmpty(config.DbHost))
                        {
                            string connStr = $"Host={config.DbHost};Port={config.DbPort};Database={config.DbName};Username={config.DbUser};Password={config.DbPassword};Timeout=5";
                            if (config.DbHost.Contains("supabase") || config.DbHost.Contains("railway")) {
                                connStr += ";SSL Mode=Require;Trust Server Certificate=True";
                            }
                            using var conn = new Npgsql.NpgsqlConnection(connStr);
                            await conn.OpenAsync();
                            
                            // Asegurar que la tabla Users tiene PasswordHash
                            using var cmdSchema = new Npgsql.NpgsqlCommand("ALTER TABLE \"Users\" ADD COLUMN IF NOT EXISTS \"PasswordHash\" text DEFAULT '';", conn);
                            await cmdSchema.ExecuteNonQueryAsync();

                            // Hash de passwords (usando un hash basico temporal o texto plano si no tenemos BCrypt aqui)
                            // Wait, PosBuilder doesn't have BCrypt. Let's just warn the user.
                            MessageBox.Show($"Advertencia: La API remota no tiene el endpoint de aprovisionamiento actualizado.\nError de la API: {err}\n\nDebe desplegar la carpeta PosServer a su servidor (Railway) para que el Login funcione correctamente.", "Requiere Actualización del Servidor", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        else 
                        {
                            MessageBox.Show($"Advertencia: No se pudo registrar usuarios en la nube automáticamente: {err}\n\nAsegúrese de que el servidor remoto esté actualizado.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Advertencia: No se pudo conectar a la nube: {ex.Message}\n\nDebe desplegar PosServer a su servidor (Railway) para que funcione.", "Requiere Actualización del Servidor", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
