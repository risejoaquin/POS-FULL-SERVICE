# PHASE 1H.2 — Checkout Request/Payment Boundary Extraction

## Scope

This iteration prepares checkout extraction without moving the checkout transaction out of `MainViewModel` yet.

## Completed

- Added clean checkout request DTOs in `PosApplication/DTOs/Local`:
  - `CheckoutRequest`
  - `CheckoutLineRequest`
  - `CheckoutPaymentRequest`
  - updated `CheckoutResult`
- `MainViewModel` now builds a `CheckoutRequest` from the current cart and the `PaymentWindow` result.
- Payment application/change calculation was isolated in request construction.
- PaymentDetails formatting was isolated in a dedicated helper.

## Preserved behavior

- Active shift validation remains unchanged.
- Stock validation remains unchanged.
- Stock/supply decrement remains unchanged.
- Inventory movements remain unchanged.
- Order creation and state transitions remain unchanged.
- Cash movement remains unchanged.
- SaveChanges/transaction/retry logic remains unchanged.
- Ticket printing remains unchanged.

## Deferred to PHASE 1H.3+

- Move checkout transaction into `ILocalOrderService` implementation.
- Move stock and supply decrement out of `MainViewModel`.
- Move cash movement creation out of `MainViewModel`.
- Move concurrency retry and transaction handling out of `MainViewModel`.
- Remove EF/DbContext from `MainViewModel` completely.
