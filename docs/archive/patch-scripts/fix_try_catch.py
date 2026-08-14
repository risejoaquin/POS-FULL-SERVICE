import re

with open("PosCore/Services/ApiService.cs", "r") as f:
    content = f.read()

content = re.sub(r'(\s+)catch\s*\(Exception', r'\1}\1catch (Exception', content)

# Fix the base address try catch
content = content.replace('        if (string.IsNullOrWhiteSpace(baseUrl))\n        {\n            baseUrl = "https://pos-full-service-production.up.railway.app/";\n        else if (!baseUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))\n        {\n            baseUrl = "https://" + baseUrl.TrimStart(\'/\');\n                try', '        if (string.IsNullOrWhiteSpace(baseUrl))\n        {\n            baseUrl = "https://pos-full-service-production.up.railway.app/";\n        }\n        else if (!baseUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))\n        {\n            baseUrl = "https://" + baseUrl.TrimStart(\'/\');\n        }\n        try')

# Fix namespace/usings
content = content.replace('using PosCore.Models;', 'using PosDomain.Entities;')
content = content.replace('PosCore.Models.', 'PosDomain.Entities.')

with open("PosCore/Services/ApiService.cs", "w") as f:
    f.write(content)
