# API Production Endpoint Inventory

Status: BASELINE INVENTORY

## Public runtime endpoints

| Endpoint | Method | Expected status | Purpose | Mutates data |
|---|---:|---:|---|---:|
| / | GET | 200 | Root runtime confirmation | No |
| /health | GET | 200 | Basic API health | No |
| /api/health | GET | 200 | API-prefixed health alias | No |
| /health/live | GET | 200 | Liveness endpoint | No |
| /health/ready | GET | 200 | Readiness endpoint with database connectivity | No |

## Expected JSON signals

Root endpoint should return service and status values.

/health and /api/health should return status Healthy.

/health/live should return status Healthy.

/health/ready should return status Healthy and database Connected.

## Known browser noise

Browsers may request:

```text
/favicon.ico
```

A 400 response from favicon does not block MACROFASE 13 because it is not an API runtime health endpoint.

## Validation boundary

This inventory is intentionally small. It validates production readiness of the API shell before business endpoint validation.
