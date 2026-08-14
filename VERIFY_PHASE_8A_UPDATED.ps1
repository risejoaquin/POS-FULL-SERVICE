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

Assert-FileContains "PosCore\Security\PosProductionReadinessOperationalBaseline.cs" "PosProductionReadinessOperationalBaseline"
Assert-FileContains "PosCore\Security\PosProductionReadinessOperationalBaseline.cs" "POS Production Readiness Operational Baseline"
Assert-FileContains "PosCore\Security\PosProductionReadinessOperationalBaseline.cs" "production readiness operational baseline documented"
Assert-FileContains "PosCore\Security\PosProductionReadinessOperationalBaseline.cs" "PHASE 7 zero-warning closure prerequisite documented"
Assert-FileContains "PosCore\Security\PosProductionReadinessOperationalBaseline.cs" "Release build clean prerequisite documented"
Assert-FileContains "PosCore\Security\PosProductionReadinessOperationalBaseline.cs" "390 tests passed source evidence documented"
Assert-FileContains "PosCore\Security\PosProductionReadinessOperationalBaseline.cs" "395 tests expected after baseline verification documented"
Assert-FileContains "PosCore\Security\PosProductionReadinessOperationalBaseline.cs" "environment configuration checklist documented"
Assert-FileContains "PosCore\Security\PosProductionReadinessOperationalBaseline.cs" "database backup and restore validation checklist documented"
Assert-FileContains "PosCore\Security\PosProductionReadinessOperationalBaseline.cs" "rollback procedure checklist documented"
Assert-FileContains "PosCore\Security\PosProductionReadinessOperationalBaseline.cs" "no checkout behavior change"
Assert-FileContains "PosCore\Security\PosProductionReadinessOperationalBaseline.cs" "no inventory mutation"
Assert-FileContains "PosCore\Security\PosProductionReadinessOperationalBaseline.cs" "no production sync enablement"
Assert-FileContains "PosCore\Security\PosProductionReadinessOperationalBaseline.cs" "no packaging execution"
Assert-FileContains "PosCore\Security\PosProductionReadinessOperationalBaseline.cs" "no deployment execution"
Assert-FileContains "PosCore\Security\PosProductionReadinessOperationalBaseline.cs" "no schema change"
Assert-FileContains "PosCore\Security\PosProductionReadinessOperationalBaseline.cs" "no migrations"

Assert-FileContains "docs\POS_PRODUCTION_READINESS_OPERATIONAL_BASELINE.md" "production readiness operational baseline documented"
Assert-FileContains "docs\POS_PRODUCTION_READINESS_OPERATIONAL_BASELINE.md" "PHASE 7 zero-warning closure prerequisite documented"
Assert-FileContains "docs\POS_PRODUCTION_READINESS_OPERATIONAL_BASELINE.md" "environment configuration checklist documented"
Assert-FileContains "docs\POS_PRODUCTION_READINESS_OPERATIONAL_BASELINE.md" "database backup and restore validation checklist documented"
Assert-FileContains "docs\POS_PRODUCTION_READINESS_OPERATIONAL_BASELINE.md" "rollback procedure checklist documented"
Assert-FileContains "docs\PHASE_8A_PRODUCTION_READINESS_OPERATIONAL_BASELINE.md" "395 tests passed"
Assert-FileContains "docs\PHASE_8A_PRODUCTION_READINESS_OPERATIONAL_BASELINE.md" "0 Advertencia(s)"
Assert-FileContains "docs\PHASE_8A_PRODUCTION_READINESS_OPERATIONAL_BASELINE.md" "0 Errores"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_8A.md" "0% -> 10%"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_8A.md" "Release Packaging and Operational Readiness"

Assert-FileContains "README.md" "PHASE 8A"
Assert-FileContains "README.md" "Production Readiness Operational Baseline"
Assert-FileContains "README.md" "395 tests passed"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "PHASE 8A"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "Production Readiness Operational Baseline"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "Release Packaging and Operational Readiness: 0% -> 10%"
Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "PosProductionReadinessOperationalBaseline_Should_Define_Operational_Readiness_Checks"
Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "Phase8A_Documentation_Should_Describe_Operational_Readiness_Baseline"
Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "VerifyPhase8A_Should_Require_Production_Readiness_Markers"

Write-Host "PHASE 8A markers verified."
