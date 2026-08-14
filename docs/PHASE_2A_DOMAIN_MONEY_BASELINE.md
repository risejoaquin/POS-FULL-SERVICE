# PHASE 2A — Domain + Money Baseline Cleanup

## Scope

This iteration intentionally limits changes to the `Money` value object and direct unit tests.

## Objective

Replace the decimal-backed internal representation in `PosDomain.ValueObjects.Money` with integer minor units while preserving its public `Amount` decimal accessor for compatibility with the current codebase.

## Changes

- `Money` now stores monetary values in `MinorUnits`.
- `Amount` is derived from `MinorUnits / 100m`.
- Currency is normalized to uppercase.
- Blank currency values are rejected.
- Arithmetic operations now operate on integer minor units.
- Multiplication rounds to the nearest cent using `MidpointRounding.AwayFromZero`.
- Added tests for construction, rounding, arithmetic, currency mismatch, and invalid currency.

## Not changed

- No EF mappings were changed.
- No migrations were changed.
- Entity monetary fields remain `decimal` for now.
- Checkout behavior was not changed.
- Tax behavior was not changed.
- Order totals were not refactored.

## Reason

Changing all entity monetary columns from `decimal` to integer cents in one step would be high-risk because it affects EF migrations, existing SQLite/Postgres data, checkout, reports, returns, and sync. This baseline introduces a correct value object first, then later phases can migrate usage gradually.

## Static notes

`Money` is currently not widely used by the application services. This phase prepares the domain model for later adoption without forcing a database schema change.
