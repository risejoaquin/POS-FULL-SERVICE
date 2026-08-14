# MACROFASE 12C dotnet-ef and runbook hotfix

This hotfix closes the local validation blockers found after MACROFASE 12C tooling was introduced.

## Fixed issues

- The runbook now contains the exact `Redeploy` marker required by the architecture test.
- `Invoke-Macrofase12C-MigrationResetAndBaseline.ps1` now handles missing `dotnet ef` gracefully.
- The script can bootstrap `dotnet-ef` as a local repository tool using `-InstallLocalDotnetEf`.
- Existing reset state is tolerated when migration files are already removed.

## Recommended command when dotnet ef is missing

```powershell
.\scripts\database\Invoke-Macrofase12C-MigrationResetAndBaseline.ps1 -GenerateBaseline -InstallLocalDotnetEf
```

## Guardrails

- No Supabase schema reset is executed automatically.
- No business logic is changed.
- No POS checkout behavior is changed.
- No inventory mutation behavior is changed.
- No public API contract is changed.
