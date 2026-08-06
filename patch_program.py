with open('./PosServer/Program.cs', 'r', encoding='utf-8') as f:
    content = f.read()

# Add ReferenceHandler.IgnoreCycles to AddControllers
old_controllers = "builder.Services.AddControllers();"
new_controllers = """builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});"""

content = content.replace(old_controllers, new_controllers)

with open('./PosServer/Program.cs', 'w', encoding='utf-8') as f:
    f.write(content)
