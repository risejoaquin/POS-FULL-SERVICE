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

Assert-FileContains "VERIFY_PHASE_10_2_UPDATED.ps1" "PHASE 10.2 markers verified."
Assert-FileContains "PosCore\Security\PosStagingExecutionSmokeTestsValidation.cs" "PosStagingExecutionSmokeTestsValidation"
Assert-FileContains "PosCore\Security\PosStagingExecutionSmokeTestsValidation.cs" "PHASE 10.3 staging execution and smoke tests documented"
Assert-FileContains "PosCore\Security\PosStagingExecutionSmokeTestsValidation.cs" "PHASE 10F staging deployment execution validation documented"
Assert-FileContains "PosCore\Security\PosStagingExecutionSmokeTestsValidation.cs" "PHASE 10G production smoke test checklist documented"
Assert-FileContains "PosCore\Security\PosStagingExecutionSmokeTestsValidation.cs" "PHASE 10.2 backup restore deployment simulation prerequisite documented"
Assert-FileContains "PosCore\Security\PosStagingExecutionSmokeTestsValidation.cs" "515 tests passed source evidence documented"
Assert-FileContains "PosCore\Security\PosStagingExecutionSmokeTestsValidation.cs" "525 tests expected after staging execution and smoke tests validation documented"
Assert-FileContains "PosCore\Security\PosStagingExecutionSmokeTestsValidation.cs" "staging-execution-evidence.json generation documented"
Assert-FileContains "PosCore\Security\PosStagingExecutionSmokeTestsValidation.cs" "staging-smoke-test-checklist.json generation documented"
Assert-FileContains "PosCore\Security\PosStagingExecutionSmokeTestsValidation.cs" "production-smoke-test-checklist.json generation documented"
Assert-FileContains "PosCore\Security\PosStagingExecutionSmokeTestsValidation.cs" "staging deployment checklist documented"
Assert-FileContains "PosCore\Security\PosStagingExecutionSmokeTestsValidation.cs" "staging health validation documented"
Assert-FileContains "PosCore\Security\PosStagingExecutionSmokeTestsValidation.cs" "POS startup smoke checklist documented"
Assert-FileContains "PosCore\Security\PosStagingExecutionSmokeTestsValidation.cs" "login smoke checklist documented"
Assert-FileContains "PosCore\Security\PosStagingExecutionSmokeTestsValidation.cs" "tenant context smoke checklist documented"
Assert-FileContains "PosCore\Security\PosStagingExecutionSmokeTestsValidation.cs" "basic sale smoke checklist documented"
Assert-FileContains "PosCore\Security\PosStagingExecutionSmokeTestsValidation.cs" "sync smoke checklist documented"
Assert-FileContains "PosCore\Security\PosStagingExecutionSmokeTestsValidation.cs" "admin operator checklist documented"
Assert-FileContains "PosCore\Security\PosStagingExecutionSmokeTestsValidation.cs" "no real production deployment"
Assert-FileContains "PosCore\Security\PosStagingExecutionSmokeTestsValidation.cs" "no production traffic routing"
Assert-FileContains "PosCore\Security\PosStagingExecutionSmokeTestsValidation.cs" "no Railway mutation"
Assert-FileContains "PosCore\Security\PosStagingExecutionSmokeTestsValidation.cs" "no Supabase mutation"
Assert-FileContains "PosCore\Security\PosStagingExecutionSmokeTestsValidation.cs" "no production database mutation"
Assert-FileContains "PosCore\Security\PosStagingExecutionSmokeTestsValidation.cs" "no real payment capture"
Assert-FileContains "PosCore\Security\PosStagingExecutionSmokeTestsValidation.cs" "no real inventory mutation"
Assert-FileContains "PosCore\Security\PosStagingExecutionSmokeTestsValidation.cs" "no release promotion"
Assert-FileContains "PosCore\Security\PosStagingExecutionSmokeTestsValidation.cs" "no schema change"
Assert-FileContains "PosCore\Security\PosStagingExecutionSmokeTestsValidation.cs" "no migrations"

Assert-FileContains "scripts\release\Validate-Phase10StagingExecutionSmokeTests.ps1" "param("
Assert-FileContains "scripts\release\Validate-Phase10StagingExecutionSmokeTests.ps1" "Validate-Phase10BackupRestoreDeploymentSimulation.ps1"
Assert-FileContains "scripts\release\Validate-Phase10StagingExecutionSmokeTests.ps1" "PHASE 10.2 backup restore deployment outputs are missing"
Assert-FileContains "scripts\release\Validate-Phase10StagingExecutionSmokeTests.ps1" "staging-execution-evidence.json"
Assert-FileContains "scripts\release\Validate-Phase10StagingExecutionSmokeTests.ps1" "staging-smoke-test-checklist.json"
Assert-FileContains "scripts\release\Validate-Phase10StagingExecutionSmokeTests.ps1" "production-smoke-test-checklist.json"
Assert-FileContains "scripts\release\Validate-Phase10StagingExecutionSmokeTests.ps1" "no real production deployment"
Assert-FileContains "scripts\release\Validate-Phase10StagingExecutionSmokeTests.ps1" "no production traffic routing"
Assert-FileContains "scripts\release\Validate-Phase10StagingExecutionSmokeTests.ps1" "no Railway mutation"
Assert-FileContains "scripts\release\Validate-Phase10StagingExecutionSmokeTests.ps1" "no Supabase mutation"
Assert-FileContains "scripts\release\Validate-Phase10StagingExecutionSmokeTests.ps1" "no production database mutation"
Assert-FileContains "scripts\release\Validate-Phase10StagingExecutionSmokeTests.ps1" "no real payment capture"
Assert-FileContains "scripts\release\Validate-Phase10StagingExecutionSmokeTests.ps1" "no real inventory mutation"
Assert-FileContains "scripts\release\Validate-Phase10StagingExecutionSmokeTests.ps1" "no release promotion"
Assert-FileContains "scripts\release\Validate-Phase10StagingExecutionSmokeTests.ps1" "PHASE 10.3 staging execution and smoke tests verified."
Assert-FileContains "scripts\release\Validate-Phase10StagingExecutionSmokeTests.ps1" "AcceptedChecks: 10"
Assert-FileContains "scripts\release\Validate-Phase10StagingExecutionSmokeTests.ps1" "BlockingIssues: 0"

Assert-FileContains "docs\POS_STAGING_EXECUTION_SMOKE_TESTS.md" "PHASE 10.3 staging execution and smoke tests documented"
Assert-FileContains "docs\PHASE_10_3_STAGING_EXECUTION_SMOKE_TESTS.md" "515 tests passed"
Assert-FileContains "docs\PHASE_10_3_STAGING_EXECUTION_SMOKE_TESTS.md" "525 tests passed"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_10_3.md" "Production readiness advanced from 55% to 75%"
Assert-FileContains "README.md" "PHASE 10.3"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "PHASE 10.3"

Write-Host "PHASE 10.3 markers verified."
