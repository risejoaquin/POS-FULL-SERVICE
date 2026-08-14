# Inventory Ledger Audit

## Canonical goal

Inventory should eventually be represented as a ledger of immutable movements, with stock derived or reconciled from movement history. The current application still uses a hybrid model:

- `Product.StockQuantity` and `Supply.Stock` are mutable snapshots.
- `InventoryMovement` records stock-changing events.
- Some services update both snapshot and movement in the same flow.

## Current model

| Area | Current behavior | Risk |
|---|---|---|
| Product sale | Decrements `Product.StockQuantity` and records movement | Can double-decrement if multiple services are composed incorrectly |
| Recipe consumption | Decrements `Supply.Stock` and records movement | Supply availability is not consistently prechecked |
| Return | Increments `Product.StockQuantity` and records movement | Needs idempotency guard before production hardening |
| Restock | Increments `Product.StockQuantity` and records movement | Needs consistent signed quantity convention |
| Manual adjustment | Changes `Product.StockQuantity` and records adjustment | Needs stronger reason/user/audit rules |

## Recommended canonical movement convention

For future phases, use this convention:

```text
InventoryMovement.Quantity = positive magnitude
InventoryMovement.MovementType = semantic direction
InventoryMovement.SignedQuantity = calculated direction-aware value
```

Example:

| MovementType | Quantity | SignedQuantity |
|---|---:|---:|
| Sale | 2 | -2 |
| RecipeConsumption | 0.250 | -0.250 |
| Return | 2 | 2 |
| Restock | 10 | 10 |
| Adjustment | depends on explicit adjustment semantics | pending |

## Do not change yet

Do not normalize existing runtime movement writes until there is a migration or compatibility adapter, because some current code may already persist negative sale quantities.

## Next safe sequence

1. Add compatibility helpers to read legacy signed movement rows safely.
2. Normalize new movement writes behind factories.
3. Add integration tests for checkout product stock, recipe supply stock and ledger rows.
4. Add idempotency checks for inventory mutations.
5. Add optimistic concurrency tests.
6. Only then consider schema changes or derived-stock ledger mode.
