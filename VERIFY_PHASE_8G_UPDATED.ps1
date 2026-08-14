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

Assert-FileContains "PosCore\Security\PosSmokeTestReleaseCandidateValidationBaseline.cs" "PosSmokeTestReleaseCandidateValidationBaseline"
Assert-FileContains "PosCore\Security\PosSmokeTestReleaseCandidateValidationBaseline.cs" "POS Smoke Test and Release Candidate Validation Baseline"
Assert-FileContains "PosCore\Security\PosSmokeTestReleaseCandidateValidationBaseline.cs" "smoke test and release candidate validation baseline documented"
Assert-FileContains "PosCore\Security\PosSmokeTestReleaseCandidateValidationBaseline.cs" "PHASE 8F release notes operator handoff prerequisite documented"
Assert-FileContains "PosCore\Security\PosSmokeTestReleaseCandidateValidationBaseline.cs" "420 tests passed source evidence documented"
Assert-FileContains "PosCore\Security\PosSmokeTestReleaseCandidateValidationBaseline.cs" "425 tests expected after smoke test release candidate baseline documented"
Assert-FileContains "PosCore\Security\PosSmokeTestReleaseCandidateValidationBaseline.cs" "release candidate identifier documented"
Assert-FileContains "PosCore\Security\PosSmokeTestReleaseCandidateValidationBaseline.cs" "release candidate build source documented"
Assert-FileContains "PosCore\Security\PosSmokeTestReleaseCandidateValidationBaseline.cs" "clean release build prerequisite documented"
Assert-FileContains "PosCore\Security\PosSmokeTestReleaseCandidateValidationBaseline.cs" "zero warning prerequisite documented"
Assert-FileContains "PosCore\Security\PosSmokeTestReleaseCandidateValidationBaseline.cs" "smoke test environment checklist documented"
Assert-FileContains "PosCore\Security\PosSmokeTestReleaseCandidateValidationBaseline.cs" "application startup smoke test documented"
Assert-FileContains "PosCore\Security\PosSmokeTestReleaseCandidateValidationBaseline.cs" "authentication smoke test documented"
Assert-FileContains "PosCore\Security\PosSmokeTestReleaseCandidateValidationBaseline.cs" "tenant context smoke test documented"
Assert-FileContains "PosCore\Security\PosSmokeTestReleaseCandidateValidationBaseline.cs" "offline mode smoke test documented"
Assert-FileContains "PosCore\Security\PosSmokeTestReleaseCandidateValidationBaseline.cs" "sync readiness smoke test documented"
Assert-FileContains "PosCore\Security\PosSmokeTestReleaseCandidateValidationBaseline.cs" "artifact manifest smoke test linkage documented"
Assert-FileContains "PosCore\Security\PosSmokeTestReleaseCandidateValidationBaseline.cs" "installer readiness smoke test linkage documented"
Assert-FileContains "PosCore\Security\PosSmokeTestReleaseCandidateValidationBaseline.cs" "release candidate go no go checklist documented"
Assert-FileContains "PosCore\Security\PosSmokeTestReleaseCandidateValidationBaseline.cs" "release candidate failure handling checklist documented"
Assert-FileContains "PosCore\Security\PosSmokeTestReleaseCandidateValidationBaseline.cs" "operator smoke test evidence archive documented"
Assert-FileContains "PosCore\Security\PosSmokeTestReleaseCandidateValidationBaseline.cs" "no checkout behavior change"
Assert-FileContains "PosCore\Security\PosSmokeTestReleaseCandidateValidationBaseline.cs" "no inventory mutation"
Assert-FileContains "PosCore\Security\PosSmokeTestReleaseCandidateValidationBaseline.cs" "no production sync enablement"
Assert-FileContains "PosCore\Security\PosSmokeTestReleaseCandidateValidationBaseline.cs" "no packaging execution"
Assert-FileContains "PosCore\Security\PosSmokeTestReleaseCandidateValidationBaseline.cs" "no installer execution"
Assert-FileContains "PosCore\Security\PosSmokeTestReleaseCandidateValidationBaseline.cs" "no deployment execution"
Assert-FileContains "PosCore\Security\PosSmokeTestReleaseCandidateValidationBaseline.cs" "no schema change"
Assert-FileContains "PosCore\Security\PosSmokeTestReleaseCandidateValidationBaseline.cs" "no migrations"
Assert-FileContains "docs\POS_SMOKE_TEST_RELEASE_CANDIDATE_VALIDATION_BASELINE.md" "smoke test and release candidate validation baseline documented"
Assert-FileContains "docs\POS_SMOKE_TEST_RELEASE_CANDIDATE_VALIDATION_BASELINE.md" "PHASE 8F release notes operator handoff prerequisite documented"
Assert-FileContains "docs\POS_SMOKE_TEST_RELEASE_CANDIDATE_VALIDATION_BASELINE.md" "release candidate identifier documented"
Assert-FileContains "docs\POS_SMOKE_TEST_RELEASE_CANDIDATE_VALIDATION_BASELINE.md" "smoke test environment checklist documented"
Assert-FileContains "docs\POS_SMOKE_TEST_RELEASE_CANDIDATE_VALIDATION_BASELINE.md" "application startup smoke test documented"
Assert-FileContains "docs\POS_SMOKE_TEST_RELEASE_CANDIDATE_VALIDATION_BASELINE.md" "authentication smoke test documented"
Assert-FileContains "docs\POS_SMOKE_TEST_RELEASE_CANDIDATE_VALIDATION_BASELINE.md" "tenant context smoke test documented"
Assert-FileContains "docs\POS_SMOKE_TEST_RELEASE_CANDIDATE_VALIDATION_BASELINE.md" "offline mode smoke test documented"
Assert-FileContains "docs\POS_SMOKE_TEST_RELEASE_CANDIDATE_VALIDATION_BASELINE.md" "sync readiness smoke test documented"
Assert-FileContains "docs\PHASE_8G_SMOKE_TEST_RELEASE_CANDIDATE_VALIDATION_BASELINE.md" "425 tests passed"
Assert-FileContains "docs\PHASE_8G_SMOKE_TEST_RELEASE_CANDIDATE_VALIDATION_BASELINE.md" "0 Advertencia(s)"
Assert-FileContains "docs\PHASE_8G_SMOKE_TEST_RELEASE_CANDIDATE_VALIDATION_BASELINE.md" "0 Errores"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_8G.md" "60% -> 70%"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_8G.md" "Release Packaging and Operational Readiness"
Assert-FileContains "README.md" "PHASE 8G"
Assert-FileContains "README.md" "Smoke Test and Release Candidate Validation Baseline"
Assert-FileContains "README.md" "425 tests passed"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "PHASE 8G"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "Smoke Test and Release Candidate Validation Baseline"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "Release Packaging and Operational Readiness: 60% -> 70%"
Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "PosSmokeTestReleaseCandidateValidationBaseline_Should_Define_Smoke_Test_Release_Candidate_Checks"
Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "Phase8G_Documentation_Should_Describe_Smoke_Test_Release_Candidate_Baseline"
Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "VerifyPhase8G_Should_Require_Smoke_Test_Release_Candidate_Markers"

Write-Host "PHASE 8G markers verified."
