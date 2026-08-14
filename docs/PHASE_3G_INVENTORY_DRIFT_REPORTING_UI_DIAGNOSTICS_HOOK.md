# PHASE 3G — Inventory Drift Reporting UI/Diagnostics Hook

## Objective

Connect the inventory drift reporting service to an internal POS diagnostics entry point without adding automatic correction or changing inventory behavior.

## Changes

- Added `InventoryDriftDiagnosticsFormatter` in `PosCore/Diagnostics`.
- Added `ShowInventoryDriftDiagnosticsAsync` command to `InventoryViewModel`.
- Added read-only diagnostic state properties to `InventoryViewModel`.
- Added an internal `Diagnóstico Drift` button to `InventoryWindow.xaml`.
- Added static guardrail tests for the diagnostics hook.
- Added `docs/INVENTORY_DRIFT_DIAGNOSTICS_HOOK.md`.

## Not Changed

- No schema change.
- No migrations.
- No checkout changes.
- No sync changes.
- No automatic correction.
- No stock mutation.
- No changes to `Product.StockQuantity` or `Supply.Stock`.
- No changes to `InventoryMovement` storage semantics.

## Safety

The UI hook calls `IInventoryDriftReportingService.GetCombinedDriftReportAsync` and formats the result. It does not call `SaveChanges`, does not mutate tracked inventory entities, and does not rebuild stock.

## Expected Result

The previous baseline had 137 tests passing. This phase adds 4 static guardrail tests, so the expected suite is approximately 141 tests passing with 0 failed tests and 0 build errors.
