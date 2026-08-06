using PosCore.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PosCore.Services;

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

    Task<LoginResponse?> LoginAsync(string username, string password);
}
