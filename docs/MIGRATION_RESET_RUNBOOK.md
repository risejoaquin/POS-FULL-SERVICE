# Migration Reset Runbook — MACROFASE 12C Preview

This runbook prepares the next block after MACROFASE 12B.

## Local migration reset

Use `scripts/database/Invoke-Macrofase12-ProductionDatabaseBaseline.ps1` from the repository root.

Recommended command:

```powershell
.\scripts\database\Invoke-Macrofase12-ProductionDatabaseBaseline.ps1 -ApplyLocalMigrationReset
```

Expected generated migration:

```text
InitialProductionBaseline
```

## Supabase reset

Because the current Supabase database has no important data, reset the schema intentionally from Supabase SQL Editor using:

```sql
DROP SCHEMA public CASCADE;
CREATE SCHEMA public;
```

The full reset script is available at:

```text
scripts/database/Reset-Supabase-PublicSchema.sql
```

## Railway

After the baseline migration is generated and pushed:

- Root Directory: `/` or empty
- Dockerfile Path: handled by `railway.json` as `PosServer/Dockerfile`
- Required variables include `JWT_KEY`, `JWT_ISSUER`, `JWT_AUDIENCE`, `ConnectionStrings__DefaultConnection`, and `ASPNETCORE_ENVIRONMENT=Production`.
