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

Assert-FileContains "VERIFY_PHASE_11_4_UPDATED.ps1" "PHASE 11.4 markers verified."
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidationClosure.cs" "PosFunctionalBusinessValidationClosure"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidationClosure.cs" "PHASE 11 POS functional business validation closure documented"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidationClosure.cs" "PHASE 11.1 cashier shift and sales flow closed"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidationClosure.cs" "PHASE 11.2 payments receipts and returns closed"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidationClosure.cs" "PHASE 11.3 inventory stock movement and offline sync closed"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidationClosure.cs" "PHASE 11.4 hardware readiness and store pilot checklist closed"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidationClosure.cs" "605 tests passed source evidence documented"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidationClosure.cs" "620 tests expected after PHASE 11 final closure documented"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidationClosure.cs" "functional-business-closure-evidence.json generation documented"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidationClosure.cs" "functional-business-readiness-scorecard.json generation documented"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidationClosure.cs" "store-pilot-entry-decision-report.json generation documented"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidationClosure.cs" "phase11-final-closure-summary.json generation documented"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidationClosure.cs" "cashier shift opening flow accepted"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidationClosure.cs" "payment method validation accepted"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidationClosure.cs" "offline sync readiness accepted"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidationClosure.cs" "store pilot rehearsal accepted"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidationClosure.cs" "no checkout real"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidationClosure.cs" "no payment capture"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidationClosure.cs" "no receipt printing"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidationClosure.cs" "no refund execution"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidationClosure.cs" "no real inventory mutation"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidationClosure.cs" "no hardware access"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidationClosure.cs" "no store pilot activation"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidationClosure.cs" "no production sync enablement"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidationClosure.cs" "no public API behavior change"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidationClosure.cs" "no schema change"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidationClosure.cs" "no migrations"
Assert-FileContains "scripts\release\Validate-Phase11FunctionalBusinessClosure.ps1" "param("
Assert-FileContains "scripts\release\Validate-Phase11FunctionalBusinessClosure.ps1" "Validate-Phase11HardwareReadinessStorePilotValidation.ps1"
Assert-FileContains "scripts\release\Validate-Phase11FunctionalBusinessClosure.ps1" "PHASE 11.4 hardware readiness store pilot outputs are missing"
Assert-FileContains "scripts\release\Validate-Phase11FunctionalBusinessClosure.ps1" "functional-business-closure-evidence.json"
Assert-FileContains "scripts\release\Validate-Phase11FunctionalBusinessClosure.ps1" "functional-business-readiness-scorecard.json"
Assert-FileContains "scripts\release\Validate-Phase11FunctionalBusinessClosure.ps1" "store-pilot-entry-decision-report.json"
Assert-FileContains "scripts\release\Validate-Phase11FunctionalBusinessClosure.ps1" "phase11-final-closure-summary.json"
Assert-FileContains "scripts\release\Validate-Phase11FunctionalBusinessClosure.ps1" "READY_FOR_CONTROLLED_STORE_PILOT_AFTER_MANUAL_OPERATOR_APPROVAL"
Assert-FileContains "scripts\release\Validate-Phase11FunctionalBusinessClosure.ps1" "PHASE 11 POS functional business validation closure verified."
Assert-FileContains "scripts\release\Validate-Phase11FunctionalBusinessClosure.ps1" "AcceptedChecks: 15"
Assert-FileContains "scripts\release\Validate-Phase11FunctionalBusinessClosure.ps1" "BlockingIssues: 0"
Assert-FileContains "docs\POS_FUNCTIONAL_BUSINESS_VALIDATION_CLOSURE.md" "PHASE 11 POS functional business validation closure documented"
Assert-FileContains "docs\PHASE_11_FINAL_FUNCTIONAL_BUSINESS_VALIDATION_CLOSURE.md" "605 tests passed"
Assert-FileContains "docs\PHASE_11_FINAL_FUNCTIONAL_BUSINESS_VALIDATION_CLOSURE.md" "620 tests passed"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_11_FINAL.md" "Functional business validation advanced from 0% to 100%"
Assert-FileContains "README.md" "PHASE 11 FINAL"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "PHASE 11 FINAL"

Write-Host "PHASE 11 FINAL markers verified."
