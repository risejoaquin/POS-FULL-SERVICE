# PHASE 3B — Inventory Mutation Guardrails

## Scope

This phase introduces conservative guardrails around inventory mutations while preserving the current database schema, checkout transaction, sync design, and ledger quantity semantics.

## Goals

- Replace direct local stock arithmetic with existing domain helpers.
- Prevent local product stock and supply stock from being reduced below zero before `SaveChangesAsync`.
- Preserve the existing checkout transaction boundary.
- Preserve concurrency retry behavior.
- Keep the current `InventoryMovement.Quantity` sign conventions for compatibility.

## Changed

### Local checkout

`LocalOrderService.ProcessCheckoutAsync` now uses:

- `Product.CanFulfill(...)`
- `Product.DecreaseStock(...)`
- `RecipeItem.RequiredFor(...)`
- `Supply.DecreaseStock(...)`

The service still writes inventory movements and commits everything inside the existing explicit transaction.

### Local inventory service

`InventoryService` now uses domain helpers for sale, return, restock, and recipe consumption stock mutation.

### Local inventory app service

`InventoryAppService.AdjustStockAsync` now routes product stock changes through a private guardrail method instead of directly modifying `StockQuantity`.

### Application order use case

`PosApplication.UseCases.Orders.OrderService.CheckoutAsync` now uses `Product.DecreaseStock(...)` instead of directly reducing stock.

### Concurrency resolution

Checkout concurrency resolution now rejects recalculated negative product or supply stock instead of blindly applying `dbStock - requestedQuantity`.

## Not changed

- No migrations changed.
- No EF mappings changed.
- No schema changed.
- No sync protocol changed.
- No `InventoryMovement.Quantity` sign normalization was done.
- No central server stock mutation refactor was done in this phase.
- No Money adoption was done.
- No checkout payment behavior was changed.

## Reason for not touching central server stock mutations

The central sync path can process orders created offline. Replacing central stock mutations with the same product-sale helper used by POS checkout may reject historical/offline sales if a product was later deactivated centrally. That should be handled in a dedicated sync conflict phase, not in this guardrail pass.

## Remaining risks

- `InventoryMovement.Quantity` still has mixed signed conventions between some flows.
- Central server order sync still mutates product and supply stock directly.
- Ledger/rebuild-from-movements behavior is not implemented yet.
- Product quantity is still integer-based while supply quantity is decimal-based.

## Validation

Expected after this phase:

- `dotnet test`: previous tests plus 2 updated/new architecture checks.
- `dotnet build -c Release Pos.sln`: 0 errors.
