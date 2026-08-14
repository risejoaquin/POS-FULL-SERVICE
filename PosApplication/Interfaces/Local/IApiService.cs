using PosDomain.Entities;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PosApplication.Interfaces.Local;

public interface IApiService
{
    Task<List<Product>> GetProductsAsync();
    Task<List<Product>> GetChangesAsync(DateTime since);
    Task<SyncPayload?> GetAllChangesAsync(DateTime since);
    
    Task<bool> DeleteProductAsync(string barcode);

    Task<bool> SyncProductAsync(Product product);
    Task<bool> SyncOrderAsync(Order order);
    
    Task<bool> SyncUserAsync(User user);
    Task<bool> DeleteUserAsync(string username);

    
    Task<bool> SyncShiftAsync(CashRegisterShift shift);

    Task<PosApplication.Interfaces.Local.LoginResponse?> LoginAsync(string username, string password);
    Task<bool> SendPingAsync(PosDomain.Entities.PingPayload payload);
    Task<bool> SyncInventoryMovementAsync(PosDomain.Entities.InventoryMovement movement);
}
