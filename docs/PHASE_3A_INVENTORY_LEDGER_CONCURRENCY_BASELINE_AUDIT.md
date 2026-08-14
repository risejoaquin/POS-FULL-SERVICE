# PHASE 3A — Inventory Ledger / Concurrency Baseline Audit

## Scope

This phase is an audit and safety baseline only. It does not change runtime behavior, database schema, migrations, checkout, returns, reports, sync, or services.

## Objective

Establish the current inventory mutation map before implementing a true inventory ledger or stronger concurrency controls.

## Current stock mutation points

### LocalOrderService.ProcessCheckoutAsync

Current checkout performs the most sensitive inventory operation:

- Opens an explicit EF transaction.
- Loads each sold product with recipe items and supplies.
- Checks product stock before mutation.
- Decrements `Product.StockQuantity` directly.
- Adds an `InventoryMovement` for product sale.
- Decrements recipe supply stock directly when recipe items exist.
- Adds an `InventoryMovement` for recipe consumption.
- Saves order, payments, cash movement and inventory mutations in one unit of work.
- Retries on `DbUpdateConcurrencyException` up to 3 times.

### InventoryService

`InventoryService` exposes lower-level local inventory methods:

- `RegisterSaleAsync` decrements product stock and recipe supply stock, then adds inventory movements.
- `RegisterReturnAsync` increments product stock and adds a return movement.
- `RegisterRestockAsync` increments product stock and adds a restock movement.

Current note: these methods do not call `SaveChangesAsync` themselves. They are intended to participate in a wider unit of work.

### InventoryAppService

`InventoryAppService.AdjustStockAsync` directly changes product stock and adds an adjustment movement, then saves immediately.

### PosApplication OrderService

`PosApplication.UseCases.Orders.OrderService.CheckoutAsync` still contains direct product stock decrement logic. This is a legacy/application-level mutation path and should be reviewed before any inventory ledger enforcement.

## Existing safety mechanisms

- Product stock has an EF concurrency marker through `[ConcurrencyCheck]` on `Product.StockQuantity`.
- Supply stock has an EF concurrency marker through `[ConcurrencyCheck]` on `Supply.Stock`.
- Local and central contexts include non-negative check constraints for product and supply stock.
- Checkout uses an explicit transaction.
- Checkout catches `DbUpdateConcurrencyException` and retries.
- Domain helpers exist for product/supply stock operations, but infrastructure services still mutate fields directly.

## Confirmed risks

### RISK-INV-001 — Direct field mutation bypasses domain helpers

Several services still mutate `StockQuantity` and `Stock` directly instead of using `Product.DecreaseStock`, `Product.IncreaseStock`, `Supply.DecreaseStock`, or `Supply.IncreaseStock`.

Impact: validations can diverge between domain and infrastructure.

### RISK-INV-002 — Ledger movement signs are inconsistent across flows

Some flows store sale quantities as negative values while the newer domain model expects positive quantities plus semantic movement type and exposes `SignedQuantity` as the calculated signed value.

Impact: reports or sync logic can double-negate or misinterpret stock movement direction.

### RISK-INV-003 — Recipe supply stock can go negative before database constraint catches it

Checkout and inventory sale flows decrement recipe supplies directly. Product stock is checked before sale, but supply availability is not consistently checked before recipe consumption.

Impact: a product may be sold even when a required supply is insufficient.

### RISK-INV-004 — Multiple mutation paths can cause duplicate stock effects

Checkout currently mutates stock directly. `InventoryService.RegisterSaleAsync` can also mutate stock. Any future orchestration that calls both for the same sale could double-decrement inventory.

Impact: accidental double discount during refactor or integration.

### RISK-INV-005 — Application-level checkout still exists outside local infrastructure transaction

`PosApplication.UseCases.Orders.OrderService.CheckoutAsync` mutates product stock through repositories and is not wrapped in a visible unit-of-work transaction.

Impact: partial order/product persistence if used in production without a transactional adapter.

## Static tests added

`PosInfrastructure.Tests/Architecture/InventoryLedgerConcurrencyBaselineTests.cs` captures the current baseline:

1. Checkout keeps inventory mutations inside an explicit transaction.
2. Checkout has a concurrency retry baseline.
3. Local and central contexts protect product/supply stock from negative values.
4. Current direct mutation hotspots are documented before behavioral refactor.

## Not changed

- No migrations.
- No EF mappings.
- No checkout behavior.
- No returns behavior.
- No sync behavior.
- No reports behavior.
- No decimal-to-Money migration.
- No inventory quantity type migration.
- No service refactor.

## Recommended next phase

Proceed with `PHASE 3B — Inventory Ledger Sign Normalization Plan` before changing runtime behavior.

The next phase should decide the canonical rule:

- Preferred: store `InventoryMovement.Quantity` as a positive magnitude and infer direction from `MovementType` through `SignedQuantity`.
- Migration requirement: existing negative sale records must be handled carefully before changing production logic.
