# MACROFASE 12B — Model Hardening

MACROFASE 12B production database baseline hardening documented.

## Purpose

This block prepares `CentralDbContext` before the project generates `InitialProductionBaseline`.
The current Railway deployment reached EF Core migrations successfully, but Supabase contained an inconsistent partial schema. Because the database is disposable, the correct path is to harden the model first, then reset the schema and generate a clean baseline migration.

## Implemented source hardening

- `ApplyProductionDatabaseBaselineHardening(modelBuilder)` centralizes production persistence rules.
- Tenant-scoped entities now receive required `TenantId` model configuration through `ConfigureTenantScopedEntity`.
- Monetary values use explicit `HasPrecision(18, 2)`.
- Stock and recipe quantities use explicit `HasPrecision(18, 3)`.
- Supply cost uses explicit `HasPrecision(18, 4)`.
- Operational indexes were added for tenant, date, status, sync, barcode, idempotency and outbox/inbox lookup paths.
- Delete behavior was made explicit for critical relationships.
- Product barcode uniqueness is filtered so empty barcodes do not block multiple products in the same tenant.
- InventoryMovement FK enforcement is intentionally deferred because the current domain allows supply movements with `ProductId = 0`.

## Baseline direction

The next block is MACROFASE 12C — Migration Reset and InitialProductionBaseline generation.
Do not apply the old `InitialServer` migration to Supabase after this point.

## Guardrails

- no Supabase mutation from this package
- no destructive SQL auto-execution
- no production data deletion by script default
- no checkout behavior change
- no inventory mutation
- no public API behavior change
- no Railway variable mutation
