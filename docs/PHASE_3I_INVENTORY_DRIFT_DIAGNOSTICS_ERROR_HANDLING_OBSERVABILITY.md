# PHASE 3I — Inventory Drift Diagnostics Error Handling + Observability

## Objective

Improve the resilience and observability of the inventory drift diagnostic UX without changing inventory behavior.

## Changes

- Added diagnostic start/success/failure logging in `InventoryViewModel`.
- Added `InventoryDriftDiagnosticsLastError`.
- Added `InventoryDriftDiagnosticsLastRunAt`.
- Hardened the error formatter so user-facing errors do not expose stack traces by default.
- Documented the observability and error handling baseline.
- Added static guardrails for read-only behavior and safe diagnostics.

## Safety

This remains diagnostic only, read-only, and does not auto-correct inventory.

No schema change. No migrations. No checkout changes. No sync changes. No stock mutation. No automatic correction.

## Expected validation

- `dotnet test` should pass.
- `dotnet build -c Release Pos.sln` should complete with zero errors.


Guardrail: no sync changes.
