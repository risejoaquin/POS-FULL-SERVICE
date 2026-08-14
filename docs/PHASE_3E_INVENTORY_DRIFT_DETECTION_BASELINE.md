# PHASE 3E — Inventory Drift Detection Baseline

## Status

Pending local verification.

## Objective

Add a diagnostic baseline that compares operational inventory columns with ledger-reconstructed inventory balances.

## Added

- `InventoryDriftDetectionReadModel`
- `InventoryDriftItem`
- `InventoryDriftReport`
- Domain tests for product and supply drift detection
- Static architecture tests ensuring the model remains read-only
- Documentation for drift detection

## Not changed

- No schema changes
- No migrations
- No EF mappings
- No checkout changes
- No returns changes
- No sync changes
- No report changes
- No automatic stock correction
- No `InventoryMovement` rewrite

## Integration safety

The new model only receives movements and quantity dictionaries and returns a report. It does not depend on EF, repositories, DbContext, WPF, ASP.NET, or infrastructure services.

## Expected validation

```text
dotnet test
0 failed

dotnet build -c Release Pos.sln
0 errors
```

## Next phase

Recommended next phase:

```text
PHASE 3F — Inventory Drift Detection Integration Surface
```

That phase should expose diagnostics through an application/infrastructure boundary without correcting stock automatically.


## Hotfix note

Added the explicit phrase `no schema change` to `docs/INVENTORY_DRIFT_DETECTION.md` so the documentation guardrail test matches the documented intent. No production code was changed.
