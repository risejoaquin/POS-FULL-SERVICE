# MACROFASE 12C — Idempotent Baseline Generation Hotfix

## Problem

`InitialProductionBaseline` was generated successfully, but rerunning the generation command attempted to create the same migration again and EF Core returned: `The name 'InitialProductionBaseline' is used by an existing migration.`

## Fix

The 12C script now checks whether an `InitialProductionBaseline` migration file already exists before calling `dotnet ef migrations add`. If it exists, the script skips generation and verifies the Release build instead.

## Design-time factory marker

The design-time factory does not require JWT_KEY, JWT_ISSUER, or JWT_AUDIENCE because it creates `CentralDbContext` without executing JWT startup validation.

## Expected behavior

- First run: generate `InitialProductionBaseline`.
- Later runs: detect existing baseline, skip generation, build Release, and exit successfully.

## Guardrails

- no Supabase reset executed automatically
- no schema drop from PowerShell
- no business logic change
- no public API behavior change
- no production data mutation
