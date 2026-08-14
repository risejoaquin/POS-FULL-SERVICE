# PHASE 1F.3 — Inventory Legacy Windows Cleanup

## Status
PENDING LOCAL VERIFICATION

## Objective
Remove the remaining direct `PosDbContext` dependency from inventory UI orchestration and legacy inventory windows.

## Scope
Modified only inventory-related UI windows and the local inventory application service contract/implementation:

- `PosCore/ViewModels/InventoryViewModel.cs`
- `PosCore/Views/SuppliesWindow.xaml.cs`
- `PosCore/Views/SupplyEditorWindow.xaml.cs`
- `PosCore/Views/ProductRecipeWindow.xaml.cs`
- `PosCore/Views/ProductModifiersConfigWindow.xaml.cs`
- `PosApplication/Interfaces/Local/IInventoryAppService.cs`
- `PosInfrastructure/Services/Local/InventoryAppService.cs`

## Result
Inventory UI no longer receives or passes `PosDbContext` to legacy windows. The legacy windows now depend on `IInventoryAppService` and database access is encapsulated in `InventoryAppService`.

## Explicitly not changed
- Checkout
- Returns
- SyncService
- PosDomain
- PosServer
- Migrations
- RLS
- Licensing
- Provisioning
- PosBuilder

## Local validation required
Run:

```powershell
dotnet test
dotnet build -c Release Pos.sln
```

Gate:

- Tests: 0 failed
- Build: 0 errors
