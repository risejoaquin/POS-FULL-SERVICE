# Database Baseline Model Hardening Report

DATABASE BASELINE MODEL HARDENING REPORT for MACROFASE 12B.

## CentralDbContext changes

`CentralDbContext` now includes `ApplyProductionDatabaseBaselineHardening(modelBuilder)`.

The hardening covers the following tables/entities:

- Products
- Orders
- OrderItems
- Payments
- Users
- Licenses
- CashRegisterShifts
- CashMovements
- Supplies
- InventoryMovements
- RecipeItems
- ProductModifiers
- ModifierOptions
- ProductModifierLinks
- OutboxMessages
- InboxMessages
- AuditLogs

## Required model standards

- Tenant-scoped tables must have required `TenantId`.
- Money fields must have explicit decimal precision.
- Inventory quantity fields must have explicit decimal precision.
- Sync/outbox/inbox tables must have operational indexes.
- Relationship delete behavior must be explicit.
- The baseline migration name must be `InitialProductionBaseline`.

## Known intentional deferral

`InventoryMovement` has an intentional FK deferral. The domain currently models product movements and supply movements in one table while keeping `ProductId` as a non-nullable integer. Supply movements use `ProductId = 0`; therefore adding a strict FK on `ProductId` would break valid current behavior. This should be normalized in a later macrofase by making product and supply references nullable with a check constraint.
