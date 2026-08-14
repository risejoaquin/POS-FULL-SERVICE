# API Contract Risk Register - MACROFASE 13B

Status: PENDING VALIDATION
Mode: static inventory plus safe production probes.

## Findings

### F13B-001 - Public metrics exposure needs decision

Routes: `/health/metrics`, `/metrics`
Severity: P2
Status: OPEN FOR 13C/13D

The metrics endpoints query operational counters such as pending sync messages, failed sync messages, order counts, conflicts, login failures, and active terminals. This can be valuable for operations, but it should not be left public without an explicit decision.

Required decision:

- Keep public only if the deployment is private/internal.
- Protect with admin/operator authentication.
- Disable in public production and expose through private monitoring.

### F13B-002 - Swagger appears globally enabled

Route: `/swagger`
Severity: P2
Status: OPEN FOR 13C/13D

Swagger is useful during integration, but public Swagger in production reveals the API surface. This is not automatically a blocker for staging, but it must be an explicit production decision.

Required decision:

- Keep enabled during controlled validation.
- Disable after API contract validation.
- Protect it behind admin/operator auth or environment gates.

### F13B-003 - Provision endpoint must stay server-side protected

Route: `POST /api/v1/auth/provision`
Severity: P1 if unprotected by service-level secret validation
Status: REQUIRES CONFIRMATION IN 13C

The route itself is public by controller design. That can be acceptable only if the service requires a provision key or equivalent server-side guard. MACROFASE 13B does not execute this endpoint.

Required decision:

- Verify service-level provision protection exists and is enforced.
- Verify `PROVISION_KEY` and `JWT_KEY` are separate.
- Verify failed provision attempts do not reveal sensitive tenant/license details.

### F13B-004 - Releases static file exposure must be intentionally scoped

Route: `/releases/*`
Severity: P2
Status: OPEN FOR RELEASE CHAIN VALIDATION

The releases directory is exposed for update delivery. This is expected for the POS update chain, but must be validated for artifact integrity and no accidental files.

Required decision:

- Only release artifacts are published.
- Checksums and manifests are validated.
- No environment files, secrets, logs, or debug artifacts are exposed.

### F13B-005 - Protected endpoint unauthenticated response shape needs normalization later

Routes: protected `/api/v1/*`
Severity: P3
Status: OPEN FOR AUTH CONTRACT VALIDATION

Protected endpoints should not return 200 without a valid JWT. Whether they return 401, 403, or a tenant-middleware 400 should be normalized later for client consistency. MACROFASE 13B only verifies they are not publicly accessible.

## Non-goals for 13B

- No login brute-force test.
- No token generation.
- No authenticated writes.
- No checkout test.
- No inventory mutation.
- No sync apply mutation.
- No user/license generation.
