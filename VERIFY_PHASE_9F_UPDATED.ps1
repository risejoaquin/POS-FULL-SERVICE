$ErrorActionPreference = "Stop"

function Assert-FileContains {
    param(
        [string]$Path,
        [string]$Text
    )

    if (!(Test-Path $Path)) {
        throw "Missing file: $Path"
    }

    $content = Get-Content -Raw -Path $Path
    if (!$content.Contains($Text)) {
        throw "Missing marker in ${Path}: $Text"
    }
}

Assert-FileContains "PosCore\Security\PosInstallerUninstallCleanupSimulationValidation.cs" "PosInstallerUninstallCleanupSimulationValidation"
Assert-FileContains "PosCore\Security\PosInstallerUninstallCleanupSimulationValidation.cs" "POS Installer Uninstall and Cleanup Simulation Validation"
Assert-FileContains "PosCore\Security\PosInstallerUninstallCleanupSimulationValidation.cs" "installer uninstall cleanup simulation validation documented"
Assert-FileContains "PosCore\Security\PosInstallerUninstallCleanupSimulationValidation.cs" "PHASE 9E launcher package prerequisite documented"
Assert-FileContains "PosCore\Security\PosInstallerUninstallCleanupSimulationValidation.cs" "465 tests passed source evidence documented"
Assert-FileContains "PosCore\Security\PosInstallerUninstallCleanupSimulationValidation.cs" "470 tests expected after installer uninstall cleanup simulation validation documented"
Assert-FileContains "PosCore\Security\PosInstallerUninstallCleanupSimulationValidation.cs" "uninstall cleanup plan generation documented"
Assert-FileContains "PosCore\Security\PosInstallerUninstallCleanupSimulationValidation.cs" "uninstall cleanup evidence generation documented"
Assert-FileContains "PosCore\Security\PosInstallerUninstallCleanupSimulationValidation.cs" "dry run only cleanup documented"
Assert-FileContains "PosCore\Security\PosInstallerUninstallCleanupSimulationValidation.cs" "no real file deletion"
Assert-FileContains "PosCore\Security\PosInstallerUninstallCleanupSimulationValidation.cs" "no real shortcut deletion"
Assert-FileContains "PosCore\Security\PosInstallerUninstallCleanupSimulationValidation.cs" "no Program Files mutation"
Assert-FileContains "PosCore\Security\PosInstallerUninstallCleanupSimulationValidation.cs" "no Desktop mutation"
Assert-FileContains "PosCore\Security\PosInstallerUninstallCleanupSimulationValidation.cs" "no Windows registry mutation"
Assert-FileContains "PosCore\Security\PosInstallerUninstallCleanupSimulationValidation.cs" "no real installer execution"
Assert-FileContains "PosCore\Security\PosInstallerUninstallCleanupSimulationValidation.cs" "no checkout behavior change"
Assert-FileContains "PosCore\Security\PosInstallerUninstallCleanupSimulationValidation.cs" "no inventory mutation"
Assert-FileContains "PosCore\Security\PosInstallerUninstallCleanupSimulationValidation.cs" "no production sync enablement"
Assert-FileContains "PosCore\Security\PosInstallerUninstallCleanupSimulationValidation.cs" "no deployment execution"
Assert-FileContains "PosCore\Security\PosInstallerUninstallCleanupSimulationValidation.cs" "no public API behavior change"
Assert-FileContains "PosCore\Security\PosInstallerUninstallCleanupSimulationValidation.cs" "no schema change"
Assert-FileContains "PosCore\Security\PosInstallerUninstallCleanupSimulationValidation.cs" "no migrations"
Assert-FileContains "scripts\release\Simulate-Phase9InstallerUninstallCleanup.ps1" "param("
Assert-FileContains "scripts\release\Simulate-Phase9InstallerUninstallCleanup.ps1" "Generate-Phase9LaunchAndShortcutPackage.ps1"
Assert-FileContains "scripts\release\Simulate-Phase9InstallerUninstallCleanup.ps1" "uninstall-cleanup-plan.json"
Assert-FileContains "scripts\release\Simulate-Phase9InstallerUninstallCleanup.ps1" "uninstall-cleanup-evidence.json"
Assert-FileContains "scripts\release\Simulate-Phase9InstallerUninstallCleanup.ps1" "desktopShortcutCandidates"
Assert-FileContains "scripts\release\Simulate-Phase9InstallerUninstallCleanup.ps1" "preservedReleaseManifests"
Assert-FileContains "scripts\release\Simulate-Phase9InstallerUninstallCleanup.ps1" "preservedChecksums"
Assert-FileContains "scripts\release\Simulate-Phase9InstallerUninstallCleanup.ps1" "preservedAuditEvidence"
Assert-FileContains "scripts\release\Simulate-Phase9InstallerUninstallCleanup.ps1" "PHASE 9F installer uninstall and cleanup simulation verified."
Assert-FileContains "docs\POS_INSTALLER_UNINSTALL_CLEANUP_SIMULATION_VALIDATION.md" "installer uninstall cleanup simulation validation documented"
Assert-FileContains "docs\PHASE_9F_INSTALLER_UNINSTALL_CLEANUP_SIMULATION_VALIDATION.md" "470 tests passed"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_9F.md" "Release Execution advanced from 50% to 60%"
Assert-FileContains "README.md" "PHASE 9F"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "PHASE 9F"

Write-Host "PHASE 9F markers verified."
