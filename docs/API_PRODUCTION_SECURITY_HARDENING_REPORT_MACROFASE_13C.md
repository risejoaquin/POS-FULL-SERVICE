# API Production Security Hardening Report - MACROFASE 13C

Previous evidence from 13B:
- Endpoint inventory exported 26 endpoints.
- Local test suite passed with 643 tests.
- Release build passed with 0 warnings and 0 errors.
- Production GET-only validation passed.
- Protected GET probes without JWT did not return 200.

Findings addressed in this patch:

## F13B-001 - Metrics exposure and 500 responses

Before 13C, optional metrics probes returned 500 for:
- /health/metrics
- /metrics

13C hardening makes these routes deterministic and non-public:
- /metrics -> 404 JSON
- /health/metrics -> 404 JSON

Marker: Metrics public exposure hardened: /metrics and /health/metrics return 404

## F13B-002 - Swagger public production exposure

Swagger middleware was previously unconditional. 13C gates Swagger behind:
- Development environment, or
- ENABLE_SWAGGER=true

Marker: Swagger production gate: ENABLE_SWAGGER

## F13B-005 - Protected response normalization

Protected routes without JWT previously could return 400 from TenantMiddleware before authorization semantics were visible. 13C normalizes unauthenticated protected API access to:
- HTTP 401
- JSON body with code UNAUTHORIZED

Marker: Protected unauthenticated API response normalized to 401

No business write endpoint is executed by the validation script.
