# MACROFASE 12C Updated Patch

Apply this patch over your current repository after `InitialProductionBaseline` has already been generated locally.

This patch is intentionally small so it does not overwrite your locally generated migration files.

Run:

```powershell
cd C:\Users\Lucilfer\Documents\POS

.\VERIFY_MACROFASE_12C_MIGRATION_BASELINE_UPDATED.ps1

dotnet test

dotnet build -c Release Pos.sln
```

Do not run `-ApplyMigrationReset` again.
Do not run `-GenerateBaseline` again unless you intentionally want to recreate the migration baseline.
