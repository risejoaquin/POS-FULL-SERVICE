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

Assert-FileContains "PosCore\Security\PosRollbackDrillRecoveryEvidenceBaseline.cs" "PosRollbackDrillRecoveryEvidenceBaseline"
Assert-FileContains "PosCore\Security\PosRollbackDrillRecoveryEvidenceBaseline.cs" "POS Rollback Drill and Recovery Evidence Baseline"
Assert-FileContains "PosCore\Security\PosRollbackDrillRecoveryEvidenceBaseline.cs" "rollback drill and recovery evidence baseline documented"
Assert-FileContains "PosCore\Security\PosRollbackDrillRecoveryEvidenceBaseline.cs" "PHASE 8G smoke test release candidate prerequisite documented"
Assert-FileContains "PosCore\Security\PosRollbackDrillRecoveryEvidenceBaseline.cs" "425 tests passed source evidence documented"
Assert-FileContains "PosCore\Security\PosRollbackDrillRecoveryEvidenceBaseline.cs" "430 tests expected after rollback recovery baseline documented"
Assert-FileContains "PosCore\Security\PosRollbackDrillRecoveryEvidenceBaseline.cs" "rollback candidate version documented"
Assert-FileContains "PosCore\Security\PosRollbackDrillRecoveryEvidenceBaseline.cs" "rollback trigger criteria documented"
Assert-FileContains "PosCore\Security\PosRollbackDrillRecoveryEvidenceBaseline.cs" "rollback owner checklist documented"
Assert-FileContains "PosCore\Security\PosRollbackDrillRecoveryEvidenceBaseline.cs" "backup restore prerequisite documented"
Assert-FileContains "PosCore\Security\PosRollbackDrillRecoveryEvidenceBaseline.cs" "database restore verification checklist documented"
Assert-FileContains "PosCore\Security\PosRollbackDrillRecoveryEvidenceBaseline.cs" "configuration restore verification checklist documented"
Assert-FileContains "PosCore\Security\PosRollbackDrillRecoveryEvidenceBaseline.cs" "artifact rollback manifest linkage documented"
Assert-FileContains "PosCore\Security\PosRollbackDrillRecoveryEvidenceBaseline.cs" "installer rollback package linkage documented"
Assert-FileContains "PosCore\Security\PosRollbackDrillRecoveryEvidenceBaseline.cs" "release candidate rollback linkage documented"
Assert-FileContains "PosCore\Security\PosRollbackDrillRecoveryEvidenceBaseline.cs" "smoke test after rollback checklist documented"
Assert-FileContains "PosCore\Security\PosRollbackDrillRecoveryEvidenceBaseline.cs" "data integrity after rollback checklist documented"
Assert-FileContains "PosCore\Security\PosRollbackDrillRecoveryEvidenceBaseline.cs" "support escalation rollback checklist documented"
Assert-FileContains "PosCore\Security\PosRollbackDrillRecoveryEvidenceBaseline.cs" "operator rollback drill evidence archive documented"
Assert-FileContains "PosCore\Security\PosRollbackDrillRecoveryEvidenceBaseline.cs" "rollback failure handling checklist documented"
Assert-FileContains "PosCore\Security\PosRollbackDrillRecoveryEvidenceBaseline.cs" "recovery go no go checklist documented"
Assert-FileContains "PosCore\Security\PosRollbackDrillRecoveryEvidenceBaseline.cs" "no checkout behavior change"
Assert-FileContains "PosCore\Security\PosRollbackDrillRecoveryEvidenceBaseline.cs" "no inventory mutation"
Assert-FileContains "PosCore\Security\PosRollbackDrillRecoveryEvidenceBaseline.cs" "no production sync enablement"
Assert-FileContains "PosCore\Security\PosRollbackDrillRecoveryEvidenceBaseline.cs" "no packaging execution"
Assert-FileContains "PosCore\Security\PosRollbackDrillRecoveryEvidenceBaseline.cs" "no installer execution"
Assert-FileContains "PosCore\Security\PosRollbackDrillRecoveryEvidenceBaseline.cs" "no deployment execution"
Assert-FileContains "PosCore\Security\PosRollbackDrillRecoveryEvidenceBaseline.cs" "no schema change"
Assert-FileContains "PosCore\Security\PosRollbackDrillRecoveryEvidenceBaseline.cs" "no migrations"
Assert-FileContains "docs\POS_ROLLBACK_DRILL_RECOVERY_EVIDENCE_BASELINE.md" "rollback drill and recovery evidence baseline documented"
Assert-FileContains "docs\POS_ROLLBACK_DRILL_RECOVERY_EVIDENCE_BASELINE.md" "PHASE 8G smoke test release candidate prerequisite documented"
Assert-FileContains "docs\POS_ROLLBACK_DRILL_RECOVERY_EVIDENCE_BASELINE.md" "rollback candidate version documented"
Assert-FileContains "docs\POS_ROLLBACK_DRILL_RECOVERY_EVIDENCE_BASELINE.md" "rollback trigger criteria documented"
Assert-FileContains "docs\POS_ROLLBACK_DRILL_RECOVERY_EVIDENCE_BASELINE.md" "database restore verification checklist documented"
Assert-FileContains "docs\POS_ROLLBACK_DRILL_RECOVERY_EVIDENCE_BASELINE.md" "smoke test after rollback checklist documented"
Assert-FileContains "docs\PHASE_8H_ROLLBACK_DRILL_RECOVERY_EVIDENCE_BASELINE.md" "425 tests passed"
Assert-FileContains "docs\PHASE_8H_ROLLBACK_DRILL_RECOVERY_EVIDENCE_BASELINE.md" "430 tests passed"
Assert-FileContains "docs\PHASE_8H_ROLLBACK_DRILL_RECOVERY_EVIDENCE_BASELINE.md" "0 Advertencia(s)"
Assert-FileContains "docs\PHASE_8H_ROLLBACK_DRILL_RECOVERY_EVIDENCE_BASELINE.md" "0 Errores"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_8H.md" "70% -> 80%"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_8H.md" "Release Packaging and Operational Readiness"
Assert-FileContains "README.md" "PHASE 8H"
Assert-FileContains "README.md" "Rollback Drill and Recovery Evidence Baseline"
Assert-FileContains "README.md" "430 tests passed"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "PHASE 8H"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "Rollback Drill and Recovery Evidence Baseline"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "Release Packaging and Operational Readiness: 70% -> 80%"
Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "PosRollbackDrillRecoveryEvidenceBaseline_Should_Define_Rollback_Drill_Recovery_Checks"
Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "Phase8H_Documentation_Should_Describe_Rollback_Drill_Recovery_Baseline"
Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "VerifyPhase8H_Should_Require_Rollback_Drill_Recovery_Markers"

Write-Host "PHASE 8H markers verified."
