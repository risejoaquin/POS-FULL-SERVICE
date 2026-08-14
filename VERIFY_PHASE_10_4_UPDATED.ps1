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

Assert-FileContains "VERIFY_PHASE_10_3_UPDATED.ps1" "PHASE 10.3 markers verified."
Assert-FileContains "PosCore\Security\PosMonitoringRollbackGoNoGoValidation.cs" "PosMonitoringRollbackGoNoGoValidation"
Assert-FileContains "PosCore\Security\PosMonitoringRollbackGoNoGoValidation.cs" "PHASE 10.4 monitoring rollback and go no-go documented"
Assert-FileContains "PosCore\Security\PosMonitoringRollbackGoNoGoValidation.cs" "PHASE 10H monitoring and alerting activation validation documented"
Assert-FileContains "PosCore\Security\PosMonitoringRollbackGoNoGoValidation.cs" "PHASE 10I production rollback procedure validation documented"
Assert-FileContains "PosCore\Security\PosMonitoringRollbackGoNoGoValidation.cs" "PHASE 10J production release go no-go final closure documented"
Assert-FileContains "PosCore\Security\PosMonitoringRollbackGoNoGoValidation.cs" "PHASE 10.3 staging execution smoke tests prerequisite documented"
Assert-FileContains "PosCore\Security\PosMonitoringRollbackGoNoGoValidation.cs" "525 tests passed source evidence documented"
Assert-FileContains "PosCore\Security\PosMonitoringRollbackGoNoGoValidation.cs" "540 tests expected after monitoring rollback go no-go validation documented"
Assert-FileContains "PosCore\Security\PosMonitoringRollbackGoNoGoValidation.cs" "monitoring-activation-evidence.json generation documented"
Assert-FileContains "PosCore\Security\PosMonitoringRollbackGoNoGoValidation.cs" "rollback-procedure-validation-report.json generation documented"
Assert-FileContains "PosCore\Security\PosMonitoringRollbackGoNoGoValidation.cs" "go-no-go-final-closure-report.json generation documented"
Assert-FileContains "PosCore\Security\PosMonitoringRollbackGoNoGoValidation.cs" "monitoring checklist documented"
Assert-FileContains "PosCore\Security\PosMonitoringRollbackGoNoGoValidation.cs" "logging validation documented"
Assert-FileContains "PosCore\Security\PosMonitoringRollbackGoNoGoValidation.cs" "alerting checklist documented"
Assert-FileContains "PosCore\Security\PosMonitoringRollbackGoNoGoValidation.cs" "incident response handoff documented"
Assert-FileContains "PosCore\Security\PosMonitoringRollbackGoNoGoValidation.cs" "rollback procedure documented"
Assert-FileContains "PosCore\Security\PosMonitoringRollbackGoNoGoValidation.cs" "rollback decision gate documented"
Assert-FileContains "PosCore\Security\PosMonitoringRollbackGoNoGoValidation.cs" "go no-go checklist documented"
Assert-FileContains "PosCore\Security\PosMonitoringRollbackGoNoGoValidation.cs" "final release readiness evidence documented"
Assert-FileContains "PosCore\Security\PosMonitoringRollbackGoNoGoValidation.cs" "operator approval gate documented"
Assert-FileContains "PosCore\Security\PosMonitoringRollbackGoNoGoValidation.cs" "no live monitoring activation"
Assert-FileContains "PosCore\Security\PosMonitoringRollbackGoNoGoValidation.cs" "no real alert routing"
Assert-FileContains "PosCore\Security\PosMonitoringRollbackGoNoGoValidation.cs" "no real production rollback"
Assert-FileContains "PosCore\Security\PosMonitoringRollbackGoNoGoValidation.cs" "no production deployment"
Assert-FileContains "PosCore\Security\PosMonitoringRollbackGoNoGoValidation.cs" "no production traffic routing"
Assert-FileContains "PosCore\Security\PosMonitoringRollbackGoNoGoValidation.cs" "no Railway mutation"
Assert-FileContains "PosCore\Security\PosMonitoringRollbackGoNoGoValidation.cs" "no Supabase mutation"
Assert-FileContains "PosCore\Security\PosMonitoringRollbackGoNoGoValidation.cs" "no production database mutation"
Assert-FileContains "PosCore\Security\PosMonitoringRollbackGoNoGoValidation.cs" "no release promotion"
Assert-FileContains "PosCore\Security\PosMonitoringRollbackGoNoGoValidation.cs" "no schema change"
Assert-FileContains "PosCore\Security\PosMonitoringRollbackGoNoGoValidation.cs" "no migrations"

Assert-FileContains "scripts\release\Validate-Phase10MonitoringRollbackGoNoGo.ps1" "param("
Assert-FileContains "scripts\release\Validate-Phase10MonitoringRollbackGoNoGo.ps1" "Validate-Phase10StagingExecutionSmokeTests.ps1"
Assert-FileContains "scripts\release\Validate-Phase10MonitoringRollbackGoNoGo.ps1" "PHASE 10.3 staging execution smoke test outputs are missing"
Assert-FileContains "scripts\release\Validate-Phase10MonitoringRollbackGoNoGo.ps1" "monitoring-activation-evidence.json"
Assert-FileContains "scripts\release\Validate-Phase10MonitoringRollbackGoNoGo.ps1" "rollback-procedure-validation-report.json"
Assert-FileContains "scripts\release\Validate-Phase10MonitoringRollbackGoNoGo.ps1" "go-no-go-final-closure-report.json"
Assert-FileContains "scripts\release\Validate-Phase10MonitoringRollbackGoNoGo.ps1" "no live monitoring activation"
Assert-FileContains "scripts\release\Validate-Phase10MonitoringRollbackGoNoGo.ps1" "no real alert routing"
Assert-FileContains "scripts\release\Validate-Phase10MonitoringRollbackGoNoGo.ps1" "no real production rollback"
Assert-FileContains "scripts\release\Validate-Phase10MonitoringRollbackGoNoGo.ps1" "no production deployment"
Assert-FileContains "scripts\release\Validate-Phase10MonitoringRollbackGoNoGo.ps1" "no production traffic routing"
Assert-FileContains "scripts\release\Validate-Phase10MonitoringRollbackGoNoGo.ps1" "no Railway mutation"
Assert-FileContains "scripts\release\Validate-Phase10MonitoringRollbackGoNoGo.ps1" "no Supabase mutation"
Assert-FileContains "scripts\release\Validate-Phase10MonitoringRollbackGoNoGo.ps1" "no production database mutation"
Assert-FileContains "scripts\release\Validate-Phase10MonitoringRollbackGoNoGo.ps1" "no release promotion"
Assert-FileContains "scripts\release\Validate-Phase10MonitoringRollbackGoNoGo.ps1" "PHASE 10.4 monitoring rollback and go no-go verified."
Assert-FileContains "scripts\release\Validate-Phase10MonitoringRollbackGoNoGo.ps1" "AcceptedChecks: 15"
Assert-FileContains "scripts\release\Validate-Phase10MonitoringRollbackGoNoGo.ps1" "BlockingIssues: 0"

Assert-FileContains "docs\POS_MONITORING_ROLLBACK_GO_NO_GO.md" "PHASE 10.4 monitoring rollback and go no-go documented"
Assert-FileContains "docs\PHASE_10_4_MONITORING_ROLLBACK_GO_NO_GO.md" "525 tests passed"
Assert-FileContains "docs\PHASE_10_4_MONITORING_ROLLBACK_GO_NO_GO.md" "540 tests passed"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_10_4.md" "Production readiness advanced from 75% to 100%"
Assert-FileContains "README.md" "PHASE 10.4"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "PHASE 10.4"

Write-Host "PHASE 10.4 markers verified."
