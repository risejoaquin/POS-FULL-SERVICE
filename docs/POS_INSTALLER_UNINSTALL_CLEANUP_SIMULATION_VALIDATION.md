# POS Installer Uninstall and Cleanup Simulation Validation

PHASE 9F documents controlled installer uninstall cleanup simulation validation.

Markers:

- installer uninstall cleanup simulation validation documented
- PHASE 9E launcher package prerequisite documented
- 465 tests passed source evidence documented
- 470 tests expected after installer uninstall cleanup simulation validation documented
- uninstall cleanup plan generation documented
- uninstall cleanup evidence generation documented
- simulated install directory cleanup candidate documented
- launcher package directory cleanup candidate documented
- desktop shortcut candidate cleanup documented
- temporary verification directory cleanup candidate documented
- generated installer artifacts preservation documented
- release manifests preservation documented
- checksums preservation documented
- audit evidence preservation documented
- dry run only cleanup documented

## Scope

This phase validates what the installer uninstall cleanup workflow would remove or preserve without deleting anything from the operator machine. The script uses PHASE 9E launcher package evidence, PHASE 9D smoke install evidence, and PHASE 9C/9B/9A generated release artifacts as inputs.

## Required inputs

- `artifacts/release/phase9/installer/pos-installer-package-launch-0.9.0-rc.1.zip`
- `artifacts/release/phase9/installer/launcher-package-manifest.json`
- `artifacts/release/phase9/installer/launcher-checksums.sha256`
- `artifacts/release/phase9/launcher/pos-installer-package-launch-0.9.0-rc.1/launch/desktop-shortcut-spec.json`
- `artifacts/release/phase9/smoke-install/smoke-install-evidence.json`
- `artifacts/release/phase9/smoke-install/pos-installer-package-0.9.0-rc.1`

If inputs are missing, `Simulate-Phase9InstallerUninstallCleanup.ps1` calls `Generate-Phase9LaunchAndShortcutPackage.ps1` to regenerate the prerequisite chain.

## Outputs

- `artifacts/release/phase9/uninstall-simulation/uninstall-cleanup-plan.json`
- `artifacts/release/phase9/uninstall-simulation/uninstall-cleanup-evidence.json`

## Cleanup plan categories

- simulatedInstallDirectory
- launcherPackageDirectory
- desktopShortcutCandidates
- temporaryVerificationDirectories
- generatedInstallerArtifacts
- preservedReleaseManifests
- preservedChecksums
- preservedAuditEvidence

## Guardrails

- no real file deletion
- no real shortcut deletion
- no Program Files mutation
- no Desktop mutation
- no Windows registry mutation
- no real installer execution
- no checkout behavior change
- no inventory mutation
- no production sync enablement
- no deployment execution
- no public API behavior change
- no schema change
- no migrations
