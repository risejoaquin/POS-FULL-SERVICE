with open('./PosCore/Services/ApiService.cs', 'r', encoding='utf-8') as f:
    content = f.read()

old_json = """    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };"""

new_json = """    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
    };"""

content = content.replace(old_json, new_json)

with open('./PosCore/Services/ApiService.cs', 'w', encoding='utf-8') as f:
    f.write(content)
