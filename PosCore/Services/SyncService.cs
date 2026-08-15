using PosApplication.Interfaces.Local;
using Serilog;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PosInfrastructure.Data.Local;
using PosDomain.Entities;

namespace PosCore.Services;

public class SyncService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SyncService> _logger;
    
    
     
    
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

    private readonly System.Threading.SemaphoreSlim _syncLock = new System.Threading.SemaphoreSlim(1, 1);
    
    public SyncService(IServiceProvider serviceProvider, ILogger<SyncService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    
    public async Task SyncNowAsync()
    {
        await SyncDataAsync();
    }



    public async Task SyncDataAsync()
    {
        if (!await _syncLock.WaitAsync(0)) return;

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
            // 2. Procesar Outbox local en lotes para pruebas de estrés y tolerancia
            var now = DateTime.UtcNow;
            var pendingMessages = await dbContext.OutboxMessages
                .Where(m => m.ProcessedAt == null &&
                            m.Status != OutboxSyncStateMachine.DeadLetter &&
                            m.NextAttemptAt <= now)
                .OrderBy(m => m.CreatedAt)
                .Take(2000) // Ráfaga máxima de 2000
                .ToListAsync();

            if (pendingMessages.Any())
            {
                _logger.LogInformation($"Iniciando sincronización: {pendingMessages.Count} mensajes pendientes.");
                
                int batchSize = 100; // Batching
                bool networkFailure = false;

                for (int i = 0; i < pendingMessages.Count; i += batchSize)
                {
                    if (networkFailure) break;

                    var batch = pendingMessages.Skip(i).Take(batchSize).ToList();

                    foreach (var message in batch)
                    {
                        try
                        {
                            OutboxSyncStateMachine.MarkProcessing(message);
                            bool success = false;
                            bool invalidEvent = false;

                            if (message.EventType == "OrderCreated" || message.EventType == "OrderReturned")
                            {
                                var order = JsonSerializer.Deserialize<Order>(message.Payload, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                                if (order != null)
                                {
                                    if (string.IsNullOrEmpty(order.TenantId)) order.TenantId = sessionManager.CurrentTenantId;
                                    // Idempotencia: Asegurar un ClientSideId único y determinista basado en el evento local
                                    if (string.IsNullOrEmpty(order.ClientSideId))
                                    {
                                        order.ClientSideId = OutboxSyncStateMachine.BuildDeterministicClientSideId(message);
                                    }
                                    order.CustomerName ??= "";
                                    order.ReturnReason ??= "";
                                    order.AuthorizedBy ??= "";
                                    order.CreatedById ??= "";
                                    order.PaymentDetails ??= "";
                                    order.TenantId ??= "";
                                    order.ClientSideId ??= "";
                                    success = await apiService.SyncOrderAsync(order);
                                }
                            }
                            else if (message.EventType == "ProductUpdated" || message.EventType == "ProductCreated")
                            {
                                var product = JsonSerializer.Deserialize<Product>(message.Payload, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                                if (product != null)
                                {
                                    if (string.IsNullOrEmpty(product.TenantId)) product.TenantId = sessionManager.CurrentTenantId;
                                    product.Name ??= "";
                                    product.Barcode ??= "";
                                    product.Category ??= "General";
                                    product.TenantId ??= "";
                                    success = await apiService.SyncProductAsync(product);
                                }
                            }
                            else if (message.EventType == "ProductDeleted")
                            {
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
                                var user = JsonSerializer.Deserialize<User>(message.Payload, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                                if (user != null)
                                {
                                    if (string.IsNullOrEmpty(user.TenantId)) user.TenantId = sessionManager.CurrentTenantId;
                                    user.Username ??= "";
                                    user.PasswordHash ??= "";
                                    user.Pin = null;
                                    user.TenantId ??= "";
                                    user.Role ??= "Cashier";
                                    success = await apiService.SyncUserAsync(user);
                                }
                            }
                            else if (message.EventType == "UserDeleted")
                            {
                                var user = JsonSerializer.Deserialize<User>(message.Payload, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                                if (user != null && !string.IsNullOrEmpty(user.Username))
                                {
                                    success = await apiService.DeleteUserAsync(user.Username);
                                }
                            }
                                                        else if (message.EventType == "ShiftOpened" || message.EventType == "ShiftClosed")
                            {
                                var shift = JsonSerializer.Deserialize<CashRegisterShift>(message.Payload, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                                if (shift != null)
                                {
                                    if (string.IsNullOrEmpty(shift.TenantId)) shift.TenantId = sessionManager.CurrentTenantId;
                                    if (shift.Movements != null) { foreach(var m in shift.Movements) { if (string.IsNullOrEmpty(m.TenantId)) m.TenantId = shift.TenantId; m.Type ??= ""; m.Reason ??= ""; m.CreatedBy ??= ""; m.TenantId ??= ""; } }
                                    shift.OpenedBy ??= "";
                                    shift.OpenedBy ??= "";
                                    shift.ClosedBy ??= "";
                                    shift.ClosedBy ??= "";
                                    shift.TenantId ??= "";
                                    success = await apiService.SyncShiftAsync(shift);
                                }
                            }
                            
                            else if (message.EventType == "InventoryMovementCreated")
                            {
                                var movement = JsonSerializer.Deserialize<InventoryMovement>(message.Payload, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                                if (movement != null)
                                {
                                    if (string.IsNullOrEmpty(movement.TenantId)) movement.TenantId = sessionManager.CurrentTenantId;
                                    success = await apiService.SyncInventoryMovementAsync(movement);
                                }
                            }
                            else if (message.EventType == "OrderItemCreated" || message.EventType == "CashMovementCreated" || message.EventType == "RecipeItemCreated" || message.EventType == "ProductModifierLinkCreated")

                            {
                                // Ya se procesan a través de su entidad padre (Order / Shift)
                                success = true;
                            }
                            else 
                            {
                                invalidEvent = true;
                            }

                            if (invalidEvent)
                            {
                                OutboxSyncStateMachine.MarkInvalidEvent(message, DateTime.UtcNow);
                                _logger.LogError(
                                    "Mensaje ID {MessageId} movido a DeadLetter por evento no soportado. EventType={EventType}, EventId={EventId}",
                                    message.Id,
                                    message.EventType,
                                    message.EventId);
                            }
                            else if (success)
                            {
                                OutboxSyncStateMachine.MarkProcessed(message, DateTime.UtcNow);
                                _logger.LogInformation(
                                    "Mensaje ID {MessageId} ({EventType}) sincronizado con éxito. EventId={EventId}",
                                    message.Id,
                                    message.EventType,
                                    message.EventId);
                            }
                            else
                            {
                                OutboxSyncStateMachine.MarkRetryableFailure(message, "Remote endpoint rejected sync message.", DateTime.UtcNow);
                                _logger.LogWarning(
                                    "Fallo al sincronizar Mensaje ID {MessageId}. Attempt={AttemptCount}, Status={Status}, EventType={EventType}, EventId={EventId}, NextAttemptAt={NextAttemptAt}",
                                    message.Id,
                                    message.AttemptCount,
                                    message.Status,
                                    message.EventType,
                                    message.EventId,
                                    message.NextAttemptAt);
                                networkFailure = message.Status != OutboxSyncStateMachine.DeadLetter;
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            OutboxSyncStateMachine.MarkRetryableFailure(message, ex.Message, DateTime.UtcNow);
                            _logger.LogWarning(
                                ex,
                                "Excepción al sincronizar Mensaje ID {MessageId}. Attempt={AttemptCount}, Status={Status}, EventType={EventType}, EventId={EventId}, NextAttemptAt={NextAttemptAt}",
                                message.Id,
                                message.AttemptCount,
                                message.Status,
                                message.EventType,
                                message.EventId,
                                message.NextAttemptAt);
                            networkFailure = message.Status != OutboxSyncStateMachine.DeadLetter;
                            break;
                        }
                    }

                    // Guardar progreso del lote actual
                    await dbContext.SaveChangesAsync();

                    if (networkFailure)
                    {
                        var failedMessage = batch.FirstOrDefault(m => m.ProcessedAt == null && m.Status == OutboxSyncStateMachine.Failed);
                        if (failedMessage != null)
                        {
                            _logger.LogWarning(
                                "Fallo de red detectado en el lote. Mensaje ID {MessageId} queda pendiente hasta {NextAttemptAt}.",
                                failedMessage.Id,
                                failedMessage.NextAttemptAt);
                        }
                        break; // Salir de la ráfaga de 2000, el timer retomará después
                    }
                    else
                    {
                        // Pequeña pausa entre lotes exitosos para no saturar CPU/Red (Stress Management)
                        await Task.Delay(100);
                    }
                }
            }

            
            // 3. Limpieza de Outbox (Eliminar procesados de más de 7 días)
            var oldMessages = await dbContext.OutboxMessages
                .Where(m => m.ProcessedAt != null && m.ProcessedAt < DateTime.UtcNow.AddDays(-7))
                .ToListAsync();
            if (oldMessages.Any())
            {
                dbContext.OutboxMessages.RemoveRange(oldMessages);
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
            _syncLock.Release();
        }
    }

    private async Task PullUpdatesFromServerAsync(PosDbContext dbContext, IApiService apiService)
    {
        try
        {
            PosDomain.Entities.SyncPayload? payload = await apiService.GetAllChangesAsync(_lastSyncTime);

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
                        // PHASE 7G SyncService nullability hygiene applied: guard nullable usernames before normalization.
                        var cloudUsername = cloudUser.Username;
                        if (string.IsNullOrWhiteSpace(cloudUsername))
                        {
                            continue;
                        }

                        var normalizedCloudUsername = cloudUsername.ToLowerInvariant();
                        var localUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Username != null && u.Username.ToLower() == normalizedCloudUsername);
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

                if (payload.Supplies != null && payload.Supplies.Any())
                {
                    foreach (var cloudSupply in payload.Supplies)
                    {
                        var localSupply = await dbContext.Supplies.FirstOrDefaultAsync(s => s.Id == cloudSupply.Id);
                        if (localSupply == null)
                        {
                            dbContext.Supplies.Add(cloudSupply);
                        }
                        else
                        {
                            localSupply.Name = cloudSupply.Name;
                            localSupply.UnitOfMeasure = cloudSupply.UnitOfMeasure;
                            localSupply.Cost = cloudSupply.Cost;
                            localSupply.Stock = cloudSupply.Stock;
                            localSupply.MinStockThreshold = cloudSupply.MinStockThreshold;
                            dbContext.Supplies.Update(localSupply);
                        }
                    }
                }
                
                if (payload.ProductModifiers != null && payload.ProductModifiers.Any())
                {
                    foreach (var cloudModifier in payload.ProductModifiers)
                    {
                        var localModifier = await dbContext.ProductModifiers.Include(m => m.Options).FirstOrDefaultAsync(m => m.Id == cloudModifier.Id);
                        if (localModifier == null)
                        {
                            dbContext.ProductModifiers.Add(cloudModifier);
                        }
                        else
                        {
                            localModifier.Name = cloudModifier.Name;
                            
                            localModifier.IsRequired = cloudModifier.IsRequired;
                            localModifier.MinSelections = cloudModifier.MinSelections;
                            localModifier.MaxSelections = cloudModifier.MaxSelections;
                            
                            // Simple replace for options
                            if (cloudModifier.Options != null)
                            {
                                dbContext.ModifierOptions.RemoveRange(localModifier.Options);
                                foreach (var opt in cloudModifier.Options)
                                {
                                    opt.Id = 0;
                                    opt.ProductModifierId = localModifier.Id;
                                    localModifier.Options.Add(opt);
                                }
                            }
                            dbContext.ProductModifiers.Update(localModifier);
                        }
                    }
                }

                _lastSyncTime = DateTime.UtcNow;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener actualizaciones del servidor.");
        }
    }
    private async Task SendPingAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var apiService = scope.ServiceProvider.GetRequiredService<IApiService>();
            var sessionManager = scope.ServiceProvider.GetRequiredService<SessionManager>();
            
            if (!sessionManager.IsAuthenticated) return;
            
            var proc = System.Diagnostics.Process.GetCurrentProcess();
            var payload = new PosDomain.Entities.PingPayload
            {
                AppVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown",
                MemoryUsageMB = Math.Round(proc.WorkingSet64 / 1024.0 / 1024.0, 2),
                PrinterStatus = "Online", // Assuming online for now, could be integrated with TicketPrinterService
                Timestamp = DateTime.UtcNow
            };
            
            await apiService.SendPingAsync(payload);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send heartbeat ping.");
        }
    }
}
