#!/bin/bash
export DOTNET_ROOT="/root/.dotnet"
export PATH="$PATH:/root/.dotnet:/root/.dotnet/tools"

cd PosDataLib

sed -i 's/public Result AddItem/public void AddItem/g' Entities/Order.cs
sed -i 's/return Result.Failure("Cannot add items to an order that is not open.");/return;/g' Entities/Order.cs
sed -i 's/return Result.Failure("Quantity must be greater than zero.");/return;/g' Entities/Order.cs
sed -i 's/public Result Complete/public void Complete/g' Entities/Order.cs
sed -i 's/return Result.Failure("Order must contain at least one item.");/return;/g' Entities/Order.cs

dotnet build
dotnet ef migrations add InitialMigration -c PosDbContext -o ../PosCore/Migrations
