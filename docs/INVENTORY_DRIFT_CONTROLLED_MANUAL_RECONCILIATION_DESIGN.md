# Inventory Drift Controlled Manual Reconciliation Design

## Purpose

This document defines the design baseline for a future controlled manual reconciliation workflow for inventory drift.

The design is intentionally conservative. It is design only, diagnostic only, manual review only, and report-only at this stage.

## Non-goals

This phase does not execute inventory reconciliation.

Mandatory limits:

- does not auto-correct inventory
- no auto-correction
- no inventory mutation
- no stock adjustment execution
- no persistence of inventory changes
- no schema change
- no migrations
- no checkout changes
- no sync changes
- no RBAC change
- no audit table creation

## Required future controls

Before any future manual reconciliation is allowed, the system must define and enforce:

1. Administrative permission requirement.
2. Persistent audit trail requirement.
3. Evidence requirement using exported drift report.
4. Physical count confirmation.
5. Explicit reason or business justification.
6. Sync-safe validation to avoid conflicting remote updates.
7. Clear separation between diagnosis and correction.

## Proposed future workflow

1. Run inventory drift diagnostics.
2. Export the drift report.
3. Start manual review.
4. Review physical stock and ledger-derived quantity.
5. Confirm the item requires reconciliation.
6. Verify administrative permission.
7. Record reason, evidence and reviewer.
8. Execute reconciliation in a future phase only after audit and sync-safe protection exist.

## Current phase behavior

PHASE 3L only exposes a design readiness state in the inventory UI.

It can mark that the design has been reviewed, but it does not create movements, does not modify Product.StockQuantity, does not modify Supply.Stock and does not persist inventory changes.

## Sync-safety design note

A future implementation must decide how reconciliation interacts with offline-first sync. A reconciliation must not be treated like an ordinary sale or restock. It must have its own auditable reason and idempotent sync behavior.

## Status

Controlled manual reconciliation is not implemented yet. This document only prepares the design baseline.
