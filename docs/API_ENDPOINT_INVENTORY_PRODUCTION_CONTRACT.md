# MACROFASE 13B - API Endpoint Inventory and Contract Validation

Status: PENDING LOCAL AND PRODUCTION VALIDATION
Scope: inventory and contract validation of the current PosServer API surface.
Execution mode: read-only, GET-only for production probes unless explicitly extended later.

## Baseline source

This inventory is based on the deployed PosServer routing shape validated during MACROFASE 13A and on the current source structure expected under:

- `PosServer/Program.cs`
- `PosServer/Controllers/AuthController.cs`
- `PosServer/Controllers/HealthController.cs`
- `PosServer/Controllers/LicenseController.cs`
- `PosServer/Controllers/ProductsController.cs`
- `PosServer/Controllers/OrdersController.cs`
- `PosServer/Controllers/InventoryMovementsController.cs`
- `PosServer/Controllers/ShiftsController.cs`
- `PosServer/Controllers/SyncController.cs`
- `PosServer/Controllers/UsersController.cs`

MACROFASE 13B does not modify checkout behavior, sales, inventory, users, tenants, sync payloads, licenses, or database rows.

## Public runtime endpoints

| Method | Route | Source | Expected unauthenticated status | Contract |
|---|---|---|---:|---|
| GET | `/` | Program.cs minimal endpoint | 200 | Service identity, runtime status, environment, timestamp. |
| GET | `/health` | Program.cs minimal endpoint | 200 | Lightweight service health. |
| GET | `/api/health` | Program.cs minimal endpoint | 200 | API health alias for external checks. |
| GET | `/health/live` | HealthController | 200 | Liveness only. No database dependency required. |
| GET | `/health/ready` | HealthController | 200 or 503 | Readiness with database connectivity. Production baseline expects `database = Connected`. |

## Operational endpoints requiring review

| Method | Route | Source | Current exposure | Risk classification | Required follow-up |
|---|---|---|---|---|---|
| GET | `/health/metrics` | HealthController | Public unless protected elsewhere | P2 | Review before exposing outside operators. It reports operational counters. |
| GET | `/metrics` | HealthController absolute route | Public unless protected elsewhere | P2 | Same as `/health/metrics`. Consider admin auth or Railway/private monitoring only. |
| GET | `/swagger` | Program.cs Swagger middleware | Public if Swagger is enabled in production | P2 | In 13C/13D decide whether Swagger remains public, disabled, or protected. |
| GET | `/releases/*` | Program.cs static files | Public by design for Squirrel releases | P2 | Validate release artifact integrity, no secret files, and intentional exposure. |

## Auth endpoints

| Method | Route | Source | Auth | Rate limit | Contract |
|---|---|---|---|---|---|
| POST | `/api/v1/auth/login` | AuthController | Public | LoginPolicy | Authenticates user and returns JWT, refresh token, tenant id and user information. |
| POST | `/api/v1/auth/refresh` | AuthController | Public | LoginPolicy | Refreshes JWT and refresh token. |
| POST | `/api/v1/auth/provision` | AuthController | Public code path | LoginPolicy | Provisions tenant/license. Must remain gated by server-side provision protection inside service/config. |

## License endpoints

| Method | Route | Source | Auth | Contract |
|---|---|---|---|---|
| POST | `/api/v1/license/validate` | LicenseController | AllowAnonymous | Validates a license key and returns validity plus tenant/license metadata. |
| POST | `/api/v1/license/generate` | LicenseController | Admin role | Generates a license. Must never be public. |

## Product endpoints

| Method | Route | Source | Auth | Tenant required | Mutates data | Contract |
|---|---|---|---|---|---|---|
| GET | `/api/v1/products` | ProductsController | Authenticated | Yes | No | Returns paged product data. |
| GET | `/api/v1/products/changes?since=...` | ProductsController | Authenticated | Yes | No | Returns changed products for sync. |
| POST | `/api/v1/products` | ProductsController | Admin role | Yes | Yes | Creates or updates product. |
| DELETE | `/api/v1/products/{barcode}` | ProductsController | Admin role | Yes | Yes | Deletes product by barcode. |

## Order endpoints

| Method | Route | Source | Auth | Tenant required | Mutates data | Contract |
|---|---|---|---|---|---|---|
| POST | `/api/v1/orders` | OrdersController | Authenticated | Yes | Yes | Creates or updates order. |
| GET | `/api/v1/orders` | OrdersController | Authenticated | Yes | No | Returns paged orders. |
| GET | `/api/v1/orders/{id}` | OrdersController | Authenticated | Yes | No | Returns one order or 404. |

## Inventory movement endpoints

| Method | Route | Source | Auth | Tenant required | Mutates data | Contract |
|---|---|---|---|---|---|---|
| POST | `/api/v1/inventorymovements` | InventoryMovementsController | Authenticated | Yes | Yes | Syncs inventory movement. |

## Shift endpoints

| Method | Route | Source | Auth | Tenant required | Mutates data | Contract |
|---|---|---|---|---|---|---|
| POST | `/api/v1/shifts` | ShiftsController | Authenticated | Yes | Yes | Syncs cash register shift. |

## Sync endpoints

| Method | Route | Source | Auth | Tenant required | Mutates data | Contract |
|---|---|---|---|---|---|---|
| GET | `/api/v1/sync/changes?since=...` | SyncController | Authenticated | Yes | No | Returns server-side changes for tenant. |
| POST | `/api/v1/sync/apply` | SyncController | Authenticated | Yes | Yes | Applies sync payload. |
| POST | `/api/v1/sync/ping` | SyncController | Authenticated | Yes | Minimal/logging | Receives heartbeat payload. |

## User endpoints

| Method | Route | Source | Auth | Tenant required | Mutates data | Contract |
|---|---|---|---|---|---|---|
| POST | `/api/v1/users` | UsersController | Admin role | Yes | Yes | Creates or updates user. |
| DELETE | `/api/v1/users/{username}` | UsersController | Admin role | Yes | Yes | Deletes user. |

## MACROFASE 13B acceptance criteria

- Endpoint inventory is documented.
- Public runtime endpoints stay 200 in production.
- `/health/ready` reports database connected in production.
- Protected GET endpoints must not return 200 without authentication.
- No production validation script sends POST, PUT, PATCH, or DELETE.
- No production validation script mutates checkout, orders, products, inventory, users, tenants, sync, or licenses.
- Risk register is created before authenticated business endpoint testing.

Next phase: MACROFASE 13C - Authenticated Business Endpoint Contract Validation.
