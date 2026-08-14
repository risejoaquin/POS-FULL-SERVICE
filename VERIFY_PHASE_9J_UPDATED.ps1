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

Assert-FileContains "PosCore\Security\PosInstallerReleaseExecutionClosureProductionHandoffValidation.cs" "PosInstallerReleaseExecutionClosureProductionHandoffValidation"
Assert-FileContains "PosCore\Security\PosInstallerReleaseExecutionClosureProductionHandoffValidation.cs" "POS Installer Release Execution Closure and Production Handoff Validation"
Assert-FileContains "PosCore\Security\PosInstallerReleaseExecutionClosureProductionHandoffValidation.cs" "installer release execution closure production handoff validation documented"
Assert-FileContains "PosCore\Security\PosInstallerReleaseExecutionClosureProductionHandoffValidation.cs" "PHASE 9I final evidence operator acceptance prerequisite documented"
Assert-FileContains "PosCore\Security\PosInstallerReleaseExecutionClosureProductionHandoffValidation.cs" "485 tests passed source evidence documented"
Assert-FileContains "PosCore\Security\PosInstallerReleaseExecutionClosureProductionHandoffValidation.cs" "490 tests expected after installer release execution closure production handoff validation documented"
Assert-FileContains "PosCore\Security\PosInstallerReleaseExecutionClosureProductionHandoffValidation.cs" "release-execution-closure-evidence.json generation documented"
Assert-FileContains "PosCore\Security\PosInstallerReleaseExecutionClosureProductionHandoffValidation.cs" "production-handoff-package.json generation documented"
Assert-FileContains "PosCore\Security\PosInstallerReleaseExecutionClosureProductionHandoffValidation.cs" "production handoff checklist documented"
Assert-FileContains "PosCore\Security\PosInstallerReleaseExecutionClosureProductionHandoffValidation.cs" "handoff blocking issues count documented"
Assert-FileContains "PosCore\Security\PosInstallerReleaseExecutionClosureProductionHandoffValidation.cs" "handoff accepted checks count documented"
Assert-FileContains "PosCore\Security\PosInstallerReleaseExecutionClosureProductionHandoffValidation.cs" "no real release execution"
Assert-FileContains "PosCore\Security\PosInstallerReleaseExecutionClosureProductionHandoffValidation.cs" "no real installer execution"
Assert-FileContains "PosCore\Security\PosInstallerReleaseExecutionClosureProductionHandoffValidation.cs" "no deployment execution"
Assert-FileContains "PosCore\Security\PosInstallerReleaseExecutionClosureProductionHandoffValidation.cs" "no schema change"
Assert-FileContains "PosCore\Security\PosInstallerReleaseExecutionClosureProductionHandoffValidation.cs" "no migrations"

Assert-FileContains "scripts\release\Simulate-Phase9ReleaseExecutionClosure.ps1" "param("
Assert-FileContains "scripts\release\Simulate-Phase9ReleaseExecutionClosure.ps1" "Simulate-Phase9ReleaseCandidateAcceptance.ps1"
Assert-FileContains "scripts\release\Simulate-Phase9ReleaseExecutionClosure.ps1" "release-execution-closure-evidence.json"
Assert-FileContains "scripts\release\Simulate-Phase9ReleaseExecutionClosure.ps1" "production-handoff-package.json"
Assert-FileContains "scripts\release\Simulate-Phase9ReleaseExecutionClosure.ps1" "releaseCandidateFinalEvidenceDocumented"
Assert-FileContains "scripts\release\Simulate-Phase9ReleaseExecutionClosure.ps1" "operatorAcceptanceChecklistEvidenceDocumented"
Assert-FileContains "scripts\release\Simulate-Phase9ReleaseExecutionClosure.ps1" "releaseArtifactChainHandoffDocumented"
Assert-FileContains "scripts\release\Simulate-Phase9ReleaseExecutionClosure.ps1" "installerPackageHandoffDocumented"
Assert-FileContains "scripts\release\Simulate-Phase9ReleaseExecutionClosure.ps1" "rollbackRecoveryHandoffDocumented"
Assert-FileContains "scripts\release\Simulate-Phase9ReleaseExecutionClosure.ps1" "PHASE 9J installer release execution closure and production handoff verified."
Assert-FileContains "scripts\release\Simulate-Phase9ReleaseExecutionClosure.ps1" "AcceptedChecks"
Assert-FileContains "scripts\release\Simulate-Phase9ReleaseExecutionClosure.ps1" "BlockingIssues"
Assert-FileContains "scripts\release\Simulate-Phase9ReleaseExecutionClosure.ps1" "noRealReleaseExecution"
Assert-FileContains "scripts\release\Simulate-Phase9ReleaseExecutionClosure.ps1" "noRealInstallerExecution"
Assert-FileContains "scripts\release\Simulate-Phase9ReleaseExecutionClosure.ps1" "noDeploymentExecution"
Assert-FileContains "scripts\release\Simulate-Phase9ReleaseExecutionClosure.ps1" "noSchemaChange"
Assert-FileContains "scripts\release\Simulate-Phase9ReleaseExecutionClosure.ps1" "noMigrations"

Assert-FileContains "docs\POS_INSTALLER_RELEASE_EXECUTION_CLOSURE_PRODUCTION_HANDOFF_VALIDATION.md" "installer release execution closure production handoff validation documented"
Assert-FileContains "docs\POS_INSTALLER_RELEASE_EXECUTION_CLOSURE_PRODUCTION_HANDOFF_VALIDATION.md" "PHASE 9I final evidence operator acceptance prerequisite documented"
Assert-FileContains "docs\PHASE_9J_INSTALLER_RELEASE_EXECUTION_CLOSURE_PRODUCTION_HANDOFF_VALIDATION.md" "485 tests passed"
Assert-FileContains "docs\PHASE_9J_INSTALLER_RELEASE_EXECUTION_CLOSURE_PRODUCTION_HANDOFF_VALIDATION.md" "490 tests passed"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_9J.md" "Release Execution advanced from 90% to 100%"
Assert-FileContains "README.md" "PHASE 9J"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "PHASE 9J"

Write-Host "PHASE 9J markers verified."
