# PHASE 9B — Installer Package Generation Execution

Status: PENDING LOCAL VERIFICATION

Prerequisite: PHASE 9A closed with 445 tests passed, release-manifest.json generated, and checksums.sha256 generated.

Expected after this phase: 450 tests passed, 0 failed, Compilación correcta, 0 Advertencia(s), 0 Errores.

This phase converts PHASE 9A published outputs into an installer package archive for operator review. It is not deployment execution.

Required files:

- `PosCore/Security/PosInstallerPackageGenerationExecution.cs`
- `scripts/release/Generate-Phase9InstallerPackage.ps1`
- `docs/POS_INSTALLER_PACKAGE_GENERATION_EXECUTION.md`
- `docs/PROJECT_PROGRESS_REPORT_PHASE_9B.md`
- `VERIFY_PHASE_9B_UPDATED.ps1`

Operator command:

```powershell
.\scripts\release\Generate-Phase9InstallerPackage.ps1 -ReleaseVersion 0.9.0-rc.1 -ReleaseChannel release-candidate
```

Expected script output:

```text
PHASE 9B installer package generated.
Package: artifacts\release\phase9\installer\pos-installer-package-0.9.0-rc.1.zip
Manifest: artifacts\release\phase9\installer\installer-package-manifest.json
Checksums: artifacts\release\phase9\installer\installer-checksums.sha256
```

Safety: no checkout behavior change; no inventory mutation; no production sync enablement; no deployment execution; no public API behavior change; no schema change; no migrations.


## Hotfix behavior
If `artifacts/release/phase9/publish/poscore-win-x64`, `posbuilder-win-x64`, `posserver`, `release-manifest.json`, or `checksums.sha256` are missing, `Generate-Phase9InstallerPackage.ps1` regenerates PHASE 9A release artifacts before creating the installer package.
