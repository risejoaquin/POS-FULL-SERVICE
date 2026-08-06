using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PosCore.Data;
using PosCore.Models;

namespace PosCore.Services;

public class SyncService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SyncService> _logger;
    private readonly System.Timers.Timer _timer;
    private bool _isSyncing = false;
    
    private DateTime _lastSyncTime = DateTime.MinValue;
    
    public event Action? OnSyncCompleted;
    public event Action<bool>? OnNetworkStatusChanged;
    private bool _isOffline = false;
    public bool IsOffline
    {
        get => _isOffline;
        private set
        {
            if (_isOffline != value)
            {
                _isOffline = value;
                System.Windows.Application.Current.Dispatcher.Invoke(() => OnNetworkStatusChanged?.Invoke(_isOffline));
            }
        }
    }

    public SyncService(IServiceProvider serviceProvider, ILogger<SyncService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        
        // Configurar timer para ejecutar cada 10 segundos
        _timer = new System.Timers.Timer(10000);
        _timer.Elapsed += async (sender, e) => { try { await SyncDataAsync(); } catch (Exception ex) { _logger.LogError(ex, "Sync failed"); } };
    }

    public void Start()
    {
        _timer.Start();
        Task.Run(async () => await SyncDataAsync());
    }

    public void Stop()
    {
        _timer.Stop();
    }

    public async Task SyncDataAsync()
    {
        if (_isSyncing) return;
        _isSyncing = true;

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PosDbContext>();
            var apiService = scope.ServiceProvider.GetRequiredService<IApiService>();
            var sessionManager = scope.ServiceProvider.GetRequiredService<SessionManager>();

            // Solo sincronizamos si hay un usuario autenticado
            if (!sessionManager.IsAuthenticated) return;

            // 1. Sincronización Inversa (Descargar cambios de BD Central)
            await PullUpdatesFromServerAsync(dbContext, apiService);

            // 2. Procesar Outbox local para enviar al servidor
            var pendingMessages = await dbContext.OutboxMessages
                .Where(m => m.ProcessedAt == null)
                .OrderBy(m => m.CreatedAt)
                .Take(500)
                .ToListAsync();

            if (pendingMessages.Any())
            {
                _logger.LogInformation($"Iniciando sincronización: {pendingMessages.Count} mensajes pendientes.");

                foreach (var message in pendingMessages)
                {
                    try
                    {
                        bool success = false;

                    if (message.EventType == "OrderCreated")
                    {
                        var order = JsonSerializer.Deserialize<Order>(message.Payload);
                        if (order != null)
                        {
                            success = await apiService.SyncOrderAsync(order);
                        }
                    }
                    else if (message.EventType == "ProductUpdated" || message.EventType == "ProductCreated")
                    {
                        var product = JsonSerializer.Deserialize<Product>(message.Payload);
                        if (product != null)
                        {
                            success = await apiService.SyncProductAsync(product);
                        }
                    }
                    else if (message.EventType == "ProductDeleted")
                    {
                        // Payload: { Id = ..., Barcode = ... }
                        using var doc = System.Text.Json.JsonDocument.Parse(message.Payload);
                        if (doc.RootElement.TryGetProperty("Barcode", out var barcodeElement))
                        {
                            var barcode = barcodeElement.GetString();
                            if (!string.IsNullOrEmpty(barcode))
                            {
                                success = await apiService.DeleteProductAsync(barcode);
                            }
                        }
                    }
                    else if (message.EventType == "UserCreated" || message.EventType == "UserUpdated")
                    {
                        var user = JsonSerializer.Deserialize<User>(message.Payload);
                        if (user != null)
                        {
                            success = await apiService.SyncUserAsync(user);
                        }
                    }
                    else if (message.EventType == "UserDeleted")
                    {
                        var user = JsonSerializer.Deserialize<User>(message.Payload);
                        if (user != null && !string.IsNullOrEmpty(user.Username))
                        {
                            success = await apiService.DeleteUserAsync(user.Username);
                        }
                    }
                    else if (message.EventType == "OrderReturned")
                    {
                        // Same endpoint for orders
                        var order = JsonSerializer.Deserialize<Order>(message.Payload);
                        if (order != null)
                        {
                            success = await apiService.SyncOrderAsync(order);
                        }
                    }
                    else if (message.EventType == "ShiftOpened" || message.EventType == "ShiftClosed" || message.EventType == "CashMovementCreated")
                    {
                        var shift = JsonSerializer.Deserialize<CashRegisterShift>(message.Payload);
                        if (shift != null)
                        {
                            // A veces el payload de cashmovement es diferente, 
                            // asumiendo que PosCore enviará el objeto CashRegisterShift completo con sus movements.
                            success = await apiService.SyncShiftAsync(shift);
                        }
                    }

                    if (success)
                    {
                        message.ProcessedAt = DateTime.UtcNow;
                        _logger.LogInformation($"Mensaje ID {message.Id} ({message.EventType}) sincronizado con éxito.");
                    }
                    else
                    {
                        message.RetryCount++;
                        if (message.RetryCount >= 5)
                        {
                            _logger.LogError($"Mensaje ID {message.Id} superó el límite máximo de reintentos. Marcando como procesado con error.");
                            message.ProcessedAt = DateTime.UtcNow; // O IsProcessed = true
                        }
                        else
                        {
                            _logger.LogWarning($"Fallo al sincronizar Mensaje ID {message.Id}. Intento {message.RetryCount}. Aplicando Backoff indefinido.");
                            await Task.Delay((int)Math.Pow(2, message.RetryCount) * 1000);
                            break;
                        }
                    }
                    }
                    catch (Exception ex)
                    {
                        message.RetryCount++;
                        if (message.RetryCount >= 5)
                        {
                            _logger.LogError($"Mensaje ID {message.Id} superó el límite máximo de reintentos con excepción: {ex.Message}. Marcando como procesado con error.");
                            message.ProcessedAt = DateTime.UtcNow;
                        }
                        else
                        {
                            _logger.LogWarning($"Excepcion al sincronizar Mensaje ID {message.Id}: {ex.Message}");
                            await Task.Delay((int)Math.Pow(2, message.RetryCount) * 1000);
                            break;
                        }
                    }
                }

                await dbContext.SaveChangesAsync();
            }
            
            IsOffline = false;
            // Notificar a la UI si hubo cambios o simplemente al terminar un ciclo de sync exitoso
            // Para evitar re-renders excesivos, idealmente solo lo llamamos si hubo pendingMessages o cloudProducts, pero por simplicidad lo llamamos siempre que termine sin error
            System.Windows.Application.Current.Dispatcher.Invoke(() => 
            {
                OnSyncCompleted?.Invoke();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error crítico durante el proceso de sincronización.");
            IsOffline = true;
        }
        finally
        {
            _isSyncing = false;
        }
    }

    private async Task PullUpdatesFromServerAsync(PosDbContext dbContext, IApiService apiService)
    {
        try
        {
            PosCore.Models.SyncPayload? payload = await apiService.GetAllChangesAsync(_lastSyncTime);

            if (payload != null)
            {
                if (payload.Products != null && payload.Products.Any())
                {
                    foreach (var cloudProduct in payload.Products)
                    {
                        var localProduct = await dbContext.Products.FirstOrDefaultAsync(p => p.Barcode == cloudProduct.Barcode);
                        if (localProduct == null)
                        {
                            cloudProduct.Id = 0; 
                            dbContext.Products.Add(cloudProduct);
                        }
                        else
                        {
                            if (cloudProduct.LastUpdated > localProduct.LastUpdated)
                            {
                                localProduct.Name = cloudProduct.Name;
                                localProduct.Price = cloudProduct.Price;
                                localProduct.Category = cloudProduct.Category;
                                localProduct.MinStockThreshold = cloudProduct.MinStockThreshold;
                                localProduct.IsActive = cloudProduct.IsActive;
                                localProduct.StockQuantity = Math.Min(localProduct.StockQuantity, cloudProduct.StockQuantity);
                                localProduct.LastUpdated = cloudProduct.LastUpdated;
                                dbContext.Products.Update(localProduct);
                            }
                        }
                    }
                }

                if (payload.Users != null && payload.Users.Any())
                {
                    foreach (var cloudUser in payload.Users)
                    {
                        var localUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == cloudUser.Username.ToLower());
                        if (localUser == null)
                        {
                            cloudUser.Id = 0;
                            dbContext.Users.Add(cloudUser);
                        }
                        else
                        {
                            if (cloudUser.LastUpdated > localUser.LastUpdated)
                            {
                                localUser.Role = cloudUser.Role;
                                localUser.IsActive = cloudUser.IsActive;
                                localUser.LastUpdated = cloudUser.LastUpdated;
                                dbContext.Users.Update(localUser);
                            }
                        }
                    }
                }
                
                if (payload.Shifts != null && payload.Shifts.Any())
                {
                    foreach (var cloudShift in payload.Shifts)
                    {
                        var localShift = await dbContext.CashRegisterShifts
                            .Include(s => s.Movements)
                            .FirstOrDefaultAsync(s => s.OpenedAt == cloudShift.OpenedAt && s.OpenedBy == cloudShift.OpenedBy);
                        if (localShift == null)
                        {
                            cloudShift.Id = 0;
                            if (cloudShift.Movements != null) {
                                foreach (var mov in cloudShift.Movements) { mov.Id = 0; mov.ShiftId = 0; }
                            }
                            dbContext.CashRegisterShifts.Add(cloudShift);
                        }
                        else
                        {
                            if (cloudShift.LastUpdated > localShift.LastUpdated)
                            {
                                localShift.ClosedAt = cloudShift.ClosedAt;
                                localShift.ActualEndingCash = cloudShift.ActualEndingCash;
                                localShift.ExpectedEndingCash = cloudShift.ExpectedEndingCash;
                                localShift.Difference = cloudShift.Difference;
                                localShift.ClosedBy = cloudShift.ClosedBy;
                                localShift.IsClosed = cloudShift.IsClosed;
                                localShift.LastUpdated = cloudShift.LastUpdated;
                                dbContext.CashRegisterShifts.Update(localShift);
                            }
                        }
                    }
                }

                if (payload.Orders != null && payload.Orders.Any())
                {
                    foreach (var cloudOrder in payload.Orders)
                    {
                        var localOrder = await dbContext.Orders
                            .Include(o => o.Items)
                            .FirstOrDefaultAsync(o => o.ClientSideId == cloudOrder.ClientSideId);
                        if (localOrder == null && !string.IsNullOrEmpty(cloudOrder.ClientSideId))
                        {
                            cloudOrder.Id = 0;
                            if (cloudOrder.Items != null) {
                                foreach (var item in cloudOrder.Items) { item.Id = 0; item.OrderId = 0; }
                            }
                            dbContext.Orders.Add(cloudOrder);
                        }
                        else if (localOrder != null)
                        {
                            if (cloudOrder.LastUpdated > localOrder.LastUpdated)
                            {
                                localOrder.IsReturned = cloudOrder.IsReturned;
                                localOrder.ReturnReason = cloudOrder.ReturnReason;
                                localOrder.AuthorizedBy = cloudOrder.AuthorizedBy;
                                localOrder.TotalAmount = cloudOrder.TotalAmount;
                                localOrder.LastUpdated = cloudOrder.LastUpdated;
                                dbContext.Orders.Update(localOrder);
                            }
                        }
                    }
                }

                await dbContext.SaveChangesAsync();
                _lastSyncTime = DateTime.UtcNow;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener actualizaciones del servidor.");
        }
    }
}
