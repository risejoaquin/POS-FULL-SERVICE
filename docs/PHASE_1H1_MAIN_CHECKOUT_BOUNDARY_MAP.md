# PHASE 1H.1 — MainViewModel Audit + Product Read Boundary

## Goal

Start the MainViewModel / Checkout cleanup with a minimal low-risk boundary extraction.

## What changed

- Product catalog loading in `MainViewModel` now uses `IInventoryAppService.GetAllProductsAsync()`.
- Direct product read queries for `LoadProductsAsync` were removed from the view model.
- Category/search filtering stays in the view model for now because it controls presentation state.

## What was intentionally not changed

The checkout transaction remains in `MainViewModel` for now and will be extracted later. It still includes:

- active shift validation
- product and recipe loading
- stock/supply decrement
- payment entity creation
- order state transitions
- cash movement creation
- retry logic for EF concurrency conflicts
- ticket printing and UI cleanup

## Boundary map for next steps

### PHASE 1H.2

Extract checkout request mapping and payment/cart DTO construction without moving the database transaction.

### PHASE 1H.3

Move the ACID checkout transaction into `ILocalOrderService.CheckoutAsync` / Infrastructure.

### PHASE 1H.4

Remove remaining `PosDbContext`, EF Core and direct infrastructure dependencies from `MainViewModel`.

## Scope exclusions

No changes were made to:

- Returns
- Inventory services beyond existing read service usage
- SyncService
- PosDomain
- PosServer
- migrations
- RLS
- licensing
- provisioning
- PosBuilder
