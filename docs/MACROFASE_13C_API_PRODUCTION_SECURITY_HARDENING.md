# MACROFASE 13C - API Production Security Hardening

Status: PENDING VERIFICATION

Goal: harden the public production API surface after the successful 13A and 13B runtime/inventory validation.

Scope:
- Disable Swagger UI in production unless ENABLE_SWAGGER=true.
- Make /metrics and /health/metrics non-public and deterministic with 404 instead of 500.
- Normalize unauthenticated protected API access to 401 with JSON response.
- Keep public runtime endpoints available: /, /health, /api/health, /health/live, /health/ready.
- Keep database readiness validation intact.

Out of scope:
- No checkout changes.
- No inventory mutation changes.
- No sales/returns/sync writes.
- No database schema changes.
- No migrations.
- No Railway variable mutation by the assistant.

Acceptance markers:
- MACROFASE 13C - API Production Security Hardening
- Swagger production gate: ENABLE_SWAGGER
- Metrics public exposure hardened: /metrics and /health/metrics return 404
- Protected unauthenticated API response normalized to 401
