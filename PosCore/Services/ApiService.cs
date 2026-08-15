using Serilog;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using PosDomain.Entities;
using PosApplication.Interfaces.Local;
using PosApplication.Models;


namespace PosCore.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
    };

    public ApiService(HttpClient httpClient, IOptions<AppSettings> settings)
    {
        _httpClient = httpClient;
        var baseUrl = settings.Value.ApiSettings?.BaseUrl;
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            baseUrl = "https://pos-full-service-production.up.railway.app/";
        }
        else if (!baseUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            baseUrl = "https://" + baseUrl.TrimStart('/');
        }
        
        try 
        {
            _httpClient.BaseAddress = new Uri(baseUrl);
        }
        catch (Exception ex) 
        {
            Serilog.Log.Error(ex, $"Error creating Uri for BaseUrl: '{baseUrl}'");
            _httpClient.BaseAddress = new Uri("https://pos-full-service-production.up.railway.app/");
        }
    }

    public async Task<List<Product>> GetProductsAsync()
    {
        try
        {
            var products = await _httpClient.GetFromJsonAsync<List<Product>>("api/v1/products", _jsonOptions);
            return products ?? new List<Product>();
        }
        catch (Exception ex) { Serilog.Log.Error(ex, "Exception in GetProductsAsync"); return new List<Product>(); }
    }

    public async Task<PosDomain.Entities.SyncPayload?> GetAllChangesAsync(DateTime since)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<PosDomain.Entities.SyncPayload>($"api/v1/sync/changes?since={Uri.EscapeDataString(since.ToString("O"))}", _jsonOptions);
        }
        catch (Exception ex) { Serilog.Log.Error(ex, "Exception in GetAllChangesAsync"); return null; }
    }

    public async Task<List<Product>> GetChangesAsync(DateTime since)
    {
        try
        {
            var products = await _httpClient.GetFromJsonAsync<List<Product>>($"api/v1/products/changes?since={Uri.EscapeDataString(since.ToString("O"))}", _jsonOptions);
            return products ?? new List<Product>();
        }
        catch (Exception ex) { Serilog.Log.Error(ex, "Exception in GetChangesAsync"); return new List<Product>(); }
    }

    
    public async Task<bool> DeleteProductAsync(string barcode)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/v1/products/{Uri.EscapeDataString(barcode)}");
            if (!response.IsSuccessStatusCode) { var err = await response.Content.ReadAsStringAsync(); Serilog.Log.Error($"Sync fail (DELETE api/v1/products/{Uri.EscapeDataString(barcode)}): {response.StatusCode} - {err}"); if (response.StatusCode == System.Net.HttpStatusCode.BadRequest || response.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity || response.StatusCode == System.Net.HttpStatusCode.Conflict) { return true; } } return response.IsSuccessStatusCode;
        }
        catch (Exception ex) { Serilog.Log.Error(ex, "Exception in DeleteProductAsync"); return false; }
    }

    public async Task<bool> SyncProductAsync(Product product)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/products", product, _jsonOptions);
            if (!response.IsSuccessStatusCode) { var err = await response.Content.ReadAsStringAsync(); Serilog.Log.Error($"Sync fail (api/v1/products): {response.StatusCode} - {err}"); if (response.StatusCode == System.Net.HttpStatusCode.BadRequest || response.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity || response.StatusCode == System.Net.HttpStatusCode.Conflict) { return true; } } return response.IsSuccessStatusCode;
        }
        catch (Exception ex) { Serilog.Log.Error(ex, "Exception in SyncProductAsync"); return false; }
    }

    public async Task<bool> SyncOrderAsync(Order order)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/orders", order, _jsonOptions);
            if (!response.IsSuccessStatusCode) { var err = await response.Content.ReadAsStringAsync(); Serilog.Log.Error($"Sync fail (api/v1/orders): {response.StatusCode} - {err}"); if (response.StatusCode == System.Net.HttpStatusCode.BadRequest || response.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity || response.StatusCode == System.Net.HttpStatusCode.Conflict) { return true; } } return response.IsSuccessStatusCode;
        }
        catch (Exception ex) { Serilog.Log.Error(ex, "Exception in SyncOrderAsync"); return false; }
    }

    
    public async Task<bool> SyncUserAsync(User user)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/users", user, _jsonOptions);
            if (!response.IsSuccessStatusCode) { var err = await response.Content.ReadAsStringAsync(); Serilog.Log.Error($"Sync fail (api/v1/users): {response.StatusCode} - {err}"); if (response.StatusCode == System.Net.HttpStatusCode.BadRequest || response.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity || response.StatusCode == System.Net.HttpStatusCode.Conflict) { return true; } } return response.IsSuccessStatusCode;
        }
        catch (Exception ex) { Serilog.Log.Error(ex, "Exception in SyncUserAsync"); return false; }
    }

    public async Task<bool> DeleteUserAsync(string username)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/v1/users/{Uri.EscapeDataString(username)}");
            if (!response.IsSuccessStatusCode) { var err = await response.Content.ReadAsStringAsync(); Serilog.Log.Error($"Sync fail (DELETE api/v1/users/{Uri.EscapeDataString(username)}): {response.StatusCode} - {err}"); if (response.StatusCode == System.Net.HttpStatusCode.BadRequest || response.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity || response.StatusCode == System.Net.HttpStatusCode.Conflict) { return true; } } return response.IsSuccessStatusCode;
        }
        catch (Exception ex) { Serilog.Log.Error(ex, "Exception in DeleteUserAsync"); return false; }
    }

    

    public async Task<bool> SyncShiftAsync(CashRegisterShift shift)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/shifts", shift, _jsonOptions);
            if (!response.IsSuccessStatusCode) { var err = await response.Content.ReadAsStringAsync(); Serilog.Log.Error($"Sync fail (api/v1/shifts): {response.StatusCode} - {err}"); if (response.StatusCode == System.Net.HttpStatusCode.BadRequest || response.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity || response.StatusCode == System.Net.HttpStatusCode.Conflict) { return true; } } return response.IsSuccessStatusCode;
        }
        catch (Exception ex) { Serilog.Log.Error(ex, "Exception in SyncShiftAsync"); return false; }
    }

    public async Task<PosApplication.Interfaces.Local.LoginResponse?> LoginAsync(string username, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/auth/login", new PosDomain.Entities.LoginRequest { Username = username, Password = password }, _jsonOptions);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<PosApplication.Interfaces.Local.LoginResponse>(_jsonOptions);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                throw new UnauthorizedAccessException("Credenciales inválidas en la nube.");
            }
            return null; // For other errors, might fallback
        }
        catch (Exception ex) { Serilog.Log.Error(ex, "Exception in LoginAsync"); return null; }
    }

    public async Task<bool> SendPingAsync(PosDomain.Entities.PingPayload payload)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/sync/ping", payload, _jsonOptions);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> SyncInventoryMovementAsync(PosDomain.Entities.InventoryMovement movement)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/inventorymovements", movement, _jsonOptions);
            return response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.BadRequest;
        }
        catch { return false; }
    }
}
