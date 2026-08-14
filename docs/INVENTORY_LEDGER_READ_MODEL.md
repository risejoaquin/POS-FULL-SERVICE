# Inventory Ledger Read Model

## Purpose

This document defines the Phase 3D inventory ledger read model baseline.

The read model reconstructs product and supply balances from `InventoryMovement` rows using `SignedQuantity` instead of reading raw `Quantity` as a stock delta.

This is a read-only baseline. It does not replace current stock columns yet.

## Why this exists

Previous phases established that `InventoryMovement.Quantity` has mixed historical semantics:

- Newer/canonical movements should store absolute positive quantities.
- Some legacy movements can store negative quantities.
- `MovementType` defines whether stock increases or decreases.
- `SignedQuantity` is the safe delta for reconstruction.

Because of that, any ledger read model must use `SignedQuantity`.

## Added read model

Phase 3D introduces:

- `PosDomain.ReadModels.InventoryLedgerReadModel`
- `PosDomain.ReadModels.InventoryLedgerBalance`

The read model supports:

- product balance reconstruction
- supply balance reconstruction
- grouped product balances
- grouped supply balances
- optional tenant filtering
- opening quantities
- legacy negative movement detection

## Interpretation rule

```text
CurrentQuantity = OpeningQuantity + Sum(InventoryMovement.SignedQuantity)
```

Raw `Quantity` must not be used as the delta.

## Scope limits

Phase 3D does not change:

- database schema
- migrations
- EF mappings
- checkout behavior
- returns behavior
- sync behavior
- reports behavior
- stock source of truth
- existing `InventoryMovement` rows

There is no schema change in this phase.

## Current status

Current stock columns remain the operational source of truth:

- `Product.StockQuantity`
- `Supply.Stock`

The ledger read model is a verification and reconstruction baseline only.

## Future use

Later phases can use this read model to:

- compare stock columns against reconstructed ledger balances
- detect drift
- build repair reports
- safely migrate toward ledger-backed stock
- harden sync replay behavior

## Safety rule

Do not mutate stock inside the read model.

The read model must remain side-effect free:

- no `DbContext`
- no `SaveChanges`
- no `StockQuantity +=/-=`
- no `Stock +=/-=`
