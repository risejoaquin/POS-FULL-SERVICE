# Inventory Drift Detection

## Purpose

Inventory drift detection compares the current operational stock columns against the stock that can be reconstructed from the inventory ledger.

Operational stock remains the active source used by the POS:

- `Product.StockQuantity`
- `Supply.Stock`

The ledger read model calculates reconstructed stock using `InventoryMovement.SignedQuantity`.

## Formula

```text
ledger_current_quantity = opening_quantity + sum(InventoryMovement.SignedQuantity)
drift_quantity = operational_quantity - ledger_current_quantity
```

## Interpretation

- `drift_quantity = 0` means operational stock matches the reconstructed ledger.
- `drift_quantity > 0` means operational stock is higher than the ledger.
- `drift_quantity < 0` means operational stock is lower than the ledger.

## Scope

This baseline is detection-only. It does not auto-correct inventory.

It is intentionally a no schema change baseline: it does not require migrations, EF mapping changes, or database column changes.

It does not:

- update `Product.StockQuantity`
- update `Supply.Stock`
- insert correction movements
- rewrite existing `InventoryMovement` rows
- change checkout behavior
- change sync behavior
- change reports
- change EF mappings
- change database schema

## Why detection first

Automatic correction is risky while historical movement signs, offline sync replay, returns, and reports still rely on the existing data model.

Detection creates a safe foundation for later phases:

1. Identify drift.
2. Classify drift source.
3. Decide whether to create correction movements.
4. Only then allow controlled reconciliation.

## Current rule

Use `InventoryDriftDetectionReadModel` for diagnostics only.

Do not use drift detection as the source of truth for checkout, sync, or stock mutation yet.
