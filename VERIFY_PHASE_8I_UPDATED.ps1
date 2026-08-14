$ErrorActionPreference = "Stop"

function Assert-FileContains {
    param(
        [string]$Path,
        [string]$Text
    )

    if (!(Test-Path $Path)) {
        throw "Missing required file: $Path"
    }

    $content = Get-Content $Path -Raw
    if (!$content.Contains($Text)) {
        throw "Missing marker '$Text' in $Path"
    }
}

Assert-FileContains "PosCore\Security\PosMonitoringPostReleaseSupportEvidenceBaseline.cs" "PosMonitoringPostReleaseSupportEvidenceBaseline"
Assert-FileContains "PosCore\Security\PosMonitoringPostReleaseSupportEvidenceBaseline.cs" "POS Monitoring and Post-Release Support Evidence Baseline"
Assert-FileContains "PosCore\Security\PosMonitoringPostReleaseSupportEvidenceBaseline.cs" "monitoring and post-release support evidence baseline documented"
Assert-FileContains "PosCore\Security\PosMonitoringPostReleaseSupportEvidenceBaseline.cs" "PHASE 8H rollback drill recovery prerequisite documented"
Assert-FileContains "PosCore\Security\PosMonitoringPostReleaseSupportEvidenceBaseline.cs" "430 tests passed source evidence documented"
Assert-FileContains "PosCore\Security\PosMonitoringPostReleaseSupportEvidenceBaseline.cs" "435 tests expected after monitoring support baseline documented"
Assert-FileContains "PosCore\Security\PosMonitoringPostReleaseSupportEvidenceBaseline.cs" "release health dashboard checklist documented"
Assert-FileContains "PosCore\Security\PosMonitoringPostReleaseSupportEvidenceBaseline.cs" "application log review checklist documented"
Assert-FileContains "PosCore\Security\PosMonitoringPostReleaseSupportEvidenceBaseline.cs" "error rate monitoring checklist documented"
Assert-FileContains "PosCore\Security\PosMonitoringPostReleaseSupportEvidenceBaseline.cs" "latency monitoring checklist documented"
Assert-FileContains "PosCore\Security\PosMonitoringPostReleaseSupportEvidenceBaseline.cs" "database health monitoring checklist documented"
Assert-FileContains "PosCore\Security\PosMonitoringPostReleaseSupportEvidenceBaseline.cs" "sync health monitoring checklist documented"
Assert-FileContains "PosCore\Security\PosMonitoringPostReleaseSupportEvidenceBaseline.cs" "installer adoption monitoring checklist documented"
Assert-FileContains "PosCore\Security\PosMonitoringPostReleaseSupportEvidenceBaseline.cs" "support triage checklist documented"
Assert-FileContains "PosCore\Security\PosMonitoringPostReleaseSupportEvidenceBaseline.cs" "post release support window documented"
Assert-FileContains "PosCore\Security\PosMonitoringPostReleaseSupportEvidenceBaseline.cs" "incident escalation path documented"
Assert-FileContains "PosCore\Security\PosMonitoringPostReleaseSupportEvidenceBaseline.cs" "rollback watch criteria documented"
Assert-FileContains "PosCore\Security\PosMonitoringPostReleaseSupportEvidenceBaseline.cs" "operator monitoring evidence archive documented"
Assert-FileContains "PosCore\Security\PosMonitoringPostReleaseSupportEvidenceBaseline.cs" "post release go no go continuation checklist documented"
Assert-FileContains "PosCore\Security\PosMonitoringPostReleaseSupportEvidenceBaseline.cs" "no checkout behavior change"
Assert-FileContains "PosCore\Security\PosMonitoringPostReleaseSupportEvidenceBaseline.cs" "no inventory mutation"
Assert-FileContains "PosCore\Security\PosMonitoringPostReleaseSupportEvidenceBaseline.cs" "no production sync enablement"
Assert-FileContains "PosCore\Security\PosMonitoringPostReleaseSupportEvidenceBaseline.cs" "no packaging execution"
Assert-FileContains "PosCore\Security\PosMonitoringPostReleaseSupportEvidenceBaseline.cs" "no installer execution"
Assert-FileContains "PosCore\Security\PosMonitoringPostReleaseSupportEvidenceBaseline.cs" "no deployment execution"
Assert-FileContains "PosCore\Security\PosMonitoringPostReleaseSupportEvidenceBaseline.cs" "no schema change"
Assert-FileContains "PosCore\Security\PosMonitoringPostReleaseSupportEvidenceBaseline.cs" "no migrations"
Assert-FileContains "docs\POS_MONITORING_POST_RELEASE_SUPPORT_EVIDENCE_BASELINE.md" "monitoring and post-release support evidence baseline documented"
Assert-FileContains "docs\POS_MONITORING_POST_RELEASE_SUPPORT_EVIDENCE_BASELINE.md" "PHASE 8H rollback drill recovery prerequisite documented"
Assert-FileContains "docs\POS_MONITORING_POST_RELEASE_SUPPORT_EVIDENCE_BASELINE.md" "release health dashboard checklist documented"
Assert-FileContains "docs\POS_MONITORING_POST_RELEASE_SUPPORT_EVIDENCE_BASELINE.md" "error rate monitoring checklist documented"
Assert-FileContains "docs\POS_MONITORING_POST_RELEASE_SUPPORT_EVIDENCE_BASELINE.md" "sync health monitoring checklist documented"
Assert-FileContains "docs\POS_MONITORING_POST_RELEASE_SUPPORT_EVIDENCE_BASELINE.md" "post release support window documented"
Assert-FileContains "docs\POS_MONITORING_POST_RELEASE_SUPPORT_EVIDENCE_BASELINE.md" "incident escalation path documented"
Assert-FileContains "docs\POS_MONITORING_POST_RELEASE_SUPPORT_EVIDENCE_BASELINE.md" "rollback watch criteria documented"
Assert-FileContains "docs\PHASE_8I_MONITORING_POST_RELEASE_SUPPORT_EVIDENCE_BASELINE.md" "430 tests passed"
Assert-FileContains "docs\PHASE_8I_MONITORING_POST_RELEASE_SUPPORT_EVIDENCE_BASELINE.md" "435 tests passed"
Assert-FileContains "docs\PHASE_8I_MONITORING_POST_RELEASE_SUPPORT_EVIDENCE_BASELINE.md" "0 Advertencia(s)"
Assert-FileContains "docs\PHASE_8I_MONITORING_POST_RELEASE_SUPPORT_EVIDENCE_BASELINE.md" "0 Errores"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_8I.md" "80% -> 90%"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_8I.md" "Release Packaging and Operational Readiness"
Assert-FileContains "README.md" "PHASE 8I"
Assert-FileContains "README.md" "Monitoring and Post-Release Support Evidence Baseline"
Assert-FileContains "README.md" "435 tests passed"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "PHASE 8I"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "Monitoring and Post-Release Support Evidence Baseline"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "Release Packaging and Operational Readiness: 80% -> 90%"
Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "PosMonitoringPostReleaseSupportEvidenceBaseline_Should_Define_Monitoring_Post_Release_Support_Checks"
Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "Phase8I_Documentation_Should_Describe_Monitoring_Post_Release_Support_Baseline"
Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "VerifyPhase8I_Should_Require_Monitoring_Post_Release_Support_Markers"

Write-Host "PHASE 8I markers verified."
