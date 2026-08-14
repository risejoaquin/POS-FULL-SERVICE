# MACROFASE 12C — Current Status

Status: InitialProductionBaseline generated locally by the operator.

Important: do not run `-ApplyMigrationReset` again unless intentionally rebuilding the baseline from scratch.

Important: do not run `-GenerateBaseline` again when `InitialProductionBaseline` already exists. The script is idempotent and will now skip generation if the migration files are present.

Next required steps:

1. Keep the generated `InitialProductionBaseline` migration files under `PosInfrastructure/Migrations`.
2. Run `dotnet test`.
3. Run `dotnet build -c Release Pos.sln`.
4. Reset the disposable Supabase `public` schema using `scripts/database/Reset-Supabase-PublicSchema-Macrofase12C.sql`.
5. Commit and push the generated migration plus the MACROFASE 12C tooling.
6. Redeploy Railway.

Expected Railway result after Supabase reset:

```text
Starting Container
Applying migration 'InitialProductionBaseline'
Now listening on: http://0.0.0.0:<PORT>
```

Guardrails:

- no production data preservation required because the database was confirmed disposable
- no business logic change
- no POS checkout behavior change
- no inventory logic change
- no public API behavior change beyond database bootstrap stabilization
- no real payment execution
- no production sync enablement
