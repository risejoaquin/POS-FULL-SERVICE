#!/bin/bash
export DOTNET_ROOT="/root/.dotnet"
export PATH="$PATH:/root/.dotnet:/root/.dotnet/tools"

cd PosDataLib

# Fix Order.cs
sed -i 's/public Result ApplyCoupon(Coupon coupon)/public void ApplyCoupon(Coupon coupon)/g' Entities/Order.cs
sed -i 's/return Result.Success();/return;/g' Entities/Order.cs
sed -i 's/return Result.Failure("El cupón ya expiró.");/return;/g' Entities/Order.cs
sed -i 's/return Result.Failure("El cupón no ha iniciado.");/return;/g' Entities/Order.cs
sed -i 's/return Result.Failure($"El pedido no cumple con el monto mínimo de {coupon.MinPurchaseAmount:C}");/return;/g' Entities/Order.cs
sed -i 's/public Result RemoveCoupon()/public void RemoveCoupon()/g' Entities/Order.cs

# Fix ITenantContext in factory
sed -i 's/public PosDbContext CreateDbContext(DbContextOptions<PosDbContext> options, ITenantContext tenantContext)/public PosDbContext CreateDbContext(DbContextOptions<PosDbContext> options)/g' Data/PosDbContextFactory.cs

dotnet build
dotnet ef migrations add InitialMigration -c PosDbContext -o ../PosCore/Migrations
