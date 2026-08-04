using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using PosCore.Models;

namespace PosCore.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
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
            var products = await _httpClient.GetFromJsonAsync<List<Product>>("api/products", _jsonOptions);
            return products ?? new List<Product>();
        }
        catch (Exception)
        {
            return new List<Product>();
        }
    }

    public async Task<List<Product>> GetChangesAsync(DateTime since)
    {
        try
        {
            var products = await _httpClient.GetFromJsonAsync<List<Product>>($"api/products/changes?since={Uri.EscapeDataString(since.ToString("O"))}", _jsonOptions);
            return products ?? new List<Product>();
        }
        catch (Exception)
        {
            return new List<Product>();
        }
    }

    
    public async Task<bool> DeleteProductAsync(string barcode)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/products/{barcode}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> SyncProductAsync(Product product)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/products", product, _jsonOptions);
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> SyncOrderAsync(Order order)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/orders", order, _jsonOptions);
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }

    
    public async Task<bool> SyncUserAsync(User user)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/users", user, _jsonOptions);
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> DeleteUserAsync(string username)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/users/{username}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }

    
    public async Task<bool> SyncShiftAsync(CashRegisterShift shift)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/shifts", shift, _jsonOptions);
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<LoginResponse?> LoginAsync(string username, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", new LoginRequest { Username = username, Password = password }, _jsonOptions);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<LoginResponse>(_jsonOptions);
            }
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
