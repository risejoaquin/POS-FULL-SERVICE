# PHASE 3H — Inventory Drift Report UX Safety Pass

## Scope

This phase improves the user-facing safety of the inventory drift diagnostic hook.

## Changes

- Added explicit diagnostic status text to `InventoryViewModel`.
- Added error state tracking for drift diagnostics.
- Added running state tracking for drift diagnostics.
- Improved formatter text to distinguish no drift, drift detected, and calculation error.
- Added a read-only safety panel to `InventoryWindow.xaml`.
- Added static guardrails to prevent the diagnostic from being treated as an auto-correction tool.

## Not changed

- No schema change.
- No migrations.
- No checkout changes.
- No sync changes.
- No stock mutation.
- No SaveChanges.
- No automatic correction.

## Validation

Expected baseline after this phase: 145 tests passed, 0 failed, 0 build errors.
