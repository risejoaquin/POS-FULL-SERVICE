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

Assert-FileContains "PosCore\Security\PosReleaseArtifactInventoryPackagingBaseline.cs" "PosReleaseArtifactInventoryPackagingBaseline"
Assert-FileContains "PosCore\Security\PosReleaseArtifactInventoryPackagingBaseline.cs" "POS Release Artifact Inventory and Packaging Baseline"
Assert-FileContains "PosCore\Security\PosReleaseArtifactInventoryPackagingBaseline.cs" "release artifact inventory and packaging baseline documented"
Assert-FileContains "PosCore\Security\PosReleaseArtifactInventoryPackagingBaseline.cs" "PHASE 8A production readiness prerequisite documented"
Assert-FileContains "PosCore\Security\PosReleaseArtifactInventoryPackagingBaseline.cs" "395 tests passed source evidence documented"
Assert-FileContains "PosCore\Security\PosReleaseArtifactInventoryPackagingBaseline.cs" "400 tests expected after packaging baseline verification documented"
Assert-FileContains "PosCore\Security\PosReleaseArtifactInventoryPackagingBaseline.cs" "PosCore release artifact listed"
Assert-FileContains "PosCore\Security\PosReleaseArtifactInventoryPackagingBaseline.cs" "PosBuilder release artifact listed"
Assert-FileContains "PosCore\Security\PosReleaseArtifactInventoryPackagingBaseline.cs" "PosServer release artifact listed"
Assert-FileContains "PosCore\Security\PosReleaseArtifactInventoryPackagingBaseline.cs" "checksum manifest checklist documented"
Assert-FileContains "PosCore\Security\PosReleaseArtifactInventoryPackagingBaseline.cs" "version stamp checklist documented"
Assert-FileContains "PosCore\Security\PosReleaseArtifactInventoryPackagingBaseline.cs" "no checkout behavior change"
Assert-FileContains "PosCore\Security\PosReleaseArtifactInventoryPackagingBaseline.cs" "no inventory mutation"
Assert-FileContains "PosCore\Security\PosReleaseArtifactInventoryPackagingBaseline.cs" "no production sync enablement"
Assert-FileContains "PosCore\Security\PosReleaseArtifactInventoryPackagingBaseline.cs" "no packaging execution"
Assert-FileContains "PosCore\Security\PosReleaseArtifactInventoryPackagingBaseline.cs" "no installer execution"
Assert-FileContains "PosCore\Security\PosReleaseArtifactInventoryPackagingBaseline.cs" "no deployment execution"
Assert-FileContains "PosCore\Security\PosReleaseArtifactInventoryPackagingBaseline.cs" "no schema change"
Assert-FileContains "PosCore\Security\PosReleaseArtifactInventoryPackagingBaseline.cs" "no migrations"

Assert-FileContains "docs\POS_RELEASE_ARTIFACT_INVENTORY_PACKAGING_BASELINE.md" "release artifact inventory and packaging baseline documented"
Assert-FileContains "docs\POS_RELEASE_ARTIFACT_INVENTORY_PACKAGING_BASELINE.md" "PHASE 8A production readiness prerequisite documented"
Assert-FileContains "docs\POS_RELEASE_ARTIFACT_INVENTORY_PACKAGING_BASELINE.md" "PosCore release artifact listed"
Assert-FileContains "docs\POS_RELEASE_ARTIFACT_INVENTORY_PACKAGING_BASELINE.md" "PosBuilder release artifact listed"
Assert-FileContains "docs\POS_RELEASE_ARTIFACT_INVENTORY_PACKAGING_BASELINE.md" "PosServer release artifact listed"
Assert-FileContains "docs\POS_RELEASE_ARTIFACT_INVENTORY_PACKAGING_BASELINE.md" "checksum manifest checklist documented"
Assert-FileContains "docs\PHASE_8B_RELEASE_ARTIFACT_INVENTORY_PACKAGING_BASELINE.md" "400 tests passed"
Assert-FileContains "docs\PHASE_8B_RELEASE_ARTIFACT_INVENTORY_PACKAGING_BASELINE.md" "0 Advertencia(s)"
Assert-FileContains "docs\PHASE_8B_RELEASE_ARTIFACT_INVENTORY_PACKAGING_BASELINE.md" "0 Errores"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_8B.md" "10% -> 20%"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_8B.md" "Release Packaging and Operational Readiness"

Assert-FileContains "README.md" "PHASE 8B"
Assert-FileContains "README.md" "Release Artifact Inventory and Packaging Baseline"
Assert-FileContains "README.md" "400 tests passed"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "PHASE 8B"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "Release Artifact Inventory and Packaging Baseline"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "Release Packaging and Operational Readiness: 10% -> 20%"
Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "PosReleaseArtifactInventoryPackagingBaseline_Should_Define_Artifact_Inventory_And_Packaging_Checks"
Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "Phase8B_Documentation_Should_Describe_Artifact_Inventory_And_Packaging_Baseline"
Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "VerifyPhase8B_Should_Require_Release_Artifact_Inventory_Packaging_Markers"

Write-Host "PHASE 8B markers verified."
