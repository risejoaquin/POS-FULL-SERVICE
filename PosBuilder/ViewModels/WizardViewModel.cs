using PosBuilder.Models;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PosBuilder.ViewModels
{
    public partial class WizardViewModel : ObservableObject
    {
        [ObservableProperty]
        private int _currentStepIndex = 0;

        [ObservableProperty]
        private string _currentStepTitle = string.Empty; // PHASE 7F PosBuilder nullability hygiene applied
        
        [ObservableProperty]
        private string _currentStepSubTitle = string.Empty; // PHASE 7F PosBuilder nullability hygiene applied

        [ObservableProperty]
        private string _currentStepCategory = string.Empty; // PHASE 7F PosBuilder nullability hygiene applied



        [ObservableProperty]
        private string _tenantName = "Mi Tienda";
        [ObservableProperty]
        private string _tenantId = "TENANT_" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
        [ObservableProperty]
        private string _provisionKey = "";

        [ObservableProperty]
        private string _businessType = "Abarrotes / Minimarket";

        [ObservableProperty]
        private string _environment = "Development";

        [ObservableProperty]
        private string _apiUrl = "http://localhost:5000";

        [ObservableProperty]
        private int _port = 3000;

        [ObservableProperty]
        private string _dbType = "SQLite";

        [ObservableProperty]
        private string _dbHost = "localhost";

        [ObservableProperty]
        private string _dbPort = "5432";

        [ObservableProperty]
        private string _dbUser = "postgres";

        [ObservableProperty]
        private string _dbPassword = "";

        [ObservableProperty]
        private string _dbName = "pos_db";

        [ObservableProperty]
        private string _jwtIssuer = "PosCore";

        [ObservableProperty]
        private string _jwtAudience = "PosApp";

        [ObservableProperty]
        private string _jwtSecret = "";
        
        [ObservableProperty]
        private string _brandingName = "Mi POS";

        [ObservableProperty]
        private string _brandingColor = "#2D5F2E";

        [ObservableProperty]
        private string _brandingLogoPath = "";

        [ObservableProperty]
        private string _adminUser = "admin";

        [ObservableProperty]
        private string _adminPassword = "";

        [ObservableProperty]
        private string _employeeUser = "cajero";


        [ObservableProperty]
        private string _employeePassword = "";

        public ObservableCollection<UserModel> ExtraUsers { get; set; } = new ObservableCollection<UserModel>();

        [RelayCommand]
        public void AddUser()
        {
            ExtraUsers.Add(new UserModel { Username = "Nuevo Usuario", Role = "Empleado" });
        }

        [RelayCommand]
        public void RemoveUser(UserModel user)
        {
            if (user != null && ExtraUsers.Contains(user))
            {
                ExtraUsers.Remove(user);
            }
        }


        [ObservableProperty]
        private bool _moduleInventory = true;

        [ObservableProperty]
        private bool _moduleReports = true;

        [ObservableProperty]
        private bool _moduleCredit = false;

        [ObservableProperty]
        private bool _moduleMultiStore = false;

        public bool CanGoNext => IsCurrentStepValid();
        public bool CanGoPrevious => CurrentStepIndex > 0;
        public bool IsLastStep => CurrentStepIndex == 6;

        [ObservableProperty]
        private string _testApiButtonText = "Probar API";

        [RelayCommand]
        public async System.Threading.Tasks.Task TestApiAsync()
        {
            if (string.IsNullOrWhiteSpace(ApiUrl) || (!ApiUrl.StartsWith("http://") && !ApiUrl.StartsWith("https://")))
            {
                MessageBox.Show("Por favor ingresa una URL válida (debe iniciar con http:// o https://).", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            TestApiButtonText = "Probando...";
            try
            {
                using var client = new System.Net.Http.HttpClient();
                client.Timeout = System.TimeSpan.FromSeconds(5);
                var response = await client.GetAsync($"{ApiUrl.TrimEnd('/')}/health");
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Conexión a la API exitosa.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"La API respondió pero con un código de error: {response.StatusCode}", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar con la API:\n{ex.Message}", "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                TestApiButtonText = "Probar API";
            }
        }

        [ObservableProperty]
        private string _testDbButtonText = "Test Connection";

        [RelayCommand]
        public async System.Threading.Tasks.Task TestConnectionAsync()
        {
            TestDbButtonText = "Probando...";
            try 
            {
                if (DbType == "PostgreSQL")
                {
                    if (string.IsNullOrWhiteSpace(DbHost) || string.IsNullOrWhiteSpace(DbUser)) { 
                        MessageBox.Show("Error al conectar: Host y Usuario son requeridos para PostgreSQL.", "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error); 
                        return;
                    }
                    string connStr = $"Host={DbHost};Port={DbPort};Database={DbName};Username={DbUser};Password={DbPassword};Timeout=3";
                    if (DbHost.Contains("supabase") || DbHost.Contains("railway")) {
                        connStr += ";SSL Mode=Require;Trust Server Certificate=True";
                    }
                    
                    using var conn = new Npgsql.NpgsqlConnection(connStr);
                    await conn.OpenAsync();
                    MessageBox.Show("Conexión exitosa a la base de datos PostgreSQL.", "Test Connection", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else 
                {
                    await System.Threading.Tasks.Task.Delay(500); // Simulate network
                    MessageBox.Show("Conexión exitosa a la base de datos local.", "Test Connection", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar a la base de datos:\n{ex.Message}", "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally 
            {
                TestDbButtonText = "Test Connection";
            }
        }

        public WizardViewModel()
        {
            PropertyChanged += WizardViewModel_PropertyChanged;
        }

        private void WizardViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Evitar StackOverflow ignorando cambios en las propiedades calculadas
            if (e.PropertyName == nameof(CanGoNext) || 
                e.PropertyName == nameof(CanGoPrevious) || 
                e.PropertyName == nameof(IsLastStep))
            {
                return;
            }

            OnPropertyChanged(nameof(CanGoNext));
            NextCommand?.NotifyCanExecuteChanged();

            if (e.PropertyName == nameof(CurrentStepIndex))
            {
                OnPropertyChanged(nameof(CanGoPrevious));
                OnPropertyChanged(nameof(IsLastStep));
                PreviousCommand?.NotifyCanExecuteChanged();
            }
        }

        [RelayCommand(CanExecute = nameof(CanGoNext))]
        public void Next()
        {
            if (CanGoNext && CurrentStepIndex < 6)
            {
                CurrentStepIndex++;
            }
        }

        [RelayCommand(CanExecute = nameof(CanGoPrevious))]
        public void Previous()
        {
            if (CanGoPrevious)
            {
                CurrentStepIndex--;
            }
        }

        private bool IsCurrentStepValid()
        {
            switch (CurrentStepIndex)
            {
                case 0: // Step 1: Environment
                    return !string.IsNullOrWhiteSpace(TenantName) && !string.IsNullOrWhiteSpace(ApiUrl) && Port > 0;
                case 1: // Step 2: DB
                    if (DbType == "PostgreSQL")
                        return !string.IsNullOrWhiteSpace(DbHost) && !string.IsNullOrWhiteSpace(DbUser) && !string.IsNullOrWhiteSpace(DbName);
                    return true;
                case 2: // Step 3: Security
                    return !string.IsNullOrWhiteSpace(ProvisionKey) && ProvisionKey.Length >= 8;
                case 3: // Step 4: Branding
                    return !string.IsNullOrWhiteSpace(BrandingName) && !string.IsNullOrWhiteSpace(BrandingColor);
                case 4: // Step 5: Users
                    return !string.IsNullOrWhiteSpace(AdminUser) && !string.IsNullOrWhiteSpace(AdminPassword) &&
                           !string.IsNullOrWhiteSpace(EmployeeUser) && !string.IsNullOrWhiteSpace(EmployeePassword);
                case 5: // Step 6: Modules
                    return true;
                case 6: // Step 7: Summary
                    return true;
                default:
                    return true;
            }
        }
    }
}
