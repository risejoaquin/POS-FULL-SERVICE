# PHASE 3F — Inventory Drift Reporting Integration Baseline

## Objective

Expose the inventory drift detection baseline through a local application/infrastructure service without changing checkout, sync, schema, migrations, or operational stock behavior.

## Scope

This phase introduces a read-only reporting integration for inventory drift diagnostics.

## Added integration

- `IInventoryDriftReportingService` in `PosApplication.Interfaces.Local`.
- `InventoryDriftReportingService` in `PosInfrastructure.Services.Local`.
- Dependency injection registration in `PosCore.Extensions.ServiceCollectionExtensions`.

## Reporting methods

- `GetProductDriftReportAsync(...)`
- `GetSupplyDriftReportAsync(...)`
- `GetCombinedDriftReportAsync(...)`

## Data sources

The integration reads:

- `Product.StockQuantity`
- `Supply.Stock`
- `InventoryMovement`

Then delegates drift calculation to:

- `InventoryDriftDetectionReadModel`
- `InventoryLedgerReadModel`
- `InventoryMovement.SignedQuantity`

## Critical safety rule

This is an internal diagnostic reporting baseline only. It does not auto-correct inventory, does not mutate stock, and does not persist fixes.

## No schema change

This phase is intentionally a no schema change baseline.

It does not add tables, columns, indexes, migrations, or EF mappings.

## Not changed

- No checkout changes.
- No returns changes.
- No sync changes.
- No automatic correction.
- No stock rebuild.
- No migration.
- No schema change.
- No reporting UI.
- No API endpoint.

## Why this phase matters

The system can now ask for a drift report from application/infrastructure code without duplicating ledger math or reading `InventoryMovement.Quantity` directly as a delta.

This prepares a future UI/admin diagnostic screen or maintenance report while keeping correction manual and explicit.

## Remaining risks

- `RISK-INV-006`: PosServer central still mutates stock directly during sync.
- `RISK-INV-007`: There is still no automated stock rebuild from ledger.
- `RISK-INV-008`: Drift reports are available internally but not yet exposed in UI/API.
