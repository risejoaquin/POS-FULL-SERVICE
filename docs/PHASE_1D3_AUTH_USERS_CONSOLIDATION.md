# PHASE 1D.3 — Auth/Users Consolidation Safety Pass

## Scope

Small safety pass after PHASE 1D.1 and PHASE 1D.2. No business rules changed.

## Changes

- `UsersViewModel.IsCurrentUserAdmin()` now uses null-safe `string.Equals(...)` comparisons.
- Removed duplicated `using` directives from `PosCore/App.xaml.cs`.
- Removed duplicated `using Serilog;` from `PosCore/Services/SessionManager.cs`.

## Not changed

- No changes to authentication behavior.
- No changes to manager override behavior.
- No removal of the legacy `admin` PIN override. This remains tracked as security debt.
- No changes to Domain, PosServer, Checkout, Returns, Inventory, Sync, migrations, licensing, or provisioning.

## Expected validation

```powershell
dotnet test
dotnet build -c Release Pos.sln
```

Gate: 0 failed tests and 0 build errors.
