# PHASE 3C — InventoryMovement Sign Semantics Audit

## Scope

This phase audits and stabilizes sign interpretation for `InventoryMovement` without changing stored data, schema, migrations, reports or sync.

## Changes

- Added sign interpretation helpers to `InventoryMovement`.
- Made `SignedQuantity` robust for both canonical positive quantities and legacy negative quantities.
- Added `AbsoluteQuantity`.
- Added `HasLegacyNegativeStoredQuantity`.
- Added `HasCanonicalPositiveStoredQuantity`.
- Added `StockDirection`.
- Added `ValidateForLedgerInterpretation()` for reading existing ledger rows safely.
- Added domain tests for canonical positive rows and legacy negative rows.
- Added architecture tests to keep sign-semantics documentation present.

## Not changed

- No migration was added.
- No existing data is rewritten.
- No EF mapping was changed.
- No report was changed.
- No sync contract was changed.
- No server replay behavior was changed.
- No checkout behavior was intentionally changed.

## Compatibility decision

`Validate()` remains strict for new canonical rows and still requires `Quantity > 0`.

`ValidateForLedgerInterpretation()` exists specifically for reading legacy rows that may have `Quantity < 0`.

## Important note

This phase does not fully normalize `InventoryMovement.Quantity`. It only creates safe interpretation rules so future phases can migrate storage semantics deliberately.

## Expected test impact

Previous baseline: 99 tests.

Phase 3C adds 8 tests:

- 6 domain tests for sign interpretation.
- 2 architecture/documentation tests.

Expected result: 107 tests passed, 0 failed.
