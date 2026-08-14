# MACROFASE 13 - API Production Validation

Status: PENDING LOCAL AND PRODUCTION VERIFICATION

## Objective

Validate the deployed POS-FULL-SERVICE API in Railway after the production database baseline was closed.

This macrofase verifies that the public runtime surface is alive, stable, and safe to use as the base for later business endpoint validation.

## Current production base URL

Expected Railway URL:

```text
https://pos-full-service-production.up.railway.app
```

## Validated baseline from MACROFASE 12

MACROFASE 12 closed with the following confirmed state:

- Railway build completed.
- Railway runtime port binding was fixed.
- PosServer reaches app.Run().
- Supabase connection works.
- EF Core migrations report database is up to date.
- Root and health endpoints respond with HTTP 200.
- /health/ready confirms database connectivity.

## API endpoints in scope for MACROFASE 13A

Only non-mutating runtime endpoints are in scope:

```text
/
/health
/api/health
/health/live
/health/ready
```

## API endpoints out of scope

The following are intentionally out of scope for this macrofase:

- Checkout operations.
- Payments.
- Inventory mutations.
- Sales creation.
- Returns.
- Sync mutations.
- Tenant data writes.
- Admin writes.
- Production data seeding.

## Safety rules

MACROFASE 13 must not perform production data mutation.

The validation scripts only execute HTTP GET requests against public health/runtime endpoints.

## Closure criteria

MACROFASE 13A can be closed when:

- The verifier script passes.
- dotnet test passes with the expected suite count.
- dotnet build -c Release Pos.sln succeeds with zero errors.
- Production validation script confirms all in-scope endpoints return HTTP 200.
- /health/ready returns database Connected.

## Next macrofase after 13A

After closing 13A, continue with:

```text
MACROFASE 13B - Authenticated API Contract Validation
```

13B should validate auth, JWT, tenant middleware and RBAC behavior without mutating production business data.
