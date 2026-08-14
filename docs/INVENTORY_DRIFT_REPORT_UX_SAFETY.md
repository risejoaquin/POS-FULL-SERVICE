# Inventory Drift Report UX Safety

## Purpose

This document defines the UX safety rules for the inventory drift diagnostic hook.

The diagnostic is internal, read-only, and intentionally does not auto-correct stock.

## Required UX states

The UI must distinguish at least these states:

- Not executed.
- Calculating diagnostic.
- Sin drift detectado.
- Con drift detected and manual review required.
- Error al calcular diagnóstico.

## Safety copy

The UI must explicitly state that the diagnostic is solo lectura / read-only and no corrige inventario.

The report must not be presented as an automatic repair tool.

## Integration limits

This is a no schema change baseline.

There are no migrations.

There are no checkout changes.

There are no sync changes.

There is no SaveChanges call, no stock mutation, and no automatic correction.

## Future phase

A future phase may add a supervised reconciliation workflow, but this phase only improves diagnostic clarity and UX safety.
