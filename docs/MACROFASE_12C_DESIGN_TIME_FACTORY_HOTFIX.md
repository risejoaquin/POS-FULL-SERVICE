# MACROFASE 12C — Design-Time Factory Hotfix

This hotfix addresses the `dotnet ef migrations add InitialProductionBaseline` failure observed during MACROFASE 12C.

## Observed failure

`dotnet ef` successfully installed and the project built, but migration generation failed with two design-time issues:

```text
Missing JWT_KEY environment variable.
Multiple constructors accepting all given argument types have been found in type 'PosInfrastructure.Data.Server.CentralDbContext'.
```

## Cause

`dotnet ef` attempted to discover `CentralDbContext` through the application startup path. That path executes `PosServer/Program.cs`, which intentionally validates production JWT variables. EF tooling then tried to instantiate `CentralDbContext`, but the context exposes both a runtime constructor and a design-tool constructor, producing constructor ambiguity.

## Fix

A dedicated design-time factory was added:

```text
PosInfrastructure/Data/Server/CentralDbContextDesignTimeFactory.cs
```

The factory implements `IDesignTimeDbContextFactory<CentralDbContext>` and creates the context without executing JWT startup validation. It also selects the constructor explicitly by passing a design-time `ITenantContext`.

## Guardrails

- No POS checkout behavior changes.
- No inventory mutation behavior changes.
- No API public contract changes.
- No Supabase schema reset is executed automatically.
- No production database write is executed by this hotfix.

## Next command

Run baseline generation again from the repository root:

```powershell
.\scripts\database\Invoke-Macrofase12C-MigrationResetAndBaseline.ps1 -GenerateBaseline
```

If the local tool manifest is missing, use:

```powershell
.\scripts\database\Invoke-Macrofase12C-MigrationResetAndBaseline.ps1 -GenerateBaseline -InstallLocalDotnetEf
```

## V2 test marker

This design-time factory does not require JWT_KEY, JWT_ISSUER, or JWT_AUDIENCE because it bypasses runtime startup validation.
