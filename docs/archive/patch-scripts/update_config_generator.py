import re

with open('PosBuilder/ConfigurationGenerator.cs', 'r', encoding='utf-8') as f:
    content = f.read()

# Find GenerateAppSettings method
match = re.search(r'public string GenerateAppSettings\(ConfigModel model\)\s*\{(.*?)var options = new JsonSerializerOptions', content, re.DOTALL)

if match:
    new_method = """public string GenerateAppSettings(ConfigModel model)
        {
            var config = new
            {
                Api = new
                {
                    BaseUrl = model.ApiBaseUrl
                },
                Tenant = new
                {
                    Id = model.TenantId
                },
                Device = new
                {
                    Id = Guid.NewGuid().ToString()
                },
                License = new 
                {
                    Key = model.LicenseKey
                }
            };
            var options = new JsonSerializerOptions"""
    content = content[:match.start()] + new_method + content[match.end()-27:]
    with open('PosBuilder/ConfigurationGenerator.cs', 'w', encoding='utf-8') as f:
        f.write(content)
