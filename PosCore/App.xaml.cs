using System.IO;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using PosInfrastructure.Data.Local;
using PosDomain.Entities;
using PosApplication.Interfaces.Local;
using PosInfrastructure.Services.Local;
using PosCore.Services;
using PosCore.Extensions;
using PosApplication.UseCases.Orders;
using PosCore.ViewModels;
using PosCore.Views;
using Squirrel;
using System.Threading.Tasks;
using Serilog;
using System;
using System.Linq;

namespace PosCore;

public partial class App : Application
{
    public App()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
    }
    private async Task CheckForUpdatesAsync(string baseApiUrl)
    {
        try
        {
            var updateUrl = Environment.GetEnvironmentVariable("POS_UPDATE_URL") ?? $"{baseApiUrl}/releases";
            using (var mgr = new UpdateManager(updateUrl))
            {
                if (mgr.IsInstalledApp)
                {
                    var updateInfo = await mgr.CheckForUpdate();
                    if (updateInfo != null && updateInfo.ReleasesToApply.Any())
                    {
                        Log.Information($"Actualización encontrada. Descargando versión {updateInfo.FutureReleaseEntry.Version} en segundo plano...");
                        await mgr.DownloadReleases(updateInfo.ReleasesToApply);
                        await mgr.ApplyReleases(updateInfo);
                        Log.Information("Actualización aplicada correctamente. Se instalará en el próximo reinicio.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Error al actualizar la aplicación (Auto-Updater).");
        }
    }
    public static IServiceProvider? ServiceProvider { get; private set; }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Global Exception Handlers
        this.DispatcherUnhandledException += (s, args) =>
        {
            Log.Fatal(args.Exception, "Unhandled UI exception");
            MessageBox.Show($"Ha ocurrido un error inesperado: {args.Exception.Message}\n\nRevisa el archivo de logs para más detalles.", "Error Fatal", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };

        AppDomain.CurrentDomain.UnhandledException += (s, args) =>
        {
            Log.Fatal(args.ExceptionObject as Exception, "Unhandled Domain exception");
            MessageBox.Show($"Ha ocurrido un error fatal: {(args.ExceptionObject as Exception)?.Message}\n\nRevisa el archivo de logs para más detalles.", "Error Fatal", MessageBoxButton.OK, MessageBoxImage.Error);
        };

        // Configuración de Logging (Serilog)
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File("logs/pos-log-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        Log.Information("Iniciando aplicación Super POS Express...");

#if !DEBUG
        // 1. Manejar eventos de Squirrel (accesos directos al instalar/desinstalar)
        try 
        {
            var updateUrl = Environment.GetEnvironmentVariable("POS_UPDATE_URL") ?? "https://example.com/releases";
            using (var mgr = new UpdateManager(updateUrl))
            {
                SquirrelAwareApp.HandleEvents(
                    onInitialInstall: (v, t) => mgr.CreateShortcutForThisExe(),
                    onAppUpdate: (v, t) => mgr.CreateShortcutForThisExe(),
                    onAppUninstall: (v, t) => mgr.RemoveShortcutForThisExe()
                    );
            }
            
        } 
        catch 
        {
            // Ignorar errores si Squirrel falla o no hay conexión
        }
#endif

        string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
        var secureSettings = SecureConfigManager.LoadAndSecureConfig(configPath);
        
        if (secureSettings.Tenant == null || string.IsNullOrEmpty(secureSettings.Tenant.CurrentTenantId))
        {
            MessageBox.Show("Error de configuración: TenantId no configurado. La aplicación se cerrará.", "Error de Configuración", MessageBoxButton.OK, MessageBoxImage.Error);
            Log.Fatal("Application failed to start: TenantId is not configured.");
            Shutdown(-1);
            return;
        }

        if (secureSettings.License == null || string.IsNullOrEmpty(secureSettings.License.LicenseKey))
        {
            MessageBox.Show("Error de configuración: Clave de licencia no configurada. La aplicación se cerrará.", "Error de Configuración", MessageBoxButton.OK, MessageBoxImage.Error);
            Log.Fatal("Application failed to start: LicenseKey is not configured.");
            Shutdown(-1);
            return;
        }
        
        var services = new ServiceCollection();

        // 0. Configuración de Logging
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddSerilog(dispose: true);
        });

        // 0. Configuración (Opciones en memoria a partir de los datos seguros)
        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(secureSettings));

        // 1. Inyección del DbContext (EF Core SQLite)
        services.AddDbContext<PosDbContext>(options =>
            options.UseSqlite(secureSettings.DatabaseSettings.ConnectionString));

        // 2. Inyección de HttpClient, Handler de Auth y Servicios
        services.AddSingleton<SessionManager>();

        services.AddTransient<AuthDelegatingHandler>();
        
        // Repositories
        services.AddScoped(typeof(PosDomain.Interfaces.IRepository<>), typeof(PosInfrastructure.Repositories.Local.Repository<>));
        services.AddScoped<PosDomain.Interfaces.IOrderRepository, PosInfrastructure.Repositories.Local.OrderRepository>();
        services.AddScoped<PosDomain.Interfaces.IProductRepository, PosInfrastructure.Repositories.Local.ProductRepository>();
        
        // Domain Services
        services.AddScoped<PosApplication.Interfaces.IOrderService, PosApplication.UseCases.Orders.OrderService>();

        services.AddHttpClient<LicenseService>();
        services.AddHttpClient<IApiService, ApiService>()
            .AddHttpMessageHandler<AuthDelegatingHandler>()
            .AddPolicyHandler(HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.RequestTimeout || msg.StatusCode == System.Net.HttpStatusCode.BadGateway || msg.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                .WaitAndRetryAsync(new[]
                {
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(4),
                    TimeSpan.FromSeconds(8),
                    TimeSpan.FromSeconds(16),
                    TimeSpan.FromSeconds(30),
                    TimeSpan.FromSeconds(60)
                }));

        // Services
        services.AddInventoryServices();

        // 3. Inyección de ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<InventoryViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<ReportsViewModel>();
        services.AddTransient<ReturnsViewModel>();
        services.AddTransient<ShiftViewModel>();
        services.AddTransient<ShiftWindow>();
        services.AddTransient<UsersViewModel>();
        services.AddTransient<UsersWindow>();
        services.AddTransient<LogViewerViewModel>();
        services.AddTransient<LogViewerWindow>();

        // 4. Inyección del servicio de sincronización (Singleton)
        services.AddSingleton<SyncService>();
        services.AddSingleton<IReceiptPrinter, TicketPrinterService>();

        // 5. Inyección de Views
        services.AddTransient<MainWindow>();
        services.AddTransient<InventoryWindow>();
        services.AddTransient<LoginWindow>();
        services.AddTransient<ReportsWindow>();
        services.AddTransient<ReturnsWindow>();

        ServiceProvider = services.BuildServiceProvider();


        // Aplicar migraciones y Backup
        using (var scope = ServiceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<PosDbContext>();
            
            // Database Migration
            dbContext.Database.Migrate();
            
            dbContext.InitializeDatabaseSettings();
            var connStr = secureSettings.DatabaseSettings.ConnectionString;
            
            DatabaseBackupService.ManageDatabaseBackup(connStr);
            
            try 
            {
                // Seed inicial
                try {

                    if (!Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.IgnoreQueryFilters(dbContext.Products).Any())
                    {
                        var currentTenant = secureSettings.Tenant.CurrentTenantId;
                        dbContext.Products.AddRange(
                            new PosDomain.Entities.Product { Name = "Coca Cola 600ml", Barcode = "7501055300075", Price = 18.00m, StockQuantity = 50, Category = "Bebidas", MinStockThreshold = 10, TenantId = currentTenant, LastUpdated = System.DateTime.Now },
                            new PosDomain.Entities.Product { Name = "Sabritas Sal 40g", Barcode = "7501011111111", Price = 15.00m, StockQuantity = 30, Category = "Botanas", MinStockThreshold = 10, TenantId = currentTenant, LastUpdated = System.DateTime.Now },
                            new PosDomain.Entities.Product { Name = "Agua Ciel 1L", Barcode = "7501022222222", Price = 12.00m, StockQuantity = 40, Category = "Bebidas", MinStockThreshold = 10, TenantId = currentTenant, LastUpdated = System.DateTime.Now }
                        );
                        dbContext.SaveChanges();
                    }
                    else 
                    {
                        var currentTenant = secureSettings.Tenant.CurrentTenantId;
                        var localProducts = dbContext.Products.IgnoreQueryFilters().Where(p => p.TenantId == "LOCAL").ToList();
                        foreach (var p in localProducts)
                        {
                            p.TenantId = currentTenant;
                        }
                        if (localProducts.Any())
                        {
                            dbContext.SaveChanges();
                        }
                    }
                } catch (Exception ex) { Serilog.Log.Error(ex, "Ignored exception"); }

            } 
            catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.SqliteErrorCode == 11 || ex.SqliteErrorCode == 26 || ex.Message.Contains("malformed"))
            {
                // 11 = SQLITE_CORRUPT, 26 = SQLITE_NOTADB
                Log.Error(ex, "Base de datos corrupta detectada.");
                if (DatabaseBackupService.TryRestoreFromBackup(connStr))
                {
                    Application.Current.Shutdown();
                    return;
                }
                else 
                {
                    MessageBox.Show("No se pudo reparar la base de datos. Póngase en contacto con el soporte.", "Error fatal", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.Shutdown();
                    return;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al aplicar migraciones de base de datos.");
            }
        }

        Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

        var sessionManager = ServiceProvider.GetRequiredService<SessionManager>();
        var appSettings = ServiceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<PosApplication.Models.AppSettings>>().Value;
        sessionManager.CurrentTenantId = appSettings.Tenant.CurrentTenantId;
        bool hasSessionData = sessionManager.LoadSession();
        bool isLoggedIn = false; // Force explicit login
        // Fallback to config if session file didn't overwrite it
        if (string.IsNullOrEmpty(sessionManager.CurrentTenantId))
        {
            sessionManager.CurrentTenantId = appSettings.Tenant.CurrentTenantId;
        }

        if (true)
        {
            var loginWindow = ServiceProvider.GetRequiredService<LoginWindow>();
            isLoggedIn = loginWindow.ShowDialog() == true;
        }

        // Check for updates
        _ = Task.Run(async () => await CheckForUpdatesAsync(secureSettings.ApiSettings.BaseUrl.TrimEnd('/')));
        
        if (isLoggedIn)
        {
            var licenseService = ServiceProvider.GetRequiredService<LicenseService>();
            bool isLicenseValid = await licenseService.ValidateLicenseAsync();
            if (!isLicenseValid)
            {
                Application.Current.Shutdown();
                return;
            }

            var syncWorker = new SyncServiceWorker(ServiceProvider, ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<SyncServiceWorker>>());
            _ = syncWorker.StartAsync(System.Threading.CancellationToken.None);
            
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            Application.Current.MainWindow = mainWindow;
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            mainWindow.Show();
        }
        else
        {
            Application.Current.Shutdown();
        }
    }
}
