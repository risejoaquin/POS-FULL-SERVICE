# PHASE 1H.3 — Checkout Transaction Extraction

## Scope

Moved the checkout transaction boundary out of `PosCore/ViewModels/MainViewModel.cs` and into `PosInfrastructure/Services/Local/LocalOrderService.cs` behind `PosApplication.Interfaces.Local.ILocalOrderService`.

## Moved out of MainViewModel

- Active shift persistence check through `ILocalOrderService.HasActiveShiftAsync`.
- Product + recipe loading for checkout.
- Stock and supply decrement.
- Inventory movement creation.
- Payment entity creation.
- Order creation and state transitions.
- Cash movement creation.
- EF transaction and `SaveChangesAsync` retry on concurrency conflict.
- `ChangeTracker.Clear` on rollback/error.

## Preserved in MainViewModel

- Cart UI state.
- Payment window UI interaction.
- CheckoutRequest construction.
- Loading indicator.
- Receipt printing after successful checkout.
- Notifications.
- Cart cleanup and product reload.

## Out of scope

- Full removal of `PosDbContext` from `MainViewModel`.
- Suspended orders redesign.
- Receipt printing extraction.
- Sync trigger extraction.
- Domain/Money redesign.
- Inventory/concurrency redesign.

## Validation

Run:

```powershell
dotnet test
dotnet build -c Release Pos.sln
```

Gate:

- Tests: 0 failed
- Build: 0 errors
