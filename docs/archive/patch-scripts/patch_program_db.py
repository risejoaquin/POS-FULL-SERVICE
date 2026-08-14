with open('PosServer/Program.cs', 'r') as f:
    c = f.read()

start_marker = "// Init Database"
end_marker = "app.Run();"

start_idx = c.find(start_marker)
end_idx = c.find(end_marker)

new_init = """// Init Database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CentralDbContext>();
    
    try
    {
        dbContext.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error aplicando migraciones: {ex.Message}");
        throw;
    }
}
"""

c = c[:start_idx] + new_init + c[end_idx:]

with open('PosServer/Program.cs', 'w') as f:
    f.write(c)
