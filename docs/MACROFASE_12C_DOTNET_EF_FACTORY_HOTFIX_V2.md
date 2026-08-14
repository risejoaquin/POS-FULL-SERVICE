# MACROFASE 12C - dotnet-ef and design-time factory hotfix V2

This hotfix closes two local verification blockers found after applying the design-time factory hotfix.

## Fixes

- Adds the exact architecture marker `without executing JWT startup validation` to `CentralDbContextDesignTimeFactory.cs`.
- Adds `.config/dotnet-tools.json` so `dotnet-ef` is treated as a repository-local tool.
- Updates `Invoke-Macrofase12C-MigrationResetAndBaseline.ps1` to restore local tools automatically when the manifest exists.
- Allows `-GenerateBaseline` to proceed without requiring the user to remember `-InstallLocalDotnetEf` in the common local-tool case.

## Guardrails

- No Supabase reset is executed automatically.
- No production data mutation is performed.
- No checkout behavior is changed.
- No inventory mutation is changed.
- No public API behavior is changed.
- No business logic is changed.
