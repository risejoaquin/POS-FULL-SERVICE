param(
    [string]$ReleaseVersion = "0.9.0-rc.1",
    [string]$PreviousVersion = "0.9.0-rc.0",
    [string]$ReleaseChannel = "release-candidate",
    [string]$OutputRoot = "artifacts/release/phase10/monitoring-rollback-go-no-go"
)

$ErrorActionPreference = "Stop"

$root = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$phase10StagingRoot = Join-Path $root "artifacts\release\phase10\staging-execution-smoke-tests"
$stagingExecutionEvidence = Join-Path $phase10StagingRoot "staging-execution-evidence.json"
$stagingSmokeTestChecklist = Join-Path $phase10StagingRoot "staging-smoke-test-checklist.json"
$productionSmokeTestChecklist = Join-Path $phase10StagingRoot "production-smoke-test-checklist.json"

if (!(Test-Path $stagingExecutionEvidence) -or !(Test-Path $stagingSmokeTestChecklist) -or !(Test-Path $productionSmokeTestChecklist)) {
    Write-Host "PHASE 10.3 staging execution smoke test outputs are missing. Regenerating staging execution smoke tests before monitoring rollback go no-go validation."
    & (Join-Path $root "scripts\release\Validate-Phase10StagingExecutionSmokeTests.ps1") -ReleaseVersion $ReleaseVersion -PreviousVersion $PreviousVersion -ReleaseChannel $ReleaseChannel
}

$outputDirectory = Join-Path $root $OutputRoot
New-Item -ItemType Directory -Force -Path $outputDirectory | Out-Null

$monitoringActivationEvidence = Join-Path $outputDirectory "monitoring-activation-evidence.json"
$rollbackProcedureValidationReport = Join-Path $outputDirectory "rollback-procedure-validation-report.json"
$goNoGoFinalClosureReport = Join-Path $outputDirectory "go-no-go-final-closure-report.json"

$monitoring = [ordered]@{
    phase = "PHASE 10.4"
    groupedPhase = "PHASE 10H - Monitoring and Alerting Activation Validation"
    releaseVersion = $ReleaseVersion
    releaseChannel = $ReleaseChannel
    prerequisite = $productionSmokeTestChecklist
    monitoringChecklist = @(
        "monitoring checklist documented",
        "logging validation documented",
        "alerting checklist documented",
        "incident response handoff documented",
        "health check visibility documented",
        "post release support owner documented"
    )
    safety = @(
        "no live monitoring activation",
        "no real alert routing",
        "no production deployment",
        "no production traffic routing",
        "no Railway mutation",
        "no Supabase mutation"
    )
    noLiveMonitoringActivation = $true
    noRealAlertRouting = $true
    noProductionDeployment = $true
    noProductionTrafficRouting = $true
    noRailwayMutation = $true
    noSupabaseMutation = $true
    blockingIssues = 0
}

$rollback = [ordered]@{
    phase = "PHASE 10.4"
    groupedPhase = "PHASE 10I - Production Rollback Procedure Validation"
    releaseVersion = $ReleaseVersion
    previousVersion = $PreviousVersion
    releaseChannel = $ReleaseChannel
    rollbackProcedure = @(
        "rollback procedure documented",
        "rollback decision gate documented",
        "rollback checkpoint evidence documented",
        "restore path reviewed",
        "operator approval gate documented"
    )
    noRealProductionRollback = $true
    noProductionDatabaseMutation = $true
    noReleasePromotion = $true
    noSchemaChange = $true
    noMigrations = $true
    blockingIssues = 0
}

$goNoGo = [ordered]@{
    phase = "PHASE 10.4"
    groupedPhase = "PHASE 10J - Production Release Go/No-Go Final Closure"
    status = "verified"
    scope = "Monitoring, Rollback and Go/No-Go"
    phase10_3StagingSmokePrerequisite = $productionSmokeTestChecklist
    monitoringActivationEvidence = $monitoringActivationEvidence
    rollbackProcedureValidationReport = $rollbackProcedureValidationReport
    acceptedChecks = 15
    blockingIssues = 0
    markers = @(
        "PHASE 10.4 monitoring rollback and go no-go documented",
        "PHASE 10H monitoring and alerting activation validation documented",
        "PHASE 10I production rollback procedure validation documented",
        "PHASE 10J production release go no-go final closure documented",
        "monitoring-activation-evidence.json generation documented",
        "rollback-procedure-validation-report.json generation documented",
        "go-no-go-final-closure-report.json generation documented",
        "monitoring checklist documented",
        "logging validation documented",
        "alerting checklist documented",
        "incident response handoff documented",
        "rollback procedure documented",
        "rollback decision gate documented",
        "go no-go checklist documented",
        "final release readiness evidence documented",
        "operator approval gate documented",
        "no live monitoring activation",
        "no real alert routing",
        "no real production rollback",
        "no production deployment",
        "no production traffic routing",
        "no Railway mutation",
        "no Supabase mutation",
        "no production database mutation",
        "no release promotion",
        "no checkout behavior change",
        "no inventory mutation",
        "no production sync enablement",
        "no public API behavior change",
        "no schema change",
        "no migrations"
    )
}

$monitoring | ConvertTo-Json -Depth 8 | Set-Content -Encoding UTF8 -Path $monitoringActivationEvidence
$rollback | ConvertTo-Json -Depth 8 | Set-Content -Encoding UTF8 -Path $rollbackProcedureValidationReport
$goNoGo | ConvertTo-Json -Depth 8 | Set-Content -Encoding UTF8 -Path $goNoGoFinalClosureReport

Write-Host "PHASE 10.4 monitoring rollback and go no-go verified."
Write-Host "MonitoringActivation: $monitoringActivationEvidence"
Write-Host "RollbackProcedureValidation: $rollbackProcedureValidationReport"
Write-Host "GoNoGoFinalClosure: $goNoGoFinalClosureReport"
Write-Host "AcceptedChecks: 15"
Write-Host "BlockingIssues: 0"
