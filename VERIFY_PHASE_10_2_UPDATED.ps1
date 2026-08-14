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

Assert-FileContains "VERIFY_PHASE_10_1_UPDATED.ps1" "PHASE 10.1 markers verified."
Assert-FileContains "PosCore\Security\PosBackupRestoreDeploymentSimulationValidation.cs" "PosBackupRestoreDeploymentSimulationValidation"
Assert-FileContains "PosCore\Security\PosBackupRestoreDeploymentSimulationValidation.cs" "PHASE 10.2 backup restore and deployment simulation documented"
Assert-FileContains "PosCore\Security\PosBackupRestoreDeploymentSimulationValidation.cs" "PHASE 10D backup and restore drill validation documented"
Assert-FileContains "PosCore\Security\PosBackupRestoreDeploymentSimulationValidation.cs" "PHASE 10E production deployment pipeline simulation documented"
Assert-FileContains "PosCore\Security\PosBackupRestoreDeploymentSimulationValidation.cs" "PHASE 10.1 production environment readiness prerequisite documented"
Assert-FileContains "PosCore\Security\PosBackupRestoreDeploymentSimulationValidation.cs" "505 tests passed source evidence documented"
Assert-FileContains "PosCore\Security\PosBackupRestoreDeploymentSimulationValidation.cs" "515 tests expected after backup restore and deployment simulation validation documented"
Assert-FileContains "PosCore\Security\PosBackupRestoreDeploymentSimulationValidation.cs" "backup-restore-drill-evidence.json generation documented"
Assert-FileContains "PosCore\Security\PosBackupRestoreDeploymentSimulationValidation.cs" "deployment-pipeline-simulation-report.json generation documented"
Assert-FileContains "PosCore\Security\PosBackupRestoreDeploymentSimulationValidation.cs" "deployment-promotion-gate-report.json generation documented"
Assert-FileContains "PosCore\Security\PosBackupRestoreDeploymentSimulationValidation.cs" "backup plan documented"
Assert-FileContains "PosCore\Security\PosBackupRestoreDeploymentSimulationValidation.cs" "restore drill evidence documented"
Assert-FileContains "PosCore\Security\PosBackupRestoreDeploymentSimulationValidation.cs" "deployment simulation documented"
Assert-FileContains "PosCore\Security\PosBackupRestoreDeploymentSimulationValidation.cs" "release artifact promotion checklist documented"
Assert-FileContains "PosCore\Security\PosBackupRestoreDeploymentSimulationValidation.cs" "rollback checkpoint documented"
Assert-FileContains "PosCore\Security\PosBackupRestoreDeploymentSimulationValidation.cs" "operator approval gate documented"
Assert-FileContains "PosCore\Security\PosBackupRestoreDeploymentSimulationValidation.cs" "no real deployment execution"
Assert-FileContains "PosCore\Security\PosBackupRestoreDeploymentSimulationValidation.cs" "no Railway mutation"
Assert-FileContains "PosCore\Security\PosBackupRestoreDeploymentSimulationValidation.cs" "no Supabase mutation"
Assert-FileContains "PosCore\Security\PosBackupRestoreDeploymentSimulationValidation.cs" "no production database mutation"
Assert-FileContains "PosCore\Security\PosBackupRestoreDeploymentSimulationValidation.cs" "no backup deletion"
Assert-FileContains "PosCore\Security\PosBackupRestoreDeploymentSimulationValidation.cs" "no restore execution against production"
Assert-FileContains "PosCore\Security\PosBackupRestoreDeploymentSimulationValidation.cs" "no release promotion"
Assert-FileContains "PosCore\Security\PosBackupRestoreDeploymentSimulationValidation.cs" "no schema change"
Assert-FileContains "PosCore\Security\PosBackupRestoreDeploymentSimulationValidation.cs" "no migrations"

Assert-FileContains "scripts\release\Validate-Phase10BackupRestoreDeploymentSimulation.ps1" "param("
Assert-FileContains "scripts\release\Validate-Phase10BackupRestoreDeploymentSimulation.ps1" "Validate-Phase10ProductionReadiness.ps1"
Assert-FileContains "scripts\release\Validate-Phase10BackupRestoreDeploymentSimulation.ps1" "PHASE 10.1 production readiness outputs are missing"
Assert-FileContains "scripts\release\Validate-Phase10BackupRestoreDeploymentSimulation.ps1" "backup-restore-drill-evidence.json"
Assert-FileContains "scripts\release\Validate-Phase10BackupRestoreDeploymentSimulation.ps1" "deployment-pipeline-simulation-report.json"
Assert-FileContains "scripts\release\Validate-Phase10BackupRestoreDeploymentSimulation.ps1" "deployment-promotion-gate-report.json"
Assert-FileContains "scripts\release\Validate-Phase10BackupRestoreDeploymentSimulation.ps1" "no real deployment execution"
Assert-FileContains "scripts\release\Validate-Phase10BackupRestoreDeploymentSimulation.ps1" "no Railway mutation"
Assert-FileContains "scripts\release\Validate-Phase10BackupRestoreDeploymentSimulation.ps1" "no Supabase mutation"
Assert-FileContains "scripts\release\Validate-Phase10BackupRestoreDeploymentSimulation.ps1" "no production database mutation"
Assert-FileContains "scripts\release\Validate-Phase10BackupRestoreDeploymentSimulation.ps1" "no backup deletion"
Assert-FileContains "scripts\release\Validate-Phase10BackupRestoreDeploymentSimulation.ps1" "no restore execution against production"
Assert-FileContains "scripts\release\Validate-Phase10BackupRestoreDeploymentSimulation.ps1" "no release promotion"
Assert-FileContains "scripts\release\Validate-Phase10BackupRestoreDeploymentSimulation.ps1" "PHASE 10.2 backup restore and deployment simulation verified."
Assert-FileContains "scripts\release\Validate-Phase10BackupRestoreDeploymentSimulation.ps1" "AcceptedChecks: 10"
Assert-FileContains "scripts\release\Validate-Phase10BackupRestoreDeploymentSimulation.ps1" "BlockingIssues: 0"

Assert-FileContains "docs\POS_BACKUP_RESTORE_DEPLOYMENT_SIMULATION.md" "PHASE 10.2 backup restore and deployment simulation documented"
Assert-FileContains "docs\PHASE_10_2_BACKUP_RESTORE_DEPLOYMENT_SIMULATION.md" "505 tests passed"
Assert-FileContains "docs\PHASE_10_2_BACKUP_RESTORE_DEPLOYMENT_SIMULATION.md" "515 tests passed"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_10_2.md" "Production readiness advanced from 30% to 55%"
Assert-FileContains "README.md" "PHASE 10.2"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "PHASE 10.2"

Write-Host "PHASE 10.2 markers verified."
