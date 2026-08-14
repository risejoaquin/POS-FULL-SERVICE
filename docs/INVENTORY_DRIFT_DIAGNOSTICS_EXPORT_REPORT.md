# Inventory Drift Diagnostics Export/Report

## Purpose

This document defines the baseline for exporting or copying the inventory drift diagnostics report from the POS inventory screen.

The feature is diagnostic only and report-only. It exists so support/admin users can copy or save the current diagnostic result for manual review.

## Safety boundaries

The export/report feature:

- diagnostic only.
- report-only.
- does not auto-correct stock.
- does not mutate Product stock.
- does not mutate Supply stock.
- does not persist inventory changes.
- no schema change.
- no migrations.
- no checkout changes.
- no sync changes.
- no ledger storage semantics change.
- no automatic reconciliation.

## Allowed actions

The UI may:

- copy the current diagnostics text to the clipboard.
- export the current diagnostics text to a `.txt` or `.md` file.
- include status, last run timestamp, current summary, and safe error state.
- log copy/export success or failure.

## Forbidden actions

The UI must not:

- correct operational stock.
- rewrite inventory movements.
- enqueue sync corrections.
- silently reconcile drift.
- create database migrations.
- write inventory state while exporting the report.

## UX rule

Every copy/export path must communicate that the report is informational only and does not correct inventory.

## Current implementation

The baseline uses:

- `CopyInventoryDriftDiagnosticsReportCommand`.
- `ExportInventoryDriftDiagnosticsReportCommand`.
- `InventoryDriftDiagnosticsFormatter.FormatExport(...)`.
- `InventoryDriftDiagnosticsLastExportPath`.
