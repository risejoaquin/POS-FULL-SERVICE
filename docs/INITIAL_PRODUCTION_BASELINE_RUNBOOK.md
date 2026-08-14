# InitialProductionBaseline Runbook

**InitialProductionBaseline runbook for MACROFASE 12C.**

## Preconditions

- MACROFASE 12A database audit is closed.
- MACROFASE 12B model hardening is closed.
- `dotnet test` passes.
- `dotnet build -c Release Pos.sln` passes.
- The target Supabase database has no important data.
- Railway Root Directory is empty or `/`.
- Railway Dockerfile path is controlled by `railway.json` or `RAILWAY_DOCKERFILE_PATH=PosServer/Dockerfile`.

## Step 1 — Dry run

```powershell
.\scripts\database\Invoke-Macrofase12C-MigrationResetAndBaseline.ps1
```

Expected dry-run markers:

```text
MACROFASE 12C dry run only.
No migration files were deleted.
No Supabase schema was dropped.
No InitialProductionBaseline migration was generated.
```

## Step 2 — Back up and remove old CentralDbContext migrations

```powershell
.\scripts\database\Invoke-Macrofase12C-MigrationResetAndBaseline.ps1 -ApplyMigrationReset
```

This backs up current files from:

```text
PosInfrastructure/Migrations
```

into:

```text
artifacts/database/migration-backups/Migrations_Backup_PreMacro12_<timestamp>
```

Then it removes the old CentralDbContext migration files so EF Core can generate a clean baseline.

## Step 3 — Generate InitialProductionBaseline

```powershell
.\scripts\database\Invoke-Macrofase12C-MigrationResetAndBaseline.ps1 -GenerateBaseline
```

Equivalent EF Core command:

```powershell
dotnet ef migrations add InitialProductionBaseline --context CentralDbContext --project PosInfrastructure --startup-project PosServer --output-dir Migrations
```



## dotnet ef availability mitigation

If Step 3 fails with:

```text
dotnet.exe : No se pudo ejecutar porque no se encontró el comando o archivo especificado.
```

then the .NET SDK is present, but the EF Core CLI tool is not available in the current shell. Run the baseline command with the local tool bootstrap option:

```powershell
.\scripts\database\Invoke-Macrofase12C-MigrationResetAndBaseline.ps1 -GenerateBaseline -InstallLocalDotnetEf
```

The script will create or reuse `.config/dotnet-tools.json`, install `dotnet-ef` as a local repository tool, restore it, and then generate `InitialProductionBaseline` through `dotnet tool run dotnet-ef`.

## Step 4 — Validate local build

```powershell
dotnet test
dotnet build -c Release Pos.sln
```

Expected after this block:

```text
640 tests passed
0 failed
Compilación correcta.
0 Advertencia(s)
0 Errores
```

## Step 5 — Reset Supabase public schema intentionally

Open Supabase SQL Editor and execute:

```sql
DROP SCHEMA IF EXISTS public CASCADE;
CREATE SCHEMA public;
GRANT USAGE ON SCHEMA public TO postgres, anon, authenticated, service_role;
GRANT ALL ON SCHEMA public TO postgres, service_role;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO postgres, service_role;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO postgres, service_role;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON FUNCTIONS TO postgres, service_role;
```

This is destructive. It is only approved because the current database was confirmed disposable.

## Step 6 — Commit and redeploy

```powershell
git add .
git commit -m "Reset EF Core migrations and create InitialProductionBaseline"
git push
```

Then **Redeploy** Railway from the latest commit.

## Step 7 — Expected Railway deploy result

```text
Starting Container
Applying migration 'InitialProductionBaseline'.
Now listening on: http://0.0.0.0:<PORT>
```

Then validate:

```powershell
curl https://<railway-url>/health
```

or:

```powershell
curl https://<railway-url>/api/health
```

## Rollback guidance

Because this database is disposable, rollback means recreating the schema again and rerunning the baseline. Do not partially restore old migrations after the baseline has been accepted.


## Design-time factory requirement

`InitialProductionBaseline` must be generated through the dedicated EF Core design-time factory:

```text
PosInfrastructure/Data/Server/CentralDbContextDesignTimeFactory.cs
```

This prevents `dotnet ef` from requiring `JWT_KEY`, `JWT_ISSUER` or `JWT_AUDIENCE` during migration generation and resolves the CentralDbContext constructor ambiguity.

After this hotfix, repeat baseline generation and then run `dotnet test` and `dotnet build -c Release Pos.sln`.
