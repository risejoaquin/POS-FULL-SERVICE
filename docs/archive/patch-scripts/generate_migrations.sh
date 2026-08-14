#!/bin/bash
export DOTNET_ROOT="/root/.dotnet"
export PATH="$PATH:/root/.dotnet:/root/.dotnet/tools"

# Create a net8.0 library with the entities and DbContext so it can be built and run on Linux
mkdir -p PosDataLib
cd PosDataLib
dotnet new classlib
dotnet add package Microsoft.EntityFrameworkCore.Sqlite -v 8.0.0
dotnet add package Microsoft.EntityFrameworkCore.Design -v 8.0.0
# Copy all entities and dbcontext over to this library
cp -r ../PosDomain/Entities .
cp -r ../PosDomain/Interfaces .
mkdir -p Data
cp ../PosInfrastructure/Data/Local/PosDbContext.cs Data/
cp ../PosInfrastructure/Data/Local/PosDbContextFactory.cs Data/

sed -i 's/using PosApplication.Interfaces.Server;//g' Data/PosDbContext.cs
sed -i 's/private readonly ITenantContext _sessionManager;//g' Data/PosDbContext.cs
sed -i 's/public PosDbContext(DbContextOptions<PosDbContext> options, ITenantContext sessionManager) : base(options)/public PosDbContext(DbContextOptions<PosDbContext> options) : base(options)/g' Data/PosDbContext.cs
sed -i 's/_sessionManager = sessionManager;//g' Data/PosDbContext.cs

sed -i 's/using PosApplication.Interfaces.Server;//g' Data/PosDbContextFactory.cs
sed -i 's/, new DummyTenantContext()//g' Data/PosDbContextFactory.cs
sed -i '/class DummyTenantContext/,/}/d' Data/PosDbContextFactory.cs

dotnet build
dotnet ef migrations add InitialMigration -c PosDbContext -o ../PosCore/Migrations
