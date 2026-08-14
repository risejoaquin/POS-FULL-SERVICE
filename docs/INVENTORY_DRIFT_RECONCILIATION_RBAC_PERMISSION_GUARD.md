# Inventory Drift Reconciliation RBAC Permission Guard

## Purpose

This document defines the permission guard baseline for future controlled manual reconciliation of inventory drift.

The baseline is permission guard only. It prepares RBAC checks and UI state before any future reconciliation execution exists.

## Scope

- RBAC baseline
- permission guard baseline
- role-based preparation check
- required permission naming
- authorization copy in the diagnostics UI
- future reconciliation access boundary

## Required permission

```text
inventory.drift.reconciliation.prepare
```

## Allowed roles for preparation

```text
Admin
Administrador
InventoryManager
```

## Safety boundaries

- permission guard only
- RBAC baseline only
- diagnostic only
- manual review only
- report-only
- does not auto-correct
- no inventory mutation
- no stock adjustment
- no inventory persistence
- no schema change
- no migrations
- no checkout changes
- no sync changes

## Required future controls before execution

A future reconciliation execution phase must add all of the following before any stock adjustment is allowed:

1. persistent audit trail
2. explicit operator identity
3. reconciliation reason
4. exported drift report evidence
5. physical count confirmation
6. sync-safe validation
7. permission enforcement at the action boundary
8. separate review and execution steps

## Current behavior

The current implementation may evaluate whether the signed-in role can prepare a future reconciliation workflow. It must not write inventory, create inventory movements, change product stock, change supply stock, or persist an adjustment.

## Non-goals

- no execution of reconciliation
- no adjustment of Product.StockQuantity
- no adjustment of Supply.Stock
- no InventoryMovement creation
- no sync mutation
- no checkout mutation
- no database migration
