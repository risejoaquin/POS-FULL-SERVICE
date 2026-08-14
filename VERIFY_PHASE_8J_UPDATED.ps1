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

Assert-FileContains "PosCore\Security\PosReleaseGoNoGoOperationalReadinessClosure.cs" "PosReleaseGoNoGoOperationalReadinessClosure"
Assert-FileContains "PosCore\Security\PosReleaseGoNoGoOperationalReadinessClosure.cs" "POS Release Go No-Go and Operational Readiness Closure"
Assert-FileContains "PosCore\Security\PosReleaseGoNoGoOperationalReadinessClosure.cs" "release go no-go and operational readiness closure documented"
Assert-FileContains "PosCore\Security\PosReleaseGoNoGoOperationalReadinessClosure.cs" "PHASE 8I monitoring post-release support prerequisite documented"
Assert-FileContains "PosCore\Security\PosReleaseGoNoGoOperationalReadinessClosure.cs" "435 tests passed source evidence documented"
Assert-FileContains "PosCore\Security\PosReleaseGoNoGoOperationalReadinessClosure.cs" "440 tests expected after release go no-go closure documented"
Assert-FileContains "PosCore\Security\PosReleaseGoNoGoOperationalReadinessClosure.cs" "release candidate validation evidence reviewed"
Assert-FileContains "PosCore\Security\PosReleaseGoNoGoOperationalReadinessClosure.cs" "artifact inventory evidence reviewed"
Assert-FileContains "PosCore\Security\PosReleaseGoNoGoOperationalReadinessClosure.cs" "versioning release manifest evidence reviewed"
Assert-FileContains "PosCore\Security\PosReleaseGoNoGoOperationalReadinessClosure.cs" "checksum verification evidence reviewed"
Assert-FileContains "PosCore\Security\PosReleaseGoNoGoOperationalReadinessClosure.cs" "installer readiness evidence reviewed"
Assert-FileContains "PosCore\Security\PosReleaseGoNoGoOperationalReadinessClosure.cs" "release notes handoff evidence reviewed"
Assert-FileContains "PosCore\Security\PosReleaseGoNoGoOperationalReadinessClosure.cs" "smoke test evidence reviewed"
Assert-FileContains "PosCore\Security\PosReleaseGoNoGoOperationalReadinessClosure.cs" "rollback drill evidence reviewed"
Assert-FileContains "PosCore\Security\PosReleaseGoNoGoOperationalReadinessClosure.cs" "monitoring support evidence reviewed"
Assert-FileContains "PosCore\Security\PosReleaseGoNoGoOperationalReadinessClosure.cs" "go decision checklist documented"
Assert-FileContains "PosCore\Security\PosReleaseGoNoGoOperationalReadinessClosure.cs" "no-go decision checklist documented"
Assert-FileContains "PosCore\Security\PosReleaseGoNoGoOperationalReadinessClosure.cs" "operational readiness closure checklist documented"
Assert-FileContains "PosCore\Security\PosReleaseGoNoGoOperationalReadinessClosure.cs" "release owner signoff checklist documented"
Assert-FileContains "PosCore\Security\PosReleaseGoNoGoOperationalReadinessClosure.cs" "support owner signoff checklist documented"
Assert-FileContains "PosCore\Security\PosReleaseGoNoGoOperationalReadinessClosure.cs" "rollback owner signoff checklist documented"
Assert-FileContains "PosCore\Security\PosReleaseGoNoGoOperationalReadinessClosure.cs" "PHASE 8 closure evidence documented"
Assert-FileContains "PosCore\Security\PosReleaseGoNoGoOperationalReadinessClosure.cs" "no checkout behavior change"
Assert-FileContains "PosCore\Security\PosReleaseGoNoGoOperationalReadinessClosure.cs" "no inventory mutation"
Assert-FileContains "PosCore\Security\PosReleaseGoNoGoOperationalReadinessClosure.cs" "no production sync enablement"
Assert-FileContains "PosCore\Security\PosReleaseGoNoGoOperationalReadinessClosure.cs" "no packaging execution"
Assert-FileContains "PosCore\Security\PosReleaseGoNoGoOperationalReadinessClosure.cs" "no installer execution"
Assert-FileContains "PosCore\Security\PosReleaseGoNoGoOperationalReadinessClosure.cs" "no deployment execution"
Assert-FileContains "PosCore\Security\PosReleaseGoNoGoOperationalReadinessClosure.cs" "no schema change"
Assert-FileContains "PosCore\Security\PosReleaseGoNoGoOperationalReadinessClosure.cs" "no migrations"
Assert-FileContains "docs\POS_RELEASE_GO_NO_GO_OPERATIONAL_READINESS_CLOSURE.md" "release go no-go and operational readiness closure documented"
Assert-FileContains "docs\POS_RELEASE_GO_NO_GO_OPERATIONAL_READINESS_CLOSURE.md" "go decision checklist documented"
Assert-FileContains "docs\POS_RELEASE_GO_NO_GO_OPERATIONAL_READINESS_CLOSURE.md" "PHASE 8 closure evidence documented"
Assert-FileContains "docs\PHASE_8J_RELEASE_GO_NO_GO_OPERATIONAL_READINESS_CLOSURE.md" "435 tests passed"
Assert-FileContains "docs\PHASE_8J_RELEASE_GO_NO_GO_OPERATIONAL_READINESS_CLOSURE.md" "440 tests passed"
Assert-FileContains "docs\PHASE_8J_RELEASE_GO_NO_GO_OPERATIONAL_READINESS_CLOSURE.md" "0 Advertencia(s)"
Assert-FileContains "docs\PHASE_8J_RELEASE_GO_NO_GO_OPERATIONAL_READINESS_CLOSURE.md" "0 Errores"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_8J.md" "90% -> 100%"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_8J.md" "Release Packaging and Operational Readiness"
Assert-FileContains "README.md" "PHASE 8J"
Assert-FileContains "README.md" "Release Go No-Go and Operational Readiness Closure"
Assert-FileContains "README.md" "440 tests passed"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "PHASE 8J"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "Release Go No-Go and Operational Readiness Closure"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "Release Packaging and Operational Readiness: 90% -> 100%"
Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "PosReleaseGoNoGoOperationalReadinessClosure_Should_Define_Release_Go_NoGo_Closure_Checks"
Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "Phase8J_Documentation_Should_Describe_Release_Go_NoGo_Operational_Readiness_Closure"
Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "VerifyPhase8J_Should_Require_Release_Go_NoGo_Operational_Readiness_Closure_Markers"

Write-Host "PHASE 8J markers verified."
