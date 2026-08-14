with open("PosCore/Services/ApiService.cs", "r") as f:
    text = f.read()

text = text.replace('            _httpClient.BaseAddress = new Uri("https://pos-full-service-production.up.railway.app/");\n        }\n\n    public async Task<List<Product>> GetProductsAsync()', '            _httpClient.BaseAddress = new Uri("https://pos-full-service-production.up.railway.app/");\n        }\n    }\n\n    public async Task<List<Product>> GetProductsAsync()')

text = text.replace('            _httpClient.BaseAddress = new Uri("https://pos-full-service-production.up.railway.app/");\n        }\n    public async Task<List<Product>> GetProductsAsync()', '            _httpClient.BaseAddress = new Uri("https://pos-full-service-production.up.railway.app/");\n        }\n    }\n\n    public async Task<List<Product>> GetProductsAsync()')

with open("PosCore/Services/ApiService.cs", "w") as f:
    f.write(text)
