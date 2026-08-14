#!/bin/bash
export DOTNET_ROOT="/root/.dotnet"
export PATH="$PATH:/root/.dotnet:/root/.dotnet/tools"

cd PosDataLib

sed -i 's/_sessionManager.GetTenantId()/"dummy"/g' Data/PosDbContext.cs
sed -i 's/_sessionManager.GetUserId()/"dummy"/g' Data/PosDbContext.cs

dotnet add package Microsoft.Extensions.Configuration
dotnet add package Microsoft.Extensions.Configuration.Json

sed -i 's/return Result.Success();/return;/g' Entities/Order.cs

cat << 'INNER_EOF' > Data/PosDbContextFactory.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace PosInfrastructure.Data.Local
{
    public class PosDbContextFactory : IDesignTimeDbContextFactory<PosDbContext>
    {
        public PosDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PosDbContext>();
            optionsBuilder.UseSqlite("Data Source=dummy.db");
            return new PosDbContext(optionsBuilder.Options);
        }
    }
}
INNER_EOF

dotnet build
dotnet ef migrations add InitialMigration -c PosDbContext -o ../PosCore/Migrations
