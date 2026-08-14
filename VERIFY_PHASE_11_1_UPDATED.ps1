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

Assert-FileContains "VERIFY_PHASE_10_4_UPDATED.ps1" "PHASE 10.4 markers verified."
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidation.cs" "PosFunctionalBusinessValidation"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidation.cs" "PHASE 11 POS functional business validation documented"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidation.cs" "PHASE 11.1 cashier shift and sales flow validation documented"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidation.cs" "PHASE 11A cashier shift opening validation documented"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidation.cs" "PHASE 11B basic sale flow validation documented"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidation.cs" "PHASE 11C shift closing and reconciliation validation documented"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidation.cs" "PHASE 10.4 production readiness prerequisite documented"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidation.cs" "540 tests passed source evidence documented"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidation.cs" "555 tests expected after cashier shift sales flow validation documented"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidation.cs" "cashier-shift-opening-evidence.json generation documented"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidation.cs" "basic-sale-flow-evidence.json generation documented"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidation.cs" "shift-closing-reconciliation-evidence.json generation documented"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidation.cs" "functional-business-validation-summary.json generation documented"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidation.cs" "open shift workflow documented"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidation.cs" "initial cash drawer balance documented"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidation.cs" "basic sale calculation documented"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidation.cs" "controlled discount application documented"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidation.cs" "payment registration checklist documented"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidation.cs" "shift close workflow documented"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidation.cs" "cash reconciliation checklist documented"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidation.cs" "functional evidence handoff documented"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidation.cs" "no real checkout execution"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidation.cs" "no real payment capture"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidation.cs" "no receipt printing"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidation.cs" "no inventory mutation"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidation.cs" "no hardware access"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidation.cs" "no production sync enablement"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidation.cs" "no public API behavior change"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidation.cs" "no schema change"
Assert-FileContains "PosCore\Security\PosFunctionalBusinessValidation.cs" "no migrations"

Assert-FileContains "scripts\release\Validate-Phase11FunctionalBusinessValidation.ps1" "param("
Assert-FileContains "scripts\release\Validate-Phase11FunctionalBusinessValidation.ps1" "Validate-Phase10MonitoringRollbackGoNoGo.ps1"
Assert-FileContains "scripts\release\Validate-Phase11FunctionalBusinessValidation.ps1" "PHASE 10.4 monitoring rollback go no-go outputs are missing"
Assert-FileContains "scripts\release\Validate-Phase11FunctionalBusinessValidation.ps1" "cashier-shift-opening-evidence.json"
Assert-FileContains "scripts\release\Validate-Phase11FunctionalBusinessValidation.ps1" "basic-sale-flow-evidence.json"
Assert-FileContains "scripts\release\Validate-Phase11FunctionalBusinessValidation.ps1" "shift-closing-reconciliation-evidence.json"
Assert-FileContains "scripts\release\Validate-Phase11FunctionalBusinessValidation.ps1" "functional-business-validation-summary.json"
Assert-FileContains "scripts\release\Validate-Phase11FunctionalBusinessValidation.ps1" "no real checkout execution"
Assert-FileContains "scripts\release\Validate-Phase11FunctionalBusinessValidation.ps1" "no real payment capture"
Assert-FileContains "scripts\release\Validate-Phase11FunctionalBusinessValidation.ps1" "no receipt printing"
Assert-FileContains "scripts\release\Validate-Phase11FunctionalBusinessValidation.ps1" "no inventory mutation"
Assert-FileContains "scripts\release\Validate-Phase11FunctionalBusinessValidation.ps1" "no hardware access"
Assert-FileContains "scripts\release\Validate-Phase11FunctionalBusinessValidation.ps1" "PHASE 11.1 cashier shift and sales flow validation verified."
Assert-FileContains "scripts\release\Validate-Phase11FunctionalBusinessValidation.ps1" "AcceptedChecks: 15"
Assert-FileContains "scripts\release\Validate-Phase11FunctionalBusinessValidation.ps1" "BlockingIssues: 0"

Assert-FileContains "docs\POS_FUNCTIONAL_BUSINESS_VALIDATION.md" "PHASE 11 POS functional business validation documented"
Assert-FileContains "docs\PHASE_11_1_CASHIER_SHIFT_SALES_FLOW_VALIDATION.md" "540 tests passed"
Assert-FileContains "docs\PHASE_11_1_CASHIER_SHIFT_SALES_FLOW_VALIDATION.md" "556 tests passed"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_11_1.md" "Functional business validation advanced from 0% to 25%"
Assert-FileContains "README.md" "PHASE 11.1"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "PHASE 11.1"

Write-Host "PHASE 11.1 markers verified."
