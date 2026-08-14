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

Assert-FileContains "PosCore\Security\PosInstallerRollbackSimulationPreviousVersionRecoveryValidation.cs" "PosInstallerRollbackSimulationPreviousVersionRecoveryValidation"
Assert-FileContains "PosCore\Security\PosInstallerRollbackSimulationPreviousVersionRecoveryValidation.cs" "POS Installer Rollback Simulation and Previous Version Recovery Validation"
Assert-FileContains "PosCore\Security\PosInstallerRollbackSimulationPreviousVersionRecoveryValidation.cs" "installer rollback simulation previous version recovery validation documented"
Assert-FileContains "PosCore\Security\PosInstallerRollbackSimulationPreviousVersionRecoveryValidation.cs" "PHASE 9G upgrade simulation prerequisite documented"
Assert-FileContains "PosCore\Security\PosInstallerRollbackSimulationPreviousVersionRecoveryValidation.cs" "475 tests passed source evidence documented"
Assert-FileContains "PosCore\Security\PosInstallerRollbackSimulationPreviousVersionRecoveryValidation.cs" "480 tests expected after installer rollback simulation previous version recovery validation documented"
Assert-FileContains "PosCore\Security\PosInstallerRollbackSimulationPreviousVersionRecoveryValidation.cs" "rollback simulation plan generation documented"
Assert-FileContains "PosCore\Security\PosInstallerRollbackSimulationPreviousVersionRecoveryValidation.cs" "previous version recovery evidence generation documented"
Assert-FileContains "PosCore\Security\PosInstallerRollbackSimulationPreviousVersionRecoveryValidation.cs" "rollback source version detection documented"
Assert-FileContains "PosCore\Security\PosInstallerRollbackSimulationPreviousVersionRecoveryValidation.cs" "rollback target version validation documented"
Assert-FileContains "PosCore\Security\PosInstallerRollbackSimulationPreviousVersionRecoveryValidation.cs" "tenant branding recovery preservation documented"
Assert-FileContains "PosCore\Security\PosInstallerRollbackSimulationPreviousVersionRecoveryValidation.cs" "local database recovery preservation documented"
Assert-FileContains "PosCore\Security\PosInstallerRollbackSimulationPreviousVersionRecoveryValidation.cs" "offline sync queue recovery preservation documented"
Assert-FileContains "PosCore\Security\PosInstallerRollbackSimulationPreviousVersionRecoveryValidation.cs" "license state recovery preservation documented"
Assert-FileContains "PosCore\Security\PosInstallerRollbackSimulationPreviousVersionRecoveryValidation.cs" "operator settings recovery preservation documented"
Assert-FileContains "PosCore\Security\PosInstallerRollbackSimulationPreviousVersionRecoveryValidation.cs" "no real rollback execution"
Assert-FileContains "PosCore\Security\PosInstallerRollbackSimulationPreviousVersionRecoveryValidation.cs" "no file overwrite"
Assert-FileContains "PosCore\Security\PosInstallerRollbackSimulationPreviousVersionRecoveryValidation.cs" "no database writes"
Assert-FileContains "PosCore\Security\PosInstallerRollbackSimulationPreviousVersionRecoveryValidation.cs" "no Windows registry mutation"
Assert-FileContains "PosCore\Security\PosInstallerRollbackSimulationPreviousVersionRecoveryValidation.cs" "no Desktop mutation"
Assert-FileContains "PosCore\Security\PosInstallerRollbackSimulationPreviousVersionRecoveryValidation.cs" "no Program Files mutation"
Assert-FileContains "PosCore\Security\PosInstallerRollbackSimulationPreviousVersionRecoveryValidation.cs" "no real installer execution"
Assert-FileContains "PosCore\Security\PosInstallerRollbackSimulationPreviousVersionRecoveryValidation.cs" "no deployment execution"
Assert-FileContains "PosCore\Security\PosInstallerRollbackSimulationPreviousVersionRecoveryValidation.cs" "no checkout behavior change"
Assert-FileContains "PosCore\Security\PosInstallerRollbackSimulationPreviousVersionRecoveryValidation.cs" "no inventory mutation"
Assert-FileContains "PosCore\Security\PosInstallerRollbackSimulationPreviousVersionRecoveryValidation.cs" "no production sync enablement"
Assert-FileContains "PosCore\Security\PosInstallerRollbackSimulationPreviousVersionRecoveryValidation.cs" "no public API behavior change"
Assert-FileContains "PosCore\Security\PosInstallerRollbackSimulationPreviousVersionRecoveryValidation.cs" "no schema change"
Assert-FileContains "PosCore\Security\PosInstallerRollbackSimulationPreviousVersionRecoveryValidation.cs" "no migrations"
Assert-FileContains "scripts\release\Simulate-Phase9InstallerRollback.ps1" "param("
Assert-FileContains "scripts\release\Simulate-Phase9InstallerRollback.ps1" "Simulate-Phase9InstallerUpgrade.ps1"
Assert-FileContains "scripts\release\Simulate-Phase9InstallerRollback.ps1" "rollback-simulation-plan.json"
Assert-FileContains "scripts\release\Simulate-Phase9InstallerRollback.ps1" "previous-version-recovery-evidence.json"
Assert-FileContains "scripts\release\Simulate-Phase9InstallerRollback.ps1" "rollbackSourceVersionDetectionDocumented"
Assert-FileContains "scripts\release\Simulate-Phase9InstallerRollback.ps1" "rollbackTargetVersionValidationDocumented"
Assert-FileContains "scripts\release\Simulate-Phase9InstallerRollback.ps1" "tenantBranding"
Assert-FileContains "scripts\release\Simulate-Phase9InstallerRollback.ps1" "localDatabase"
Assert-FileContains "scripts\release\Simulate-Phase9InstallerRollback.ps1" "offlineSyncQueue"
Assert-FileContains "scripts\release\Simulate-Phase9InstallerRollback.ps1" "licenseState"
Assert-FileContains "scripts\release\Simulate-Phase9InstallerRollback.ps1" "operatorSettings"
Assert-FileContains "scripts\release\Simulate-Phase9InstallerRollback.ps1" "PHASE 9H installer rollback simulation and previous version recovery verified."
Assert-FileContains "docs\POS_INSTALLER_ROLLBACK_SIMULATION_PREVIOUS_VERSION_RECOVERY_VALIDATION.md" "installer rollback simulation previous version recovery validation documented"
Assert-FileContains "docs\PHASE_9H_INSTALLER_ROLLBACK_SIMULATION_PREVIOUS_VERSION_RECOVERY_VALIDATION.md" "480 tests passed"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_9H.md" "Release Execution advanced from 70% to 80%"
Assert-FileContains "README.md" "PHASE 9H"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "PHASE 9H"

Write-Host "PHASE 9H markers verified."
