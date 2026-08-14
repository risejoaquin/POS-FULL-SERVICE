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

Assert-FileContains "PosCore\Security\PosReleaseNotesOperatorHandoffBaseline.cs" "PosReleaseNotesOperatorHandoffBaseline"
Assert-FileContains "PosCore\Security\PosReleaseNotesOperatorHandoffBaseline.cs" "POS Release Notes and Operator Handoff Baseline"
Assert-FileContains "PosCore\Security\PosReleaseNotesOperatorHandoffBaseline.cs" "release notes and operator handoff baseline documented"
Assert-FileContains "PosCore\Security\PosReleaseNotesOperatorHandoffBaseline.cs" "PHASE 8E installer readiness prerequisite documented"
Assert-FileContains "PosCore\Security\PosReleaseNotesOperatorHandoffBaseline.cs" "415 tests passed source evidence documented"
Assert-FileContains "PosCore\Security\PosReleaseNotesOperatorHandoffBaseline.cs" "420 tests expected after release notes handoff baseline documented"
Assert-FileContains "PosCore\Security\PosReleaseNotesOperatorHandoffBaseline.cs" "release notes audience documented"
Assert-FileContains "PosCore\Security\PosReleaseNotesOperatorHandoffBaseline.cs" "release summary checklist documented"
Assert-FileContains "PosCore\Security\PosReleaseNotesOperatorHandoffBaseline.cs" "known limitations checklist documented"
Assert-FileContains "PosCore\Security\PosReleaseNotesOperatorHandoffBaseline.cs" "operator handoff checklist documented"
Assert-FileContains "PosCore\Security\PosReleaseNotesOperatorHandoffBaseline.cs" "support escalation path documented"
Assert-FileContains "PosCore\Security\PosReleaseNotesOperatorHandoffBaseline.cs" "rollback communication checklist documented"
Assert-FileContains "PosCore\Security\PosReleaseNotesOperatorHandoffBaseline.cs" "smoke test results handoff documented"
Assert-FileContains "PosCore\Security\PosReleaseNotesOperatorHandoffBaseline.cs" "artifact manifest handoff documented"
Assert-FileContains "PosCore\Security\PosReleaseNotesOperatorHandoffBaseline.cs" "installer readiness handoff documented"
Assert-FileContains "PosCore\Security\PosReleaseNotesOperatorHandoffBaseline.cs" "monitoring handoff documented"
Assert-FileContains "PosCore\Security\PosReleaseNotesOperatorHandoffBaseline.cs" "go no go handoff checklist documented"
Assert-FileContains "PosCore\Security\PosReleaseNotesOperatorHandoffBaseline.cs" "release owner approval checklist documented"
Assert-FileContains "PosCore\Security\PosReleaseNotesOperatorHandoffBaseline.cs" "post release support window documented"
Assert-FileContains "PosCore\Security\PosReleaseNotesOperatorHandoffBaseline.cs" "operator evidence archive checklist documented"
Assert-FileContains "PosCore\Security\PosReleaseNotesOperatorHandoffBaseline.cs" "no checkout behavior change"
Assert-FileContains "PosCore\Security\PosReleaseNotesOperatorHandoffBaseline.cs" "no inventory mutation"
Assert-FileContains "PosCore\Security\PosReleaseNotesOperatorHandoffBaseline.cs" "no production sync enablement"
Assert-FileContains "PosCore\Security\PosReleaseNotesOperatorHandoffBaseline.cs" "no packaging execution"
Assert-FileContains "PosCore\Security\PosReleaseNotesOperatorHandoffBaseline.cs" "no installer execution"
Assert-FileContains "PosCore\Security\PosReleaseNotesOperatorHandoffBaseline.cs" "no deployment execution"
Assert-FileContains "PosCore\Security\PosReleaseNotesOperatorHandoffBaseline.cs" "no schema change"
Assert-FileContains "PosCore\Security\PosReleaseNotesOperatorHandoffBaseline.cs" "no migrations"
Assert-FileContains "docs\POS_RELEASE_NOTES_OPERATOR_HANDOFF_BASELINE.md" "release notes and operator handoff baseline documented"
Assert-FileContains "docs\POS_RELEASE_NOTES_OPERATOR_HANDOFF_BASELINE.md" "PHASE 8E installer readiness prerequisite documented"
Assert-FileContains "docs\POS_RELEASE_NOTES_OPERATOR_HANDOFF_BASELINE.md" "release summary checklist documented"
Assert-FileContains "docs\POS_RELEASE_NOTES_OPERATOR_HANDOFF_BASELINE.md" "known limitations checklist documented"
Assert-FileContains "docs\POS_RELEASE_NOTES_OPERATOR_HANDOFF_BASELINE.md" "operator handoff checklist documented"
Assert-FileContains "docs\POS_RELEASE_NOTES_OPERATOR_HANDOFF_BASELINE.md" "support escalation path documented"
Assert-FileContains "docs\POS_RELEASE_NOTES_OPERATOR_HANDOFF_BASELINE.md" "rollback communication checklist documented"
Assert-FileContains "docs\POS_RELEASE_NOTES_OPERATOR_HANDOFF_BASELINE.md" "smoke test results handoff documented"
Assert-FileContains "docs\PHASE_8F_RELEASE_NOTES_OPERATOR_HANDOFF_BASELINE.md" "420 tests passed"
Assert-FileContains "docs\PHASE_8F_RELEASE_NOTES_OPERATOR_HANDOFF_BASELINE.md" "0 Advertencia(s)"
Assert-FileContains "docs\PHASE_8F_RELEASE_NOTES_OPERATOR_HANDOFF_BASELINE.md" "0 Errores"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_8F.md" "50% -> 60%"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_8F.md" "Release Packaging and Operational Readiness"
Assert-FileContains "README.md" "PHASE 8F"
Assert-FileContains "README.md" "Release Notes and Operator Handoff Baseline"
Assert-FileContains "README.md" "420 tests passed"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "PHASE 8F"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "Release Notes and Operator Handoff Baseline"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "Release Packaging and Operational Readiness: 50% -> 60%"
Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "PosReleaseNotesOperatorHandoffBaseline_Should_Define_Release_Notes_Operator_Handoff_Checks"
Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "Phase8F_Documentation_Should_Describe_Release_Notes_Operator_Handoff_Baseline"
Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "VerifyPhase8F_Should_Require_Release_Notes_Operator_Handoff_Markers"

Write-Host "PHASE 8F markers verified."
