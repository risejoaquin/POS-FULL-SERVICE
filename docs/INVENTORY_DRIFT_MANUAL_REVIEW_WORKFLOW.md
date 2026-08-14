# Inventory Drift Manual Review Workflow

## Purpose

This document defines the Phase 3K manual review workflow baseline for inventory drift diagnostics.

The workflow is diagnostic only and manual review only. It helps an operator identify that drift exists, export the diagnostic report, and prepare a controlled human review. It does not apply inventory corrections.

## Scope

Phase 3K adds a UI/internal workflow marker for manual review:

- Detect that drift exists from the current diagnostics state.
- Mark manual review as required when drift is present.
- Show clear operator instructions.
- Allow support/admin users to export the existing diagnostic report before any future correction process.

## Safety boundaries

This baseline is intentionally conservative:

- read-only
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

## Workflow states

The manual review workflow exposes the following user-facing states:

- `Revisión manual no iniciada`
- `Revisión manual no requerida`
- `Revisión manual requerida`
- `Revisión manual bloqueada por error de diagnóstico`
- `Revisión manual no disponible`
- `Revisión manual en preparación`

## Operator workflow

1. Execute the inventory drift diagnostic.
2. If drift exists, review the diagnostic summary.
3. Export or copy the drift report.
4. Compare the report against physical inventory and operational records.
5. Document the discrepancy for a future authorized reconciliation process.
6. Do not adjust stock from this workflow.

## Non-goals

Phase 3K does not introduce:

- automatic reconciliation
- manual stock correction
- approval workflow persistence
- audit table persistence
- sync event publishing
- checkout or return behavior changes
- inventory ledger mutation

## Future phase

A future phase may add a controlled reconciliation workflow with explicit permissions, audit logging, and approval boundaries. That future phase must remain separate from this manual review baseline.
