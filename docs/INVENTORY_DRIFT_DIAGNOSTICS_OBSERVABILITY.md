# Inventory Drift Diagnostics Observability

## Purpose

This document defines the observability and error-handling baseline for the internal inventory drift diagnostic flow.

## Scope

The diagnostic hook remains read-only and diagnostic only. It does not auto-correct inventory, does not mutate stock, performs no schema change, introduces no migrations, and makes no checkout changes and no sync changes.

## Error handling rules

- Diagnostic failures must not crash the inventory screen.
- The UI must show a safe error state: `Error al calcular diagnóstico`.
- The user-facing message must avoid stack traces.
- Technical details belong in logs, not in the main UX copy.
- `IsInventoryDriftDiagnosticsRunning` must be reset in a `finally` block.

## Observability rules

The ViewModel logs:

- diagnostic start,
- diagnostic success with summary counts,
- diagnostic failure with exception details.

The success log includes total items, drifted items and negative ledger items.

## Non-goals

This phase does not add automatic repair, ledger rebuild, stock reconciliation, schema changes, migrations, checkout changes, no sync changes, or server-side replay changes.
