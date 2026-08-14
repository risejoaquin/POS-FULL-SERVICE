# PHASE 9F - Installer Uninstall and Cleanup Simulation Validation

Status: PENDING LOCAL VERIFICATION

PHASE 9F adds dry-run uninstall cleanup simulation validation for the PHASE 9 installer release chain.

## Source evidence

- PHASE 9E closed with 465 tests passed.
- PHASE 9E build had 0 Advertencia(s).
- PHASE 9E build had 0 Errores.
- PHASE 9E launcher package generated.
- PHASE 9E generated four launch scripts.

## Expected validation after this increment

- 470 tests passed
- 0 failed
- 0 Advertencia(s)
- 0 Errores
- PHASE 9F installer uninstall and cleanup simulation verified.

## Operator command

```powershell
.\scripts\release\Simulate-Phase9InstallerUninstallCleanup.ps1 -ReleaseVersion 0.9.0-rc.1 -ReleaseChannel release-candidate
```

## Expected outputs

- `artifacts\release\phase9\uninstall-simulation\uninstall-cleanup-plan.json`
- `artifacts\release\phase9\uninstall-simulation\uninstall-cleanup-evidence.json`

## Safety

PHASE 9F is dry run only. It does not delete files, shortcuts, registry keys, Program Files content, Desktop content, database content, release manifests, checksums, or audit evidence.
