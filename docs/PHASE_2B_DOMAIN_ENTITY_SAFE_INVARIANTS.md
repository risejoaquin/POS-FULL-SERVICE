# PHASE 2B — Domain Entity Audit + Safe Invariants

## Scope

This iteration adds safe domain helper methods and tests without changing EF mappings, migrations, database columns, application services, checkout, returns, reports, sync, PosServer, or PosBuilder.

## Entities touched

- `Order`
- `Product`
- `Payment`
- `CashMovement`
- `CashRegisterShift`

## Strategy

The implementation preserves current public setters and persistence shape. New invariants were added as explicit domain methods and computed properties so existing EF materialization and application flows are not broken.

## Added invariants

### Product

- Low-stock computed status.
- Fulfillment validation.
- Stock increase/decrease helpers.
- Price update validation.

### CashMovement

- Centralized `Entrada` / `Salida` constants.
- Signed amount helper.
- Validation for shift, amount and movement type.

### CashRegisterShift

- Open/closed computed state.
- Safe close helper.
- Safe movement registration helper.

### Payment

- Cash/card computed helpers.
- Payment validation helper.

### Order

- Payment registration helper.
- Refund marking helper.
- Computed status helpers.

## Explicitly not changed

- No migration changes.
- No EF mapping changes.
- No decimal-to-Money entity migration yet.
- No checkout behavior change.
- No returns behavior change.
- No sync behavior change.
- No PosServer contract change.

## Validation

Run:

```powershell
dotnet test
dotnet build -c Release Pos.sln
```

Gate:

- Tests: 0 failed.
- Build: 0 errors.


## Expected test count

The previous validated baseline had 38 passing tests. This iteration adds 21 domain tests, so the expected total is approximately 59 passing tests if no unrelated tests were added or removed.
