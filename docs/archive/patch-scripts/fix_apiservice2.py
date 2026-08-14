with open("PosCore/Services/ApiService.cs", "r") as f:
    text = f.read()

# Fix constructor missing }
text = text.replace('        catch (Exception ex) \n        {\n            Serilog.Log.Error(ex, $"Error creating Uri for BaseUrl: \'{baseUrl}\'");\n            _httpClient.BaseAddress = new Uri("https://pos-full-service-production.up.railway.app/");\n        }\n    public async Task<List<Product>> GetProductsAsync()', '        catch (Exception ex) \n        {\n            Serilog.Log.Error(ex, $"Error creating Uri for BaseUrl: \'{baseUrl}\'");\n            _httpClient.BaseAddress = new Uri("https://pos-full-service-production.up.railway.app/");\n        }\n    }\n\n    public async Task<List<Product>> GetProductsAsync()')

# Fix SendPingAsync missing }
text = text.replace('        catch\n        {\n            return false;\n        }\n    public async Task<bool> SyncInventoryMovementAsync', '        catch\n        {\n            return false;\n        }\n    }\n\n    public async Task<bool> SyncInventoryMovementAsync')

with open("PosCore/Services/ApiService.cs", "w") as f:
    f.write(text)

