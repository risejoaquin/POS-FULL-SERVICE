# PHASE 1F.2 — Inventory Write Operations Cleanup

## Scope

This iteration moves the primary product write operations out of `InventoryViewModel` and into `IInventoryAppService` / `InventoryAppService`.

## Moved operations

- Create product
- Update product
- Delete product
- Import products from parsed CSV rows

## Still intentionally deferred

The following legacy windows remain out of scope for this iteration and still receive `PosDbContext` from `InventoryViewModel`:

- `SuppliesWindow`
- `ProductRecipeWindow`
- `ProductModifiersConfigWindow`

They should be handled in a later subphase because they involve supplies, recipes and modifier UI flows.

## Rules preserved

- No checkout changes
- No returns changes
- No sync changes
- No domain changes
- No server changes
- No migrations
- No product model redesign
