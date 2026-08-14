using PosCore.Services;
using System.Collections.ObjectModel;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using PosDomain.Entities;
using PosApplication.Models;
using PosApplication.Interfaces.Local;
using PosApplication.DTOs.Local;

namespace PosCore.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isAdmin;
    [ObservableProperty]
    private bool _isOffline;

    [ObservableProperty]
    private string _syncStatusMessage = "Sincronizando...";

    [ObservableProperty]
    private SolidColorBrush _syncStatusColor = Brushes.Gray;

    [ObservableProperty]
    private bool _isHardwareError = false;

    [ObservableProperty]
    private string _hardwareErrorMessage = string.Empty;

    public static ObservableCollection<ObservableCollection<OrderItem>> SuspendedOrders { get; set; } = new();

    [ObservableProperty]
    private decimal _discountAmount = 0m;

    [ObservableProperty]
    private bool _isDiscountApplied = false;
    private readonly IInventoryAppService _inventoryAppService;
    private readonly ILocalOrderService _localOrderService;

    // Propiedades Observables
    [ObservableProperty]
    private ObservableCollection<OrderItem> _cart = new();

    [ObservableProperty]
    private ObservableCollection<ShortcutConfig> _shortcuts = new();

    private ShortcutManager _shortcutManager;

    [ObservableProperty]
    private ObservableCollection<Product> _products = new();

    [ObservableProperty]
    private ObservableCollection<Product> _filteredProducts = new();


    [ObservableProperty]
    private ObservableCollection<string> _categories = new();

    [ObservableProperty]
    private string _selectedCategory = "Todas";

    [RelayCommand]
    private void CancelSale()
    {
        if (Cart.Count > 0)
        {
            var result = System.Windows.MessageBox.Show("¿Está seguro de cancelar la venta actual?", "Cancelar Venta", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
            if (result == System.Windows.MessageBoxResult.Yes)
            {
                Cart.Clear();
                CustomerName = string.Empty;
                IsDiscountApplied = false;
                UpdateTotal();
            }
        }
    }

    [RelayCommand]
    private void FilterByCategory(string category)
    {
        SelectedCategory = category;
        if (LoadProductsCommand.CanExecute(null))
            LoadProductsCommand.Execute(null);
    }

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private string _notificationMessage = string.Empty;

    [ObservableProperty]
    private bool _isNotificationVisible = false;

    [ObservableProperty]
    private SolidColorBrush _notificationColor = Brushes.Green;

    private int _notificationId = 0;
    private async Task ShowNotification(string message, bool isError = false)
    {
        var currentId = System.Threading.Interlocked.Increment(ref _notificationId);
        await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
        {
            NotificationMessage = message;
            NotificationColor = isError ? Brushes.Red : Brushes.Green;
            IsNotificationVisible = true;
        });

        await Task.Delay(3000);

        if (_notificationId == currentId)
        {
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                IsNotificationVisible = false;
            });
        }
    }

    partial void OnSearchQueryChanged(string value)
    {
        ApplySearchFilter();
    }

    public void ProcessBarcode()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery)) return;
        
        var exactBarcodeMatch = Products.FirstOrDefault(p => p.Barcode == SearchQuery);
        if (exactBarcodeMatch != null)
        {
            AddToCart(exactBarcodeMatch);
            System.Windows.Application.Current.Dispatcher.InvokeAsync(() => 
            {
                SearchQuery = string.Empty;
            });
        }
    }


    private void ApplySearchFilter()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            FilteredProducts = new ObservableCollection<Product>(Products);
            return;
        }

        var query = SearchQuery.ToLower();


        var matches = Products.Where(p => 
            p.Name.ToLower().Contains(query) || 
            p.Barcode.Contains(query)
        ).ToList();

        FilteredProducts = new ObservableCollection<Product>(matches);
    }

    [ObservableProperty]
    private decimal _total;

    [ObservableProperty]
    private decimal _subTotal;

    [ObservableProperty]
    private decimal _taxes;

    [ObservableProperty]
    private string _customerName = string.Empty;

    
    [RelayCommand]
    private void ConfigurePrinter()
    {
        var configWindow = new PosCore.Views.PrinterConfigWindow();
        if (configWindow.ShowDialog() == true)
        {
            Settings.Printer.PortName = configWindow.SelectedPrinter;
            Settings.Printer.PrintLogo = configWindow.PrintLogo;
            // Opcionalmente guardar localmente en appsettings.json, aquí solo se actualiza en memoria para la sesión.
            _ = ShowNotification("Configuración de impresora guardada.", false);
        }
    }


    [ObservableProperty]
    private AppSettings _settings;

    [ObservableProperty]
    private SolidColorBrush _primaryColorBrush = Brushes.Blue;

    private readonly SyncService _syncService;
    private readonly IReceiptPrinter _ticketPrinterService;
    private readonly PosCore.Services.SessionManager _sessionManager;

    public event Action? OnFocusSearchRequested;
    [RelayCommand]
    private void FocusSearch() { OnFocusSearchRequested?.Invoke(); }

    public MainViewModel(IOptions<AppSettings> settings, SyncService syncService, IReceiptPrinter ticketPrinterService, PosCore.Services.SessionManager sessionManager, IInventoryAppService inventoryAppService, ILocalOrderService localOrderService)
    {
        _shortcutManager = new ShortcutManager();
        Shortcuts = new ObservableCollection<ShortcutConfig>(_shortcutManager.CurrentShortcuts);

        _inventoryAppService = inventoryAppService;
        _localOrderService = localOrderService;
        _settings = settings.Value;
        _syncService = syncService;
        _ticketPrinterService = ticketPrinterService;
        _ticketPrinterService.OnPrinterNotification += (msg) => 
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() => ShowNotification(msg, true));
        };
        _sessionManager = sessionManager;
        IsAdmin = _sessionManager.Role != null && _sessionManager.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase);

        _syncService.OnSyncCompleted += () => 
        {
            if (LoadProductsCommand.CanExecute(null))
            {
                LoadProductsCommand.Execute(null);
            }
        };
        _syncService.OnNetworkStatusChanged += (isOffline) =>
        {
            IsOffline = isOffline;
            SyncStatusMessage = isOffline ? "Modo Offline (Reintentando...)" : "Sincronizado";
            SyncStatusColor = isOffline ? Brushes.Orange : Brushes.Green;
        };
        IsOffline = _syncService.IsOffline;
        SyncStatusMessage = IsOffline ? "Modo Offline (Reintentando...)" : "Sincronizado";
        SyncStatusColor = IsOffline ? Brushes.Orange : Brushes.Green;
        
        try {
            var color = (Color)ColorConverter.ConvertFromString(Settings.WhiteLabel.PrimaryColor);
            PrimaryColorBrush = new SolidColorBrush(color);
        } catch (Exception ex) { Serilog.Log.Error(ex, "Handled exception");
            // fallback if color is invalid
        }
        
        // Cargar productos iniciales
        LoadProductsCommand.Execute(null);
    }


    
    [RelayCommand]
    private async Task LoadProductsAsync()
    {
        var allProducts = (await _inventoryAppService.GetAllProductsAsync()).ToList();
        var query = allProducts.AsEnumerable();
        
        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            var lowerQuery = SearchQuery.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(lowerQuery) ||
                (!string.IsNullOrWhiteSpace(p.Barcode) && p.Barcode.ToLower().Contains(lowerQuery)));
        }
        
        var cats = allProducts
            .Select(p => p.Category)
            .Where(c => !string.IsNullOrEmpty(c))
            .Distinct()
            .ToList();
        if (!cats.Contains("General")) cats.Add("General");
        
        if (!Categories.Contains("Todas")) Categories.Add("Todas");
        foreach (var c in cats)
        {
            if (!Categories.Contains(c)) Categories.Add(c);
        }
        
        if (!string.IsNullOrEmpty(SelectedCategory) && SelectedCategory != "Todas")
        {
            query = query.Where(p => p.Category == SelectedCategory);
        }

        var products = query.OrderBy(p => p.Name).Take(50).ToList();
        Products.Clear();
        foreach (var p in products)
        {
            Products.Add(p);
        }
        ApplySearchFilter();
    }

    [RelayCommand]
    private void AddToCart(Product product)
    {
        var existingItem = Cart.FirstOrDefault(i => i.ProductId == product.Id);
        
        int currentQuantity = existingItem?.Quantity ?? 0;
        if (product.StockQuantity <= currentQuantity)
        {
            _ = ShowNotification($"Stock insuficiente. Solo hay {product.StockQuantity} disponibles.", true);
            return;
        }

        if (existingItem != null)
        {
            existingItem.Quantity++;
            var index = Cart.IndexOf(existingItem);
            if (index >= 0) {
                Cart.RemoveAt(index);
                Cart.Insert(index, existingItem);
            }
        }
        else
        {
            Cart.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductBarcode = product.Barcode,
                Product = product,
                Quantity = 1,
                UnitPrice = product.Price
            });
        }
        
        UpdateTotal();
    }

    [RelayCommand]
    private void RemoveFromCart(OrderItem item)
    {
        if (item != null)
        {
            Cart.Remove(item);
            UpdateTotal();
        }
    }

    [RelayCommand]
    private void IncreaseQuantity(OrderItem item)
    {
        if (item != null)
        {
            if (item.Product != null && item.Quantity >= item.Product.StockQuantity)
            {
                _ = ShowNotification($"Stock insuficiente. Solo hay {item.Product.StockQuantity} disponibles.", true);
                return;
            }
            item.Quantity++;
            var index = Cart.IndexOf(item);
            if (index >= 0) {
                Cart.RemoveAt(index);
                Cart.Insert(index, item);
            }
            UpdateTotal();
        }
    }

    [RelayCommand]
    private void ModifyItem(OrderItem item)
    {
        if (item != null)
        {
            var modifierWindow = new PosCore.Views.ItemModifierWindow(item);
            if (modifierWindow.ShowDialog() == true)
            {
                // Force UI update for the cart by replacing the item to trigger property changed
                var index = Cart.IndexOf(item);
                if (index >= 0) {
                    Cart.RemoveAt(index);
                    Cart.Insert(index, item);
                }
                UpdateTotal();
            }
        }
    }

    [RelayCommand]
    private void DecreaseQuantity(OrderItem item)
    {
        if (item != null)
        {
            if (item.Quantity > 1)
            {
                item.Quantity--;
            }
            else
            {
                Cart.Remove(item);
            }
        }
        
        UpdateTotal();
    }

    [RelayCommand]
    private void OpenShift()
    {
        var shiftWindow = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<PosCore.Views.ShiftWindow>(App.ServiceProvider!);
        shiftWindow.ShowDialog();
    }

    [RelayCommand]
    private void OpenReports()
    {
        var reportsWindow = App.ServiceProvider?.GetService(typeof(Views.ReportsWindow)) as System.Windows.Window;
        reportsWindow?.ShowDialog();
    }

    [RelayCommand]
    private void OpenReturns()
    {
        var returnsWindow = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<PosCore.Views.ReturnsWindow>(App.ServiceProvider!);
        returnsWindow.ShowDialog();
        
        LoadProductsCommand.Execute(null);
    }

    [RelayCommand]
    private void OpenUsers()
    {
        var usersWindow = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<PosCore.Views.UsersWindow>(App.ServiceProvider!);
        usersWindow.ShowDialog();
    }

    [RelayCommand]
    private void LockScreen()
    {
        _sessionManager.CurrentUserId = string.Empty;
        var loginWindow = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<PosCore.Views.LoginWindow>(App.ServiceProvider!);
        if (loginWindow.ShowDialog() != true)
        {
            // If they cancel or close, shutdown
            System.Windows.Application.Current.Shutdown();
        }
    }

    [RelayCommand]
    private void Logout()
    {
        var result = System.Windows.MessageBox.Show("¿Está seguro de que desea cerrar sesión?", "Cerrar sesión", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
        if (result == System.Windows.MessageBoxResult.Yes)
        {
            _sessionManager.ClearSession();
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var currentApp = System.Windows.Application.Current;
                var processPath = System.Environment.ProcessPath;
                if (!string.IsNullOrEmpty(processPath))
                {
                    System.Diagnostics.Process.Start(processPath);
                }
                currentApp.Shutdown();
            });
        }
    }

    [RelayCommand]
    private void OpenInventory()
    {
        var inventoryWindow = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<PosCore.Views.InventoryWindow>(App.ServiceProvider!);
        inventoryWindow.ShowDialog();
        
        LoadProductsCommand.Execute(null);
    }

    private CheckoutRequest BuildCheckoutRequest(IEnumerable<PosCore.Views.PaymentEntry> payments, string idempotencyKey, decimal taxRate)
    {
        var totalTendered = payments.Sum(p => p.Amount);
        var changeDue = totalTendered > Total ? totalTendered - Total : 0m;
        var remainingChange = changeDue;
        var paymentRequests = new System.Collections.Generic.List<CheckoutPaymentRequest>();

        foreach (var payment in payments)
        {
            var appliedAmount = payment.Amount;
            var changeApplied = 0m;
            if (remainingChange > 0 && payment.Method == "Efectivo")
            {
                changeApplied = System.Math.Min(remainingChange, payment.Amount);
                appliedAmount -= changeApplied;
                remainingChange -= changeApplied;
            }

            paymentRequests.Add(new CheckoutPaymentRequest
            {
                Method = payment.Method,
                TenderedAmount = payment.Amount,
                AppliedAmount = appliedAmount,
                ChangeApplied = changeApplied
            });
        }

        var subtotal = Total / (1 + taxRate);
        var taxes = Total - subtotal;

        return new CheckoutRequest
        {
            CustomerName = CustomerName,
            TenantId = _sessionManager.CurrentTenantId,
            CreatedById = _sessionManager.CurrentUserId,
            IdempotencyKey = idempotencyKey,
            OrderDate = DateTime.Now,
            SubTotal = subtotal,
            TaxAmount = taxes,
            TotalAmount = Total,
            DiscountAmount = DiscountAmount,
            TaxRate = taxRate,
            TotalTendered = totalTendered,
            ChangeDue = changeDue,
            Lines = Cart.Select(item => new CheckoutLineRequest
            {
                ProductId = item.ProductId,
                ProductBarcode = item.ProductBarcode,
                ProductName = item.Product?.Name ?? string.Empty,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Discount = item.Discount,
                Notes = item.Notes,
                SubTotal = item.SubTotal
            }).ToList(),
            Payments = paymentRequests
        };
    }

    [RelayCommand]
    private async Task CheckoutAsync()
    {
        if (!Cart.Any()) return;

        if (!await _localOrderService.HasActiveShiftAsync(_sessionManager.CurrentTenantId))
        {
            _ = ShowNotification("No hay un turno abierto. Por favor, abra un turno.", true);
            return;
        }

        var paymentWindow = new PosCore.Views.PaymentWindow(Total);
        if (paymentWindow.ShowDialog() != true)
        {
            return;
        }

        try
        {
            if (System.Windows.Application.Current.MainWindow is PosCore.Views.MainWindow mainWindow)
            {
                mainWindow.ShowLoading("Procesando pago y sincronizando con el servidor...");
            }

            decimal taxRate = Settings.Tax?.DefaultTaxRate ?? 0.16m;
            var idempotencyKey = Guid.NewGuid().ToString();
            var checkoutRequest = BuildCheckoutRequest(paymentWindow.Payments, idempotencyKey, taxRate);

            var checkoutResult = await _localOrderService.ProcessCheckoutAsync(checkoutRequest);
            if (!checkoutResult.IsSuccess)
            {
                if (System.Windows.Application.Current.MainWindow is PosCore.Views.MainWindow failedWindow)
                {
                    failedWindow.HideLoading();
                }
                _ = ShowNotification(checkoutResult.Message, true);
                return;
            }

            if (checkoutResult.OrderId.HasValue)
            {
                try
                {
                    var order = await _localOrderService.GetOrderByIdAsync(checkoutResult.OrderId.Value);
                    _ticketPrinterService.PrintTicket(order);
                }
                catch (System.Exception ex)
                {
                    _ = ShowNotification($"Venta guardada, pero falló la impresión: {ex.Message}", true);
                }
            }

            CustomerName = string.Empty;
            Cart.Clear();
            IsDiscountApplied = false;
            UpdateTotal();

            if (System.Windows.Application.Current.MainWindow is PosCore.Views.MainWindow mainWin)
            {
                mainWin.HideLoading();
            }
            _ = ShowNotification("Venta completada exitosamente.", false);

            LoadProductsCommand.Execute(null);
        }
        catch (System.Exception ex)
        {
            if (System.Windows.Application.Current.MainWindow is PosCore.Views.MainWindow mainWindowError)
            {
                mainWindowError.HideLoading();
            }
            _ = ShowNotification($"Error: {ex.Message}", true);
        }
    }

    [RelayCommand]
    private void SuspendOrder()
    {
        if (!Cart.Any()) return;
        
        // Add current cart to suspended
        var suspendedCart = new ObservableCollection<OrderItem>(Cart);
        SuspendedOrders.Add(suspendedCart);
        
        CustomerName = string.Empty;
        Cart.Clear();
            IsDiscountApplied = false;
            UpdateTotal();
        _ = ShowNotification("Orden suspendida exitosamente.", false);
    }

    [RelayCommand]
    private void ResumeOrder()
    {
        if (Cart.Any())
        {
            _ = ShowNotification("Hay una orden en curso. Ciérrela o suspéndala antes de retomar otra.", true);
            return;
        }

        var resumeWindow = new PosCore.Views.SuspendedOrdersWindow(SuspendedOrders);
        if (resumeWindow.ShowDialog() == true && resumeWindow.SelectedOrder != null)
        {
            foreach (var item in resumeWindow.SelectedOrder)
            {
                Cart.Add(item);
            }
            SuspendedOrders.Remove(resumeWindow.SelectedOrder);
            UpdateTotal();
            _ = ShowNotification("Orden retomada.", false);
        }
    }

    [RelayCommand]
    private void OpenLogs()
    {
        var logsWindow = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<PosCore.Views.LogViewerWindow>(App.ServiceProvider!);
        logsWindow.ShowDialog();
    }

    [RelayCommand]
    private void TestPrinter()
    {
        try
        {
            bool success = _ticketPrinterService.TestPrinter();
            if (!success)
            {
                IsHardwareError = true;
                HardwareErrorMessage = "Error de impresora detectado durante prueba.";
                _ = ShowNotification("Fallo al imprimir", true);
            }
            else
            {
                IsHardwareError = false;
                _ = ShowNotification("Prueba de impresión enviada.", false);
            }
        }
        catch (System.Exception ex)
        {
            _ = ShowNotification($"Error de impresión: {ex.Message}", true);
        }
    }

    public void UpdateTotal()
    {
        SubTotal = Cart.Sum(i => i.SubTotal);
        if (IsDiscountApplied)
        {
            DiscountAmount = SubTotal * 0.10m;
        }
        else
        {
            DiscountAmount = 0;
        }
        Total = SubTotal - DiscountAmount;
    }

    [RelayCommand]
    public void ExecuteShortcut(string actionName)
    {
        if (string.IsNullOrEmpty(actionName)) return;
        
        switch (actionName)
        {
            case "OpenShift": OpenShiftCommand.Execute(null); break;
            case "OpenReturns": OpenReturnsCommand.Execute(null); break;
            case "OpenReports": if (IsAdmin) OpenReportsCommand.Execute(null); break;
            case "OpenUsers": if (IsAdmin) OpenUsersCommand.Execute(null); break;
            case "OpenInventory": if (IsAdmin) OpenInventoryCommand.Execute(null); break;
            case "SuspendOrder": SuspendOrderCommand.Execute(null); break;
            case "ResumeOrder": ResumeOrderCommand.Execute(null); break;
            case "TechSupport": OpenLogsCommand.Execute(null); break;
            case "OpenSettings": OpenSettingsCommand.Execute(null); break;
            case "OpenDiscount": ApplyDiscount(); break;
            default:
                System.Windows.MessageBox.Show($"Acción '{actionName}' no implementada aún.", "Info", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                break;
        }
    }

    [RelayCommand]
    public void ApplyDiscount()
    {
        if (!IsDiscountApplied)
        {
            DiscountAmount = SubTotal * 0.10m;
            IsDiscountApplied = true;
        }
        else
        {
            DiscountAmount = 0;
            IsDiscountApplied = false;
        }
        UpdateTotal();
    }
    [RelayCommand]
    public void OpenSettings()
    {
        var settingsWindow = new PosCore.Views.SettingsWindow(_shortcutManager);
        if (settingsWindow.ShowDialog() == true)
        {
            Shortcuts.Clear();
            foreach(var s in _shortcutManager.CurrentShortcuts)
            {
                Shortcuts.Add(s);
            }
        }
    }

}