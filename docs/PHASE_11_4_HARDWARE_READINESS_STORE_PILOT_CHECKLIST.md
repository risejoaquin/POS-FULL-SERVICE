# PHASE 11.4 - Hardware Readiness and Store Pilot Checklist

## Status

PENDING LOCAL VERIFICATION.

## Scope

PHASE 11.4 hardware readiness and store pilot checklist documented.

This block validates the final functional-business readiness layer before a controlled physical pilot: device readiness, operator training, pilot-store entry, issue capture, go-live rehearsal, support escalation, and exit criteria.

## Baseline

- Source baseline: 588 tests passed
- Expected after this phase: 604 tests passed

## Expected command

```powershell
.\VERIFY_PHASE_11_4_UPDATED.ps1

dotnet test

dotnet build -c Release Pos.sln

.\scripts\release\Validate-Phase11HardwareReadinessStorePilotValidation.ps1 -ReleaseVersion 0.9.0-rc.1 -PreviousVersion 0.9.0-rc.0 -ReleaseChannel release-candidate
```

## Expected result

```text
PHASE 11.4 markers verified.
604 tests passed
0 failed
Compilación correcta.
0 Advertencia(s)
0 Errores
PHASE 11.4 hardware readiness and store pilot checklist verified.
AcceptedChecks: 15
BlockingIssues: 0
```

## Guardrails

No real hardware access, no live device mutation, no printer execution, no cash drawer pulse, no scanner capture, no payment terminal execution, no store pilot activation, no production traffic routing, no real inventory mutation, no production sync enablement, no public API behavior change, no schema change, and no migrations.
