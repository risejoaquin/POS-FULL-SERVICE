#!/bin/bash
cat << 'INNER_EOF' > PosBuilder/MainWindow.xaml.cs.patch
--- PosBuilder/MainWindow.xaml.cs
+++ PosBuilder/MainWindow.xaml.cs
@@ -124,31 +124,24 @@
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
-            string envPath = System.IO.Path.Combine(outputDir, "railway.env.example");
-            string sqlPath = System.IO.Path.Combine(outputDir, "init.sql");
 
             await Task.Delay(500);
             
-            MainOverlay.Show("Generando archivos de configuración (appsettings.json)...");
-            bool ok1 = await generator.WriteWithIntegrityValidationAsync(appSettingsPath, generator.GenerateAppSettings(config));
-            
-            await Task.Delay(500);
-            MainOverlay.Show("Generando script de base de datos SQL e Inyección de Dependencias...");
-            bool ok2 = await generator.WriteWithIntegrityValidationAsync(envPath, generator.GenerateEnvFile(config));
-            bool ok3 = await generator.WriteWithIntegrityValidationAsync(sqlPath, generator.GenerateSqlScript(config));
-
-            if (ok1 && ok2 && ok3)
-            {
-                MainOverlay.Show("Registrando usuarios en la nube y generando licencia...");
+            MainOverlay.Show("Provisionando Tenant en el servidor...");
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
                             "Fallo al aprovisionar usuarios en la nube (Error " + (int)response.StatusCode + ").\n" +
                             "Detalles: " + err + "\n\n" +
                             "Esto ocurre porque la API remota (Railway) aún no ha sido actualizada con el nuevo código.\n\n" +
                             "PASO REQUERIDO:\nDebe desplegar los cambios de 'PosServer' a Railway.", 
                             "Requiere Actualizar Servidor", MessageBoxButton.OK, MessageBoxImage.Warning);
+                            MainOverlay.Hide();
+                            return;
                     }
+                    
+                    var result = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
+                    string licenseKey = result.GetProperty("licenseKey").GetString();
+                    config.LicenseKey = licenseKey;
+                    
+                    MainOverlay.Show("Generando archivos de configuración local (appsettings.json)...");
+                    bool ok1 = await generator.WriteWithIntegrityValidationAsync(appSettingsPath, generator.GenerateAppSettings(config));
+
+                    if (!ok1) {
+                        NotificationService.Instance.ShowError("Error al guardar archivo appsettings.");
+                        MainOverlay.Hide();
+                        return;
+                    }
+                    
+                    // Try to place it directly in PosCore
+                    try {
+                       string corePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.Environment.CurrentDirectory, "..", "..", "..", "..", "PosCore"));
+                       if (!System.IO.Directory.Exists(corePath)) { corePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.Environment.CurrentDirectory, "..", "PosCore")); }
+                       if (!System.IO.Directory.Exists(corePath)) { corePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.Environment.CurrentDirectory, "PosCore")); }
+                       if (System.IO.Directory.Exists(corePath)) {
+                           System.IO.File.Copy(appSettingsPath, System.IO.Path.Combine(corePath, "appsettings.json"), true);
+                       }
+                    } catch {}
+
+                    MainOverlay.Hide();
+                    NotificationService.Instance.ShowSuccess("Instalación Completada Exitosamente.");
+
+                    string creds = $"Administrador: {config.AdminUser} / {config.AdminPassword}\nEmpleado: {config.EmployeeUser} / {config.EmployeePassword}\nLicencia: {licenseKey}";
+                    var modal = new SuccessModal(outputDir, creds);
+                    modal.Owner = this;
+                    modal.ShowDialog();
+                    Close();
                 }
                 catch (Exception ex)
                 {
                     MessageBox.Show(
                         $"No se pudo conectar a la API para aprovisionar: {ex.Message}\n\n" +
                         "PASO REQUERIDO:\nDebe desplegar la carpeta 'PosServer' a su servidor (Railway).", 
                         "Requiere Actualizar Servidor", MessageBoxButton.OK, MessageBoxImage.Warning);
+                    MainOverlay.Hide();
                 }
-
-                MainOverlay.Show("Compilando binarios de cliente POS (PosCore). Esto puede tomar unos segundos...");
-                MainOverlay.ShowLog();
-                try 
-                {
-                    // Copy appsettings.json to PosCore before compiling
-                    string corePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.Environment.CurrentDirectory, "..", "..", "..", "..", "PosCore"));
-                    if (!System.IO.Directory.Exists(corePath)) { corePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.Environment.CurrentDirectory, "..", "PosCore")); }
-                    if (!System.IO.Directory.Exists(corePath)) { corePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.Environment.CurrentDirectory, "PosCore")); }
-
-                    string serverPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.Environment.CurrentDirectory, "..", "..", "..", "..", "PosServer"));
-                    if (!System.IO.Directory.Exists(serverPath)) { serverPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.Environment.CurrentDirectory, "..", "PosServer")); }
-                    if (!System.IO.Directory.Exists(serverPath)) {
-                        serverPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.Environment.CurrentDirectory, "PosServer")); // Fallback 2
-                    }
-
-                    if (System.IO.Directory.Exists(serverPath)) 
-                    {
-                        string serverAppSettingsJson = generator.GenerateServerAppSettings(config);
-                        System.IO.File.WriteAllText(System.IO.Path.Combine(serverPath, "appsettings.Development.json"), serverAppSettingsJson);
-                        System.IO.File.WriteAllText(System.IO.Path.Combine(serverPath, "appsettings.json"), serverAppSettingsJson);
-                    }
-                    
-                    string logFilePath = System.IO.Path.Combine(outputDir, "build.log");
-                    
-                    if (System.IO.Directory.Exists(corePath)) 
-                    {
-                        System.IO.File.Copy(appSettingsPath, System.IO.Path.Combine(corePath, "appsettings.json"), true);
-                        
-                        var psi = new System.Diagnostics.ProcessStartInfo
-                        {
-                            FileName = "dotnet",
-                            Arguments = $"publish \"{corePath}\" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o \"{System.IO.Path.Combine(outputDir, "PosClient")}\"",
-                            UseShellExecute = false,
-                            CreateNoWindow = true,
-                            RedirectStandardOutput = true,
-                            RedirectStandardError = true
-                        };
-
-                        using var process = new System.Diagnostics.Process { StartInfo = psi };
-
-                        var fullOutput = new System.Text.StringBuilder();
-                        var fullError = new System.Text.StringBuilder();
-
-                        process.OutputDataReceived += (s, ev) => {
-                            if (ev.Data != null) {
-                                MainOverlay.AppendLog(ev.Data);
-                                fullOutput.AppendLine(ev.Data);
-                            }
-                        };
-                        process.ErrorDataReceived += (s, ev) => {
-                            if (ev.Data != null) {
-                                MainOverlay.AppendLog("ERROR: " + ev.Data);
-                                fullError.AppendLine(ev.Data);
-                            }
-                        };
-
-                        process.Start();
-                        process.BeginOutputReadLine();
-                        process.BeginErrorReadLine();
-                        
-                        await process.WaitForExitAsync();
-                        
-                        await System.IO.File.WriteAllTextAsync(logFilePath, $"=== Salida Estándar ===\n{fullOutput.ToString()}\n=== Salida de Error ===\n{fullError.ToString()}");
-                        
-                        if (process.ExitCode != 0)
-                        {
-                            throw new Exception($"El proceso de compilación falló con código {process.ExitCode}. Revisa build.log para más detalles.");
-                        }
-                    }
-                }
-                catch (Exception ex)
-                {
-                    NotificationService.Instance.ShowError("Error al compilar PosCore: " + ex.Message);
-                }
-
-                MainOverlay.Hide();
-                NotificationService.Instance.ShowSuccess("Archivos generados exitosamente.");
-
-                string logPath = System.IO.Path.Combine(outputDir, "validation.log");
-                await System.IO.File.WriteAllTextAsync(logPath, $"Configuración validada exitosamente: {DateTime.Now}");
-
-                string creds = $"Administrador: {config.AdminUser} / {config.AdminPassword}\nEmpleado: {config.EmployeeUser} / {config.EmployeePassword}\n\nEl cliente compilado está en la carpeta Output/PosClient";
-                
-                var modal = new SuccessModal(outputDir, creds);
-                modal.Owner = this;
-                modal.ShowDialog();
-                
-                try
-                {
-                    string corePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.Environment.CurrentDirectory, "..", "..", "..", "..", "PosCore"));
-                    if (!System.IO.Directory.Exists(corePath)) { corePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.Environment.CurrentDirectory, "..", "PosCore")); }
-                    if (!System.IO.Directory.Exists(corePath)) { corePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.Environment.CurrentDirectory, "PosCore")); }
-
-                    string serverPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.Environment.CurrentDirectory, "..", "..", "..", "..", "PosServer"));
-                    if (!System.IO.Directory.Exists(serverPath)) { serverPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.Environment.CurrentDirectory, "..", "PosServer")); }
-                    if (!System.IO.Directory.Exists(serverPath)) { serverPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.Environment.CurrentDirectory, "PosServer")); }
-
-                    if (System.IO.Directory.Exists(serverPath))
-                    {
-                        var serverProcess = new System.Diagnostics.ProcessStartInfo
-                        {
-                            FileName = "dotnet",
-                            Arguments = $"run --project \"{serverPath}\"",
-                            UseShellExecute = true
-                        };
-                        System.Diagnostics.Process.Start(serverProcess);
-                    }
-                    
-                    string clientExe = System.IO.Path.Combine(outputDir, "PosClient", "PosCore.exe");
-                    if (System.IO.File.Exists(clientExe))
-                    {
-                        var clientProcess = new System.Diagnostics.ProcessStartInfo
-                        {
-                            FileName = clientExe,
-                            UseShellExecute = true,
-                            WorkingDirectory = System.IO.Path.Combine(outputDir, "PosClient")
-                        };
-                        System.Diagnostics.Process.Start(clientProcess);
-                    }
-                    else if (System.IO.Directory.Exists(corePath))
-                    {
-                         var clientFallbackProcess = new System.Diagnostics.ProcessStartInfo
-                         {
-                             FileName = "dotnet",
-                             Arguments = $"run --project \"{corePath}\"",
-                             UseShellExecute = true
-                         };
-                         System.Diagnostics.Process.Start(clientFallbackProcess);
-                    }
-                }
-                catch (Exception ex)
-                {
-                    System.Windows.MessageBox.Show("Error al iniciar las aplicaciones: " + ex.Message, "Ejecución Automática", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
-                }
-                
-                Close();
-            }
-            else
-            {
-                NotificationService.Instance.ShowError("Error de integridad al generar los archivos.");
-            }
         }
     }
INNER_EOF
patch -u PosBuilder/MainWindow.xaml.cs < PosBuilder/MainWindow.xaml.cs.patch
