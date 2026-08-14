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

Assert-FileContains "PosCore\Security\PosChecksumArtifactVerificationBaseline.cs" "PosChecksumArtifactVerificationBaseline"
Assert-FileContains "PosCore\Security\PosChecksumArtifactVerificationBaseline.cs" "POS Checksum and Artifact Verification Baseline"
Assert-FileContains "PosCore\Security\PosChecksumArtifactVerificationBaseline.cs" "checksum and artifact verification baseline documented"
Assert-FileContains "PosCore\Security\PosChecksumArtifactVerificationBaseline.cs" "PHASE 8C versioning release manifest prerequisite documented"
Assert-FileContains "PosCore\Security\PosChecksumArtifactVerificationBaseline.cs" "405 tests passed source evidence documented"
Assert-FileContains "PosCore\Security\PosChecksumArtifactVerificationBaseline.cs" "410 tests expected after checksum verification baseline documented"
Assert-FileContains "PosCore\Security\PosChecksumArtifactVerificationBaseline.cs" "sha256 checksum algorithm documented"
Assert-FileContains "PosCore\Security\PosChecksumArtifactVerificationBaseline.cs" "artifact checksum generation command documented"
Assert-FileContains "PosCore\Security\PosChecksumArtifactVerificationBaseline.cs" "artifact checksum verification command documented"
Assert-FileContains "PosCore\Security\PosChecksumArtifactVerificationBaseline.cs" "manifest checksum cross-check documented"
Assert-FileContains "PosCore\Security\PosChecksumArtifactVerificationBaseline.cs" "artifact tamper detection checklist documented"
Assert-FileContains "PosCore\Security\PosChecksumArtifactVerificationBaseline.cs" "artifact path existence verification documented"
Assert-FileContains "PosCore\Security\PosChecksumArtifactVerificationBaseline.cs" "artifact size verification documented"
Assert-FileContains "PosCore\Security\PosChecksumArtifactVerificationBaseline.cs" "artifact version match verification documented"
Assert-FileContains "PosCore\Security\PosChecksumArtifactVerificationBaseline.cs" "release manifest checksum linkage documented"
Assert-FileContains "PosCore\Security\PosChecksumArtifactVerificationBaseline.cs" "operator checksum review checklist documented"
Assert-FileContains "PosCore\Security\PosChecksumArtifactVerificationBaseline.cs" "checksum failure handling checklist documented"
Assert-FileContains "PosCore\Security\PosChecksumArtifactVerificationBaseline.cs" "artifact verification audit evidence documented"
Assert-FileContains "PosCore\Security\PosChecksumArtifactVerificationBaseline.cs" "no checkout behavior change"
Assert-FileContains "PosCore\Security\PosChecksumArtifactVerificationBaseline.cs" "no inventory mutation"
Assert-FileContains "PosCore\Security\PosChecksumArtifactVerificationBaseline.cs" "no production sync enablement"
Assert-FileContains "PosCore\Security\PosChecksumArtifactVerificationBaseline.cs" "no packaging execution"
Assert-FileContains "PosCore\Security\PosChecksumArtifactVerificationBaseline.cs" "no installer execution"
Assert-FileContains "PosCore\Security\PosChecksumArtifactVerificationBaseline.cs" "no deployment execution"
Assert-FileContains "PosCore\Security\PosChecksumArtifactVerificationBaseline.cs" "no schema change"
Assert-FileContains "PosCore\Security\PosChecksumArtifactVerificationBaseline.cs" "no migrations"

Assert-FileContains "docs\POS_CHECKSUM_ARTIFACT_VERIFICATION_BASELINE.md" "checksum and artifact verification baseline documented"
Assert-FileContains "docs\POS_CHECKSUM_ARTIFACT_VERIFICATION_BASELINE.md" "PHASE 8C versioning release manifest prerequisite documented"
Assert-FileContains "docs\POS_CHECKSUM_ARTIFACT_VERIFICATION_BASELINE.md" "sha256 checksum algorithm documented"
Assert-FileContains "docs\POS_CHECKSUM_ARTIFACT_VERIFICATION_BASELINE.md" "artifact checksum generation command documented"
Assert-FileContains "docs\POS_CHECKSUM_ARTIFACT_VERIFICATION_BASELINE.md" "artifact checksum verification command documented"
Assert-FileContains "docs\POS_CHECKSUM_ARTIFACT_VERIFICATION_BASELINE.md" "manifest checksum cross-check documented"
Assert-FileContains "docs\PHASE_8D_CHECKSUM_ARTIFACT_VERIFICATION_BASELINE.md" "410 tests passed"
Assert-FileContains "docs\PHASE_8D_CHECKSUM_ARTIFACT_VERIFICATION_BASELINE.md" "0 Advertencia(s)"
Assert-FileContains "docs\PHASE_8D_CHECKSUM_ARTIFACT_VERIFICATION_BASELINE.md" "0 Errores"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_8D.md" "30% -> 40%"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_8D.md" "Release Packaging and Operational Readiness"

Assert-FileContains "README.md" "PHASE 8D"
Assert-FileContains "README.md" "Checksum and Artifact Verification Baseline"
Assert-FileContains "README.md" "410 tests passed"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "PHASE 8D"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "Checksum and Artifact Verification Baseline"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "Release Packaging and Operational Readiness: 30% -> 40%"
Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "PosChecksumArtifactVerificationBaseline_Should_Define_Checksum_Artifact_Verification_Checks"
Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "Phase8D_Documentation_Should_Describe_Checksum_Artifact_Verification_Baseline"
Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "VerifyPhase8D_Should_Require_Checksum_Artifact_Verification_Markers"

Write-Host "PHASE 8D markers verified."
