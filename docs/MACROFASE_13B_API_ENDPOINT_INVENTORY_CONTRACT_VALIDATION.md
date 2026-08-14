# MACROFASE 13B - API Endpoint Inventory and Contract Validation

Status: PENDING VERIFICATION
Previous baseline: MACROFASE 13A closed with public runtime endpoints responding in production.

## Purpose

MACROFASE 13B freezes the API endpoint inventory before deeper business validation. The goal is to know exactly what exists, what is public, what requires JWT, what requires Admin role, what is read-only, and what mutates data.

## Quality gates

1. Static inventory exists for every controller endpoint.
2. Public runtime endpoints are revalidated in production.
3. Protected GET endpoints are probed without token and must not return 200.
4. Write endpoints are listed but not executed.
5. Risk register is created before authenticated business tests.
6. The validation scripts must remain GET-only in production.

## Files added

- `docs/API_ENDPOINT_INVENTORY_PRODUCTION_CONTRACT.md`
- `docs/API_CONTRACT_RISK_REGISTER_MACROFASE_13B.md`
- `docs/MACROFASE_13B_API_ENDPOINT_INVENTORY_CONTRACT_VALIDATION.md`
- `docs/PROJECT_PROGRESS_REPORT_MACROFASE_13B.md`
- `scripts/production/Export-Macrofase13B-EndpointInventory.ps1`
- `scripts/production/Validate-Macrofase13B-ApiEndpointInventoryContract.ps1`
- `VERIFY_MACROFASE_13B_API_ENDPOINT_INVENTORY_CONTRACT_VALIDATION.ps1`

## Production validation routes

The default production script validates only safe GET routes:

- `/`
- `/health`
- `/api/health`
- `/health/live`
- `/health/ready`

With `-IncludeProtectedReadProbes`, it also sends unauthenticated GET requests to selected protected read routes and passes only if they do not return 200.

## Acceptance

MACROFASE 13B is closed when:

- verifier passes;
- `dotnet test` remains 643 passed, 0 failed;
- `dotnet build -c Release Pos.sln` remains 0 warnings, 0 errors;
- production validation passes;
- endpoint inventory and risk register are committed.

Next: MACROFASE 13C - Authenticated Business Endpoint Contract Validation.
