# MACROFASE 12C — Migration Reset and InitialProductionBaseline

**MACROFASE 12C migration reset and InitialProductionBaseline documented.**

## Purpose

This block converts the database layer from a drifted deployment state into a clean production database baseline.

The Railway deploy reached `Database.Migrate()` successfully, but PostgreSQL failed with:

```text
42P07: relation "CashRegisterShifts" already exists
```

That means the infrastructure, Docker build, Railway runtime, JWT configuration and PostgreSQL connection are already working. The remaining blocker is schema drift: existing database objects do not match EF Core migration history.

## Decision

Because the current Supabase database is disposable, the correct action is not to repair individual tables. The correct action is to reset the `public` schema and generate a new baseline migration named:

```text
InitialProductionBaseline
```

## Scope

This block provides the controlled migration reset tooling. It does not silently delete data and it does not contact Supabase automatically unless the operator intentionally executes the reset script or SQL.

## Actions implemented

- Central migration reset runbook.
- Supabase public schema reset SQL.
- PowerShell script to back up old CentralDbContext migrations.
- PowerShell script path to remove old server migrations.
- PowerShell command path to generate `InitialProductionBaseline`.
- Post-generation validation checklist.
- Railway redeploy checklist.

## Non-goals

- No POS checkout behavior changes.
- No inventory mutation behavior changes.
- No API public contract changes.
- No production data preservation workflow because this environment was confirmed disposable.
- No automatic Supabase destructive execution from this package.

## Acceptance criteria

```text
MACROFASE 12C migration baseline reset tooling verified.
InitialProductionBaseline generation path documented.
Supabase public schema reset SQL documented.
Railway redeploy validation path documented.
```
