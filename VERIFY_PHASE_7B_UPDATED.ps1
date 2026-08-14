$ErrorActionPreference = "Stop"

function Assert-FileContains {
    param(
        [string]$Path,
        [string]$Text
    )

    if (-not (Test-Path $Path)) {
        throw "Missing required file: $Path"
    }

    $content = Get-Content $Path -Raw
    if ($content -notlike "*$Text*") {
        throw "Missing marker '$Text' in $Path"
    }
}

Assert-FileContains "PosCore\Security\PosNullabilityWarningHardeningBaseline.cs" "PosNullabilityWarningHardeningBaseline"
Assert-FileContains "PosCore\Security\PosNullabilityWarningHardeningBaseline.cs" "POS Nullability Warning Hardening Baseline"
Assert-FileContains "PosCore\Security\PosNullabilityWarningHardeningBaseline.cs" "CS8602 possible null dereference classified"
Assert-FileContains "PosCore\Security\PosNullabilityWarningHardeningBaseline.cs" "CS8601 possible null assignment classified"
Assert-FileContains "PosCore\Security\PosNullabilityWarningHardeningBaseline.cs" "CS8618 non-nullable initialization classified"
Assert-FileContains "PosCore\Security\PosNullabilityWarningHardeningBaseline.cs" "CS8622 delegate nullability mismatch classified"
Assert-FileContains "PosCore\Security\PosNullabilityWarningHardeningBaseline.cs" "CS8600 possible null conversion classified"
Assert-FileContains "PosCore\Security\PosNullabilityWarningHardeningBaseline.cs" "CS8603 possible null return classified"
Assert-FileContains "PosCore\Security\PosNullabilityWarningHardeningBaseline.cs" "no checkout behavior change"
Assert-FileContains "PosCore\Security\PosNullabilityWarningHardeningBaseline.cs" "no inventory mutation"
Assert-FileContains "PosCore\Security\PosNullabilityWarningHardeningBaseline.cs" "no production sync enablement"
Assert-FileContains "PosCore\Security\PosNullabilityWarningHardeningBaseline.cs" "no schema change"
Assert-FileContains "PosCore\Security\PosNullabilityWarningHardeningBaseline.cs" "no migrations"

Assert-FileContains "docs\POS_NULLABILITY_WARNING_HARDENING_BASELINE.md" "POS Nullability Warning Hardening Baseline"
Assert-FileContains "docs\POS_NULLABILITY_WARNING_HARDENING_BASELINE.md" "Server service nullability hotspots documented"
Assert-FileContains "docs\POS_NULLABILITY_WARNING_HARDENING_BASELINE.md" "Builder nullability hotspots documented"
Assert-FileContains "docs\PHASE_7B_NULLABILITY_WARNING_HARDENING_BASELINE.md" "PHASE 7B"
Assert-FileContains "docs\PHASE_7B_NULLABILITY_WARNING_HARDENING_BASELINE.md" "350 tests passed"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_7B.md" "10% -> 20%"

Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "PosNullabilityWarningHardeningBaseline_Should_Define_Warning_Classification_Checks"
Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "PHASE 7B markers verified."
Assert-FileContains "README.md" "PHASE 7B"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "PHASE 7B"

Write-Host "PHASE 7B markers verified."
