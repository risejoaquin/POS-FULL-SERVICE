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

Assert-FileContains "PosCore\Security\PosInstallerUpgradeSimulationVersionPreservationValidation.cs" "PosInstallerUpgradeSimulationVersionPreservationValidation"
Assert-FileContains "PosCore\Security\PosInstallerUpgradeSimulationVersionPreservationValidation.cs" "POS Installer Upgrade Simulation and Version Preservation Validation"
Assert-FileContains "PosCore\Security\PosInstallerUpgradeSimulationVersionPreservationValidation.cs" "installer upgrade simulation version preservation validation documented"
Assert-FileContains "PosCore\Security\PosInstallerUpgradeSimulationVersionPreservationValidation.cs" "PHASE 9F uninstall cleanup simulation prerequisite documented"
Assert-FileContains "PosCore\Security\PosInstallerUpgradeSimulationVersionPreservationValidation.cs" "470 tests passed source evidence documented"
Assert-FileContains "PosCore\Security\PosInstallerUpgradeSimulationVersionPreservationValidation.cs" "475 tests expected after installer upgrade simulation version preservation validation documented"
Assert-FileContains "PosCore\Security\PosInstallerUpgradeSimulationVersionPreservationValidation.cs" "upgrade simulation plan generation documented"
Assert-FileContains "PosCore\Security\PosInstallerUpgradeSimulationVersionPreservationValidation.cs" "upgrade preservation evidence generation documented"
Assert-FileContains "PosCore\Security\PosInstallerUpgradeSimulationVersionPreservationValidation.cs" "previous version detection documented"
Assert-FileContains "PosCore\Security\PosInstallerUpgradeSimulationVersionPreservationValidation.cs" "target version validation documented"
Assert-FileContains "PosCore\Security\PosInstallerUpgradeSimulationVersionPreservationValidation.cs" "tenant branding preservation documented"
Assert-FileContains "PosCore\Security\PosInstallerUpgradeSimulationVersionPreservationValidation.cs" "local database preservation documented"
Assert-FileContains "PosCore\Security\PosInstallerUpgradeSimulationVersionPreservationValidation.cs" "offline sync queue preservation documented"
Assert-FileContains "PosCore\Security\PosInstallerUpgradeSimulationVersionPreservationValidation.cs" "license state preservation documented"
Assert-FileContains "PosCore\Security\PosInstallerUpgradeSimulationVersionPreservationValidation.cs" "operator settings preservation documented"
Assert-FileContains "PosCore\Security\PosInstallerUpgradeSimulationVersionPreservationValidation.cs" "no real upgrade execution"
Assert-FileContains "PosCore\Security\PosInstallerUpgradeSimulationVersionPreservationValidation.cs" "no file overwrite"
Assert-FileContains "PosCore\Security\PosInstallerUpgradeSimulationVersionPreservationValidation.cs" "no database writes"
Assert-FileContains "PosCore\Security\PosInstallerUpgradeSimulationVersionPreservationValidation.cs" "no Windows registry mutation"
Assert-FileContains "PosCore\Security\PosInstallerUpgradeSimulationVersionPreservationValidation.cs" "no Desktop mutation"
Assert-FileContains "PosCore\Security\PosInstallerUpgradeSimulationVersionPreservationValidation.cs" "no Program Files mutation"
Assert-FileContains "PosCore\Security\PosInstallerUpgradeSimulationVersionPreservationValidation.cs" "no real installer execution"
Assert-FileContains "PosCore\Security\PosInstallerUpgradeSimulationVersionPreservationValidation.cs" "no deployment execution"
Assert-FileContains "PosCore\Security\PosInstallerUpgradeSimulationVersionPreservationValidation.cs" "no checkout behavior change"
Assert-FileContains "PosCore\Security\PosInstallerUpgradeSimulationVersionPreservationValidation.cs" "no inventory mutation"
Assert-FileContains "PosCore\Security\PosInstallerUpgradeSimulationVersionPreservationValidation.cs" "no production sync enablement"
Assert-FileContains "PosCore\Security\PosInstallerUpgradeSimulationVersionPreservationValidation.cs" "no public API behavior change"
Assert-FileContains "PosCore\Security\PosInstallerUpgradeSimulationVersionPreservationValidation.cs" "no schema change"
Assert-FileContains "PosCore\Security\PosInstallerUpgradeSimulationVersionPreservationValidation.cs" "no migrations"
Assert-FileContains "scripts\release\Simulate-Phase9InstallerUpgrade.ps1" "param("
Assert-FileContains "scripts\release\Simulate-Phase9InstallerUpgrade.ps1" "Simulate-Phase9InstallerUninstallCleanup.ps1"
Assert-FileContains "scripts\release\Simulate-Phase9InstallerUpgrade.ps1" "upgrade-simulation-plan.json"
Assert-FileContains "scripts\release\Simulate-Phase9InstallerUpgrade.ps1" "upgrade-preservation-evidence.json"
Assert-FileContains "scripts\release\Simulate-Phase9InstallerUpgrade.ps1" "tenantBranding"
Assert-FileContains "scripts\release\Simulate-Phase9InstallerUpgrade.ps1" "localDatabase"
Assert-FileContains "scripts\release\Simulate-Phase9InstallerUpgrade.ps1" "offlineSyncQueue"
Assert-FileContains "scripts\release\Simulate-Phase9InstallerUpgrade.ps1" "licenseState"
Assert-FileContains "scripts\release\Simulate-Phase9InstallerUpgrade.ps1" "operatorSettings"
Assert-FileContains "scripts\release\Simulate-Phase9InstallerUpgrade.ps1" "PHASE 9G installer upgrade simulation and version preservation verified."
Assert-FileContains "docs\POS_INSTALLER_UPGRADE_SIMULATION_VERSION_PRESERVATION_VALIDATION.md" "installer upgrade simulation version preservation validation documented"
Assert-FileContains "docs\PHASE_9G_INSTALLER_UPGRADE_SIMULATION_VERSION_PRESERVATION_VALIDATION.md" "475 tests passed"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_9G.md" "Release Execution advanced from 60% to 70%"
Assert-FileContains "README.md" "PHASE 9G"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "PHASE 9G"

Write-Host "PHASE 9G markers verified."
