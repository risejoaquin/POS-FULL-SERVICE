# Inventory Ledger Sign Semantics

## Phase

PHASE 3C — InventoryMovement Sign Semantics Audit

## Purpose

This document freezes the current sign semantics before any ledger normalization or data migration is attempted.

The current codebase has mixed historical conventions for `InventoryMovement.Quantity`:

- Some local checkout paths store sale and recipe consumption quantities as negative values.
- Some inventory service paths store sale, recipe consumption, return and restock quantities as positive values and rely on `MovementType` to infer the stock direction.

This means `Quantity` cannot safely be interpreted directly as the stock delta.

## Canonical future convention

The canonical future convention should be:

- `Quantity` stores the absolute quantity as a positive value.
- `MovementType` defines whether the movement increases, decreases or neutrally adjusts stock.
- `SignedQuantity` / `ToSignedQuantity()` is the only safe stock-delta representation.

## Current compatibility rule

Because historical rows may already contain a legacy negative quantity, Phase 3C does not rewrite rows and performs no data migration.

For interpretation only:

- `AbsoluteQuantity` returns `Math.Abs(Quantity)`.
- `SignedQuantity` normalizes sale and recipe-consumption movements to negative values.
- `SignedQuantity` normalizes return and restock movements to positive values.
- `Adjustment` preserves the stored sign because adjustments may intentionally add or subtract stock.

## Legacy negative quantity

A legacy negative quantity is any `InventoryMovement` row where `Quantity < 0`.

This is tolerated for interpretation so existing data can be read safely. It is not the desired convention for newly created rows.

## New movement validation

`Validate()` remains strict and requires `Quantity > 0`. This is the intended rule for new canonical movements.

`ValidateForLedgerInterpretation()` allows negative non-zero quantities so legacy rows can be interpreted without being rejected as invalid historical data.

## No data migration

Phase 3C intentionally performs no data migration.

Before any migration is introduced, the project needs a dedicated phase that verifies:

- which rows currently store negative values,
- which services still create negative rows,
- whether reports read `Quantity` directly,
- whether sync payloads expect stored signs,
- whether server-side replay depends on historical signs.

## Risks still open

- RISK-INV-002 — signs of `InventoryMovement.Quantity` are not normalized at storage level.
- RISK-INV-007 — there is no trusted stock rebuild process from ledger yet.
- RISK-INV-008 — reports or sync may still read `Quantity` directly instead of `SignedQuantity`.
