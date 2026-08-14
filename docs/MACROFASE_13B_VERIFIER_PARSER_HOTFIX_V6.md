# MACROFASE 13B verifier parser hotfix V6

This hotfix replaces the MACROFASE 13B verifier with a parser-safe and artifact-aware PowerShell implementation.

## Problem resolved

The previous verifier required a strict Markdown marker for the endpoint count in `docs/API_ENDPOINT_INVENTORY_PRODUCTION_CONTRACT.md`.
That was too brittle because the endpoint count was already proven by the export script output and by generated artifacts.

## V6 behavior

- Uses literal `.Contains()` checks only.
- Avoids wildcard parsing.
- Avoids `$Path:` interpolation errors.
- Avoids smart dash parser issues.
- Does not require a strict `Endpoint count: 26` Markdown line in the contract doc.
- If `artifacts/macro13b` exists, validates CSV/JSON inventory artifacts contain 26 endpoints.
- If artifacts are absent, the verifier still validates source docs and scripts and tells the operator to regenerate artifacts when needed.

## Accepted external evidence

- Endpoint inventory export reported 26 endpoints.
- Local test suite reported 643 passed and 0 failed.
- Release build reported 0 warnings and 0 errors.
- Production validation passed with GET-only public endpoint checks and protected read probes.

## Closure intent

This hotfix is only a verifier quality fix. It does not change API runtime behavior, database schema, migrations, Railway deployment, checkout, inventory, sync, orders, payments, or tenant data.
