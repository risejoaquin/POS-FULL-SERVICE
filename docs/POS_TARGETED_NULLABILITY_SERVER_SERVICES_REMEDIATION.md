# POS Targeted Nullability Remediation: Server Services

## Scope

PHASE 7C applies the first targeted nullable warning remediation slice after the PHASE 7B baseline. The scope is limited to server-side service hotspots already classified in the baseline:

- `PosInfrastructure/Services/Server/AuthService.cs`
- `PosInfrastructure/Services/Server/UserService.cs`
- `PosInfrastructure/Data/Server/CentralDbContext.cs`

## Remediation checks

- targeted nullability remediation documented
- AuthService nullable password hash guard implemented
- AuthService token claim null guard implemented
- AuthService provision request null guard implemented
- AuthService admin credential null guard implemented
- AuthService employee credential null guard implemented
- UserService nullable payload contract implemented
- UserService username comparison null guard implemented
- UserService delete username null guard implemented
- CentralDbContext DbSet null-forgiving initialization implemented
- CentralDbContext audit entity id string conversion guard implemented
- CentralDbContext outbox tenant id string conversion guard implemented
- server services only remediation scope documented
- no checkout behavior change
- no inventory mutation
- no production sync enablement
- no schema change
- no migrations
- operator-safe targeted nullability remediation message documented

## Applied remediation

### AuthService

- Login now validates that `PasswordHash` is present before bcrypt verification.
- Refresh validates required token claims before querying by username and tenant.
- Provision validates the request, tenant id, admin credentials and optional employee credentials before string operations.
- Expired-token principal extraction now rejects blank tokens before validation.

### UserService

- `CreateOrUpdateUserAsync` now has a nullable payload contract and returns a nullable user on invalid payload.
- Username comparisons are guarded before lowercase operations.
- Delete user rejects blank usernames before query construction.

### CentralDbContext

- DbSet properties without initialization use null-forgiving initialization.
- Audit entity id conversion falls back to an empty string if `ToString()` returns null.
- Outbox tenant id conversion falls back to an empty string if `ToString()` returns null.

## Safety boundaries

- No checkout behavior change
- No inventory mutation
- No production sync enablement
- No queue processing change
- No checkpoint behavior change
- No schema change
- No migrations

## Operator-safe message

Targeted server service nullability remediation prepared. This phase only adds defensive null guards and safe string conversions in server service hotspots. It does not change checkout, inventory, production sync, schema or migrations.
