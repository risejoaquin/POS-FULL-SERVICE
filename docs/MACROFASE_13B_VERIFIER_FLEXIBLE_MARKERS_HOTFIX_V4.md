# MACROFASE 13B Verifier Flexible Markers Hotfix V4

This hotfix fixes the local PowerShell verifier only.

## Reason

The previous verifier required the exact marker:

```text
Endpoint count: 26
```

Some generated documentation may format the same evidence as Markdown, for example:

```text
Endpoint count: `26`
Endpoint count: **26**
26 routes
26 endpoints
```

The validation evidence is still valid because the export script already produced an endpoint inventory with count 26 and the production validation script passed.

## Changes

- Uses `Assert-ContainsAnyLiteral`.
- Keeps `.Contains()` literal checks.
- Keeps `-LiteralPath`.
- Keeps safe `${Path}` interpolation.
- Accepts equivalent endpoint-count markers.
- Does not change application runtime code.
- Does not change Railway deployment behavior.
- Does not call production mutation endpoints.

## Expected output

```text
MACROFASE 13B API endpoint inventory contract validation markers verified.
Verifier hotfix V4 verified: flexible literal markers, no wildcard parsing, safe colon interpolation.
```
