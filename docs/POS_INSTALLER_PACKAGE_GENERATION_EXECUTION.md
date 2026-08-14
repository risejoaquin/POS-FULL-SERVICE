# POS Installer Package Generation Execution

PHASE 9B documents and scripts installer package generation execution from PHASE 9A published artifacts.

Required evidence:

- installer package generation execution documented
- PHASE 9A release artifact execution prerequisite documented
- 445 tests passed source evidence documented
- 450 tests expected after installer package generation execution documented
- published artifact source directory documented
- installer package staging directory documented
- PosCore published artifact input documented
- PosBuilder published artifact input documented
- PosServer published artifact input documented
- release manifest input documented
- checksums input documented
- installer package manifest generation documented
- installer package checksum generation documented
- installer package zip archive generation documented
- installer package output naming convention documented
- installer package verification command documented
- operator package generation command documented
- package failure handling checklist documented

## Execution script

Run after PHASE 9A artifacts exist:

```powershell
.\scripts\release\Generate-Phase9InstallerPackage.ps1 -ReleaseVersion 0.9.0-rc.1 -ReleaseChannel release-candidate
```

The script requires:

- `artifacts/release/phase9/publish/poscore-win-x64`
- `artifacts/release/phase9/publish/posbuilder-win-x64`
- `artifacts/release/phase9/publish/posserver`
- `artifacts/release/phase9/release-manifest.json`
- `artifacts/release/phase9/checksums.sha256`

It generates:

- `artifacts/release/phase9/installer/pos-installer-package-0.9.0-rc.1.zip`
- `artifacts/release/phase9/installer/installer-package-manifest.json`
- `artifacts/release/phase9/installer/installer-checksums.sha256`

Safety boundaries: no checkout behavior change; no inventory mutation; no production sync enablement; no deployment execution; no public API behavior change; no schema change; no migrations.


## Prerequisite recovery behavior
The package generator is operator-safe after fresh extraction: when PHASE 9A artifact inputs are absent, it invokes the PHASE 9A generator and then verifies all required inputs before creating the installer package.
