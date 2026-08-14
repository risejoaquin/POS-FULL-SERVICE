import re

with open("PosCore/Services/ApiService.cs", "r") as f:
    text = f.read()

# Fix the base address try catch
text = text.replace('        if (string.IsNullOrWhiteSpace(baseUrl))\n        {\n            baseUrl = "https://pos-full-service-production.up.railway.app/";\n        else if (!baseUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))', '        if (string.IsNullOrWhiteSpace(baseUrl))\n        {\n            baseUrl = "https://pos-full-service-production.up.railway.app/";\n        }\n        else if (!baseUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))')

text = text.replace('        {\n            baseUrl = "https://" + baseUrl.TrimStart(\'/\');\n                try ', '        {\n            baseUrl = "https://" + baseUrl.TrimStart(\'/\');\n        }\n        try ')

text = text.replace('        try \n        {\n            _httpClient.BaseAddress = new Uri(baseUrl);\n        catch', '        try \n        {\n            _httpClient.BaseAddress = new Uri(baseUrl);\n        }\n        catch')

# Fix catch blocks
text = re.sub(r'        catch \(Exception ex\) \{ ([^}]+) \}', r'        }\n        catch (Exception ex) { \1 }', text)

text = text.replace('using PosCore.Models;', 'using PosDomain.Entities;')
text = text.replace('PosCore.Models.', 'PosDomain.Entities.')

with open("PosCore/Services/ApiService.cs", "w") as f:
    f.write(text)
