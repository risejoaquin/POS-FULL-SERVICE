# POS Installer Smoke Install Simulation and Package Extraction Validation

PHASE 9D documents installer smoke install simulation package extraction validation documented.

This phase depends on PHASE 9C installer package integrity verification prerequisite documented and the 455 tests passed source evidence documented. After this phase, 460 tests expected after installer smoke install simulation package extraction validation documented.

## Scope

The phase adds a controlled smoke install simulation that extracts the installer package into a simulated install directory. It does not run a real installer and does not deploy anything.

## Execution script

Operator smoke install simulation command documented:

```powershell
.\scripts\release\Simulate-Phase9InstallerSmokeInstall.ps1 -ReleaseVersion 0.9.0-rc.1 -ReleaseChannel release-candidate
```

The script documents and validates:

- simulated install directory creation documented
- installer package extraction to simulated install directory documented
- PosCore simulated install content verification documented
- PosBuilder simulated install content verification documented
- PosServer simulated install content verification documented
- release manifest simulated install content verification documented
- checksums simulated install content verification documented
- simulated install file count evidence documented
- simulated install executable candidate discovery documented
- simulated install smoke evidence manifest documented
- smoke install simulation failure handling documented

## Outputs

Expected outputs:

- `artifacts/release/phase9/smoke-install/pos-installer-package-0.9.0-rc.1/`
- `artifacts/release/phase9/smoke-install/smoke-install-evidence.json`

## Safety boundary

- no real installer execution
- no checkout behavior change
- no inventory mutation
- no production sync enablement
- no deployment execution
- no public API behavior change
- no schema change
- no migrations
