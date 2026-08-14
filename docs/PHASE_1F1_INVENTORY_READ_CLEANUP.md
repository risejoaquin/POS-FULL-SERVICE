# PHASE 1F.1 — Inventory Audit + Read Operations Cleanup

## Scope

This iteration only moves inventory read/list/export data loading behind `IInventoryAppService`.

## Completed

- Added `PosInfrastructure/Services/Local/InventoryAppService.cs`.
- Registered `IInventoryAppService -> InventoryAppService` in `PosCore/Extensions/ServiceCollectionExtensions.cs`.
- Updated `InventoryViewModel` to use `IInventoryAppService` for product loading/search and export data retrieval.

## Intentionally deferred to PHASE 1F.2

`InventoryViewModel` still keeps `PosDbContext` for write operations and legacy child windows:

- save product
- import products
- delete product
- open supplies window
- configure recipe
- configure modifiers

These are not touched in 1F.1 to avoid mixing read cleanup with write/stock mutation behavior.

## Validation expected

Run locally on Windows:

```powershell
dotnet test
dotnet build -c Release Pos.sln
```

Acceptance:

- 0 failed tests
- 0 build errors
