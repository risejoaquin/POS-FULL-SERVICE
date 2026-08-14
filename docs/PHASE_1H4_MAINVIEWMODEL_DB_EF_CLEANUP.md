# PHASE 1H.4 — Remove remaining DbContext/EF from MainViewModel

## Scope

This iteration removes the remaining direct `PosDbContext` / Entity Framework dependency from `PosCore/ViewModels/MainViewModel.cs` after checkout transaction extraction in Phase 1H.3.

## Changes

- Removed `PosDbContext` constructor dependency from `MainViewModel`.
- Removed public `DbContext` field from `MainViewModel`.
- Removed unused legacy local `IInventoryService` constructor dependency.
- Removed unused `IApiService` dependency from `MainViewModel`.
- Removed `Microsoft.EntityFrameworkCore`, `PosInfrastructure.Data.Local`, `PosInfrastructure.Services.Local`, and `System.Text.Json` usings from `MainViewModel`.
- Updated `MainViewModelTests` constructors to match the cleaned ViewModel signature.

## Static gate

`PosCore/ViewModels/MainViewModel.cs` must have 0 direct references to:

- `PosDbContext`
- `DbContext`
- `Microsoft.EntityFrameworkCore`
- `SaveChanges`
- `SaveChangesAsync`
- `BeginTransaction`
- `BeginTransactionAsync`
- `_dbContext`
- `InventoryMovements`
- `CashMovements`
- `Orders.Add`
- `OrderManagementService`
- `PosInfrastructure`

## Not changed

- Checkout transaction behavior
- Payment UI
- Receipt printing
- Sync trigger
- Returns
- Inventory services
- PosDomain
- PosServer
- migrations
- RLS
- licensing
- provisioning
- PosBuilder
