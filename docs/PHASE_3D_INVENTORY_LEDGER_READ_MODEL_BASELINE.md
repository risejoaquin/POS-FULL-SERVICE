# PHASE 3D — Inventory Ledger Read Model Baseline

## Scope

This phase introduces a read-only baseline for reconstructing inventory balances from ledger movements.

## Objective

Create a safe domain read model that uses `InventoryMovement.SignedQuantity` so existing mixed sign semantics do not corrupt stock reconstruction.

## Changes

- Added `InventoryLedgerReadModel`.
- Added `InventoryLedgerBalance`.
- Added tests for product reconstruction.
- Added tests for supply reconstruction.
- Added tests for legacy negative movement handling.
- Added tests for tenant filtering.
- Added tests for grouped balances.
- Added static architecture tests requiring the read model to stay read-only.
- Added documentation in `docs/INVENTORY_LEDGER_READ_MODEL.md`.

## Not changed

- No migrations were changed.
- No EF mappings were changed.
- No schema was changed.
- Checkout behavior was not changed.
- Returns behavior was not changed.
- Sync behavior was not changed.
- Reports behavior was not changed.
- Existing stock columns were not replaced.
- Existing ledger rows were not normalized.

## Safety decision

`Product.StockQuantity` and `Supply.Stock` remain the operational source of truth.

The new read model is only a baseline for verification, drift detection, and future reconstruction work.

## Validation expectation

Baseline before this phase: 107 tests.

This phase adds 13 tests.

Expected result:

```text
120 tests passed
0 failed
0 build errors
```
