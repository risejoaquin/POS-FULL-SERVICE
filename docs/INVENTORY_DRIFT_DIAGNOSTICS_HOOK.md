# Inventory Drift Diagnostics Hook

## Purpose

This document describes the internal UI/diagnostics hook added for inventory drift reporting.

The hook allows the POS inventory screen to request a read-only drift report through `IInventoryDriftReportingService` and display a summarized diagnostic message.

## Scope

This is a diagnostic hook only. It does not auto-correct stock, does not call SaveChanges, and does not mutate `Product.StockQuantity` or `Supply.Stock`.

This phase is intentionally a no schema change baseline. It includes no migrations, no checkout changes, no sync changes, no report recalculation, and no ledger rebuild.

## UI Entry Point

`InventoryWindow.xaml` exposes an internal button bound to:

```text
ShowInventoryDriftDiagnosticsCommand
```

The command is implemented in `InventoryViewModel` and calls:

```text
IInventoryDriftReportingService.GetCombinedDriftReportAsync
```

The result is formatted by:

```text
InventoryDriftDiagnosticsFormatter.Format
```

## Safety Rules

- diagnostic only
- does not auto-correct
- no stock mutation
- no SaveChanges
- no schema change
- no migrations
- no checkout changes
- no sync changes

## Future Work

A future phase may expose a richer diagnostics screen, export drift reports, or add administrator-only repair workflows. Those workflows must be explicit and audited; they must not be introduced through this hook.
