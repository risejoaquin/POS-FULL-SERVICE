import re

with open('PosCore/App.xaml.cs', 'r', encoding='utf-8') as f:
    content = f.read()

# Pattern to replace
pattern = r'// Aplicar migraciones y Backup(.*?)// Seed inicial'
replacement = """// Aplicar migraciones y Backup
        using (var scope = ServiceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<PosDbContext>();
            
            // Database Migration
            dbContext.Database.Migrate();
            
            dbContext.InitializeDatabaseSettings();
            var connStr = secureSettings.DatabaseSettings.ConnectionString;
            
            DatabaseBackupService.ManageDatabaseBackup(connStr);
            
            try 
            {
                // Seed inicial"""

new_content = re.sub(pattern, replacement, content, flags=re.DOTALL)

with open('PosCore/App.xaml.cs', 'w', encoding='utf-8') as f:
    f.write(new_content)
