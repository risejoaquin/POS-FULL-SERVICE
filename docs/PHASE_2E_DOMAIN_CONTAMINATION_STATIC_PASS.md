# PHASE 2E — Domain Contamination Static Pass

## Scope

This phase performs a conservative static architecture pass over `PosDomain`.

## Changes

- Removed the empty placeholder file `PosDomain/Class1.cs`.
- Added architecture tests validating that `PosDomain` does not reference or expose types from:
  - `PosInfrastructure`
  - `PosCore`
  - `PosServer`
  - `Microsoft.EntityFrameworkCore`
  - WPF assemblies / namespaces
  - ASP.NET Core namespaces

## Explicitly not changed

The following were intentionally not changed in this phase:

- EF mappings
- migrations
- DataAnnotations attributes currently used for timestamps/concurrency compatibility
- API transport DTOs that still live in `PosDomain`
- decimal monetary columns
- checkout
- returns
- reports
- sync
- server contracts
- builder/provisioning

## Findings / debt

The static pass confirms that `PosDomain` has no direct assembly dependency on UI, infrastructure or server projects.

Remaining domain cleanup debt:

- Some EF-oriented attributes still exist in domain entities, especially concurrency/timestamp annotations. Removing them should be done only with explicit EF mapping replacement.
- Some API/request DTO-style classes still live under `PosDomain/Entities` and should be moved later in a dedicated contract cleanup phase.
- Monetary entity fields still use `decimal` for compatibility. Migration to minor units / `Money` should be phased separately.

## Validation

Required local gate:

```powershell
dotnet test
dotnet build -c Release Pos.sln
```

Expected result:

- Tests: 0 failed
- Build: 0 errors
