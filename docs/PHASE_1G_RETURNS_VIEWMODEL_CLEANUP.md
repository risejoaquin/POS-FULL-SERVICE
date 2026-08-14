# PHASE 1G — ReturnsViewModel Cleanup

## Scope

Moved return lookup and execution logic out of `PosCore/ViewModels/ReturnsViewModel.cs` and behind `IReturnsService`.

## Files changed

- `PosCore/ViewModels/ReturnsViewModel.cs`
- `PosApplication/Interfaces/Local/IReturnsService.cs`
- `PosApplication/DTOs/Local/ReturnItemRequest.cs`
- `PosInfrastructure/Services/Local/ReturnsService.cs`
- `PosCore/Extensions/ServiceCollectionExtensions.cs`

## Architecture result

`ReturnsViewModel` now orchestrates UI concerns only:

- order search trigger
- confirmation dialogs
- partial-return item selection window
- manager override window
- reason window
- success/error messages
- ticket reprint

`ReturnsService` owns database and transactional behavior:

- order lookup with items/products
- active shift validation
- full return transaction
- partial return transaction
- order status transition
- inventory return registration
- cash movement registration for cash refunds
- outbox message creation

## Deferred

No checkout, inventory service internals, sync, domain model, server, migrations, RLS, licensing, or provisioning changes were made.
