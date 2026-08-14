# PHASE 2D — Product / Inventory Domain Alignment

## Status

PENDING LOCAL VERIFICATION

## Scope

This phase adds conservative domain invariants for product and inventory entities without changing persistence mappings, migrations, checkout, sync, reports, returns, or infrastructure services.

## Modified domain entities

- `Product`
  - Added `ValidateForSale()`.
  - Added `UpdateName(...)`.
  - Added `UpdateMinStockThreshold(...)`.
  - Added `Activate()` / `Deactivate()`.

- `Supply`
  - Added `IsLowStock`.
  - Added `CanConsume(...)`.
  - Added `DecreaseStock(...)` / `IncreaseStock(...)`.
  - Added `UpdateCost(...)`.
  - Added `UpdateMinStockThreshold(...)`.

- `RecipeItem`
  - Added `Validate()`.
  - Added `UpdateQuantity(...)`.
  - Added `RequiredFor(...)`.

- `InventoryMovement`
  - Added movement type constants.
  - Added movement factories for product sale, product return, product restock, and supply consumption.
  - Added `IsProductMovement`, `IsSupplyMovement`, `DecreasesStock`, `IncreasesStock`, and `SignedQuantity`.
  - Added `Validate()`.

## Tests added

- `SupplyTests`
- `RecipeItemTests`
- `InventoryMovementTests`
- Additional `Product` invariant tests

## Explicitly not changed

- Migrations
- EF mappings
- `PosDbContext`
- Decimal columns
- Checkout transaction
- Returns transaction
- Sync
- Reports
- `PosServer`
- `PosBuilder`
- RLS
- Licensing
- Provisioning

## Expected validation

Run:

```powershell
dotnet test
dotnet build -c Release Pos.sln
```

Expected:

- Tests: 0 failed
- Build: 0 errors
