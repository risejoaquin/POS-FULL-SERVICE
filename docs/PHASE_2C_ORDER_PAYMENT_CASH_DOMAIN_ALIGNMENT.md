# PHASE 2C — Order / Payment / Cash Domain Alignment

## Scope

Conservative domain-only alignment of `Order`, `Payment`, and `CashMovement`.

## Goals

- Add small domain helpers for payment totals, cash/card separation, balance due and full-payment state.
- Add payment state helpers and safe transition to refunded.
- Add cash movement factories and reason validation.
- Add tests for the new invariants.

## Out of scope

No EF mappings, migrations, checkout transaction, returns transaction, reports, sync, PosServer, PosBuilder, RLS, licensing, or provisioning were modified.

## Files modified

- `PosDomain/Entities/Order.cs`
- `PosDomain/Entities/Payment.cs`
- `PosDomain/Entities/CashMovement.cs`
- `PosDomain.Tests/Entities/OrderTests.cs`
- `PosDomain.Tests/Entities/PaymentTests.cs`
- `PosDomain.Tests/Entities/CashMovementTests.cs`

## Validation

Run:

```powershell
dotnet test
dotnet build -c Release Pos.sln
```

Acceptance:

- 0 failed tests
- 0 build errors
