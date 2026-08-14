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

Assert-FileContains "VERIFY_PHASE_11_1_UPDATED.ps1" "PHASE 11.1 markers verified."
Assert-FileContains "PosCore\Security\PosPaymentsReceiptsReturnsValidation.cs" "PosPaymentsReceiptsReturnsValidation"
Assert-FileContains "PosCore\Security\PosPaymentsReceiptsReturnsValidation.cs" "PHASE 11.2 payments receipts and returns validation documented"
Assert-FileContains "PosCore\Security\PosPaymentsReceiptsReturnsValidation.cs" "PHASE 11D payment method validation documented"
Assert-FileContains "PosCore\Security\PosPaymentsReceiptsReturnsValidation.cs" "PHASE 11E receipt generation and audit validation documented"
Assert-FileContains "PosCore\Security\PosPaymentsReceiptsReturnsValidation.cs" "PHASE 11F returns and refund workflow validation documented"
Assert-FileContains "PosCore\Security\PosPaymentsReceiptsReturnsValidation.cs" "PHASE 11.1 functional business prerequisite documented"
Assert-FileContains "PosCore\Security\PosPaymentsReceiptsReturnsValidation.cs" "556 tests passed source evidence documented"
Assert-FileContains "PosCore\Security\PosPaymentsReceiptsReturnsValidation.cs" "572 tests expected after payments receipts returns validation documented"
Assert-FileContains "PosCore\Security\PosPaymentsReceiptsReturnsValidation.cs" "payment-method-validation-evidence.json generation documented"
Assert-FileContains "PosCore\Security\PosPaymentsReceiptsReturnsValidation.cs" "receipt-generation-audit-evidence.json generation documented"
Assert-FileContains "PosCore\Security\PosPaymentsReceiptsReturnsValidation.cs" "returns-refund-workflow-evidence.json generation documented"
Assert-FileContains "PosCore\Security\PosPaymentsReceiptsReturnsValidation.cs" "payments-receipts-returns-summary.json generation documented"
Assert-FileContains "PosCore\Security\PosPaymentsReceiptsReturnsValidation.cs" "cash payment checklist documented"
Assert-FileContains "PosCore\Security\PosPaymentsReceiptsReturnsValidation.cs" "card payment checklist documented"
Assert-FileContains "PosCore\Security\PosPaymentsReceiptsReturnsValidation.cs" "split payment checklist documented"
Assert-FileContains "PosCore\Security\PosPaymentsReceiptsReturnsValidation.cs" "payment reconciliation checklist documented"
Assert-FileContains "PosCore\Security\PosPaymentsReceiptsReturnsValidation.cs" "receipt number traceability documented"
Assert-FileContains "PosCore\Security\PosPaymentsReceiptsReturnsValidation.cs" "receipt totals and tax snapshot documented"
Assert-FileContains "PosCore\Security\PosPaymentsReceiptsReturnsValidation.cs" "receipt audit trail checklist documented"
Assert-FileContains "PosCore\Security\PosPaymentsReceiptsReturnsValidation.cs" "return eligibility checklist documented"
Assert-FileContains "PosCore\Security\PosPaymentsReceiptsReturnsValidation.cs" "refund approval checkpoint documented"
Assert-FileContains "PosCore\Security\PosPaymentsReceiptsReturnsValidation.cs" "return reversal evidence documented"
Assert-FileContains "PosCore\Security\PosPaymentsReceiptsReturnsValidation.cs" "no real payment capture"
Assert-FileContains "PosCore\Security\PosPaymentsReceiptsReturnsValidation.cs" "no live payment gateway call"
Assert-FileContains "PosCore\Security\PosPaymentsReceiptsReturnsValidation.cs" "no receipt printing"
Assert-FileContains "PosCore\Security\PosPaymentsReceiptsReturnsValidation.cs" "no refund execution"
Assert-FileContains "PosCore\Security\PosPaymentsReceiptsReturnsValidation.cs" "no inventory mutation"
Assert-FileContains "PosCore\Security\PosPaymentsReceiptsReturnsValidation.cs" "no real checkout execution"
Assert-FileContains "PosCore\Security\PosPaymentsReceiptsReturnsValidation.cs" "no hardware access"
Assert-FileContains "PosCore\Security\PosPaymentsReceiptsReturnsValidation.cs" "no production sync enablement"
Assert-FileContains "PosCore\Security\PosPaymentsReceiptsReturnsValidation.cs" "no public API behavior change"
Assert-FileContains "PosCore\Security\PosPaymentsReceiptsReturnsValidation.cs" "no schema change"
Assert-FileContains "PosCore\Security\PosPaymentsReceiptsReturnsValidation.cs" "no migrations"
Assert-FileContains "scripts\release\Validate-Phase11PaymentsReceiptsReturnsValidation.ps1" "param("
Assert-FileContains "scripts\release\Validate-Phase11PaymentsReceiptsReturnsValidation.ps1" "Validate-Phase11FunctionalBusinessValidation.ps1"
Assert-FileContains "scripts\release\Validate-Phase11PaymentsReceiptsReturnsValidation.ps1" "PHASE 11.1 functional business outputs are missing"
Assert-FileContains "scripts\release\Validate-Phase11PaymentsReceiptsReturnsValidation.ps1" "payment-method-validation-evidence.json"
Assert-FileContains "scripts\release\Validate-Phase11PaymentsReceiptsReturnsValidation.ps1" "receipt-generation-audit-evidence.json"
Assert-FileContains "scripts\release\Validate-Phase11PaymentsReceiptsReturnsValidation.ps1" "returns-refund-workflow-evidence.json"
Assert-FileContains "scripts\release\Validate-Phase11PaymentsReceiptsReturnsValidation.ps1" "payments-receipts-returns-summary.json"
Assert-FileContains "scripts\release\Validate-Phase11PaymentsReceiptsReturnsValidation.ps1" "no real payment capture"
Assert-FileContains "scripts\release\Validate-Phase11PaymentsReceiptsReturnsValidation.ps1" "no live payment gateway call"
Assert-FileContains "scripts\release\Validate-Phase11PaymentsReceiptsReturnsValidation.ps1" "no receipt printing"
Assert-FileContains "scripts\release\Validate-Phase11PaymentsReceiptsReturnsValidation.ps1" "no refund execution"
Assert-FileContains "scripts\release\Validate-Phase11PaymentsReceiptsReturnsValidation.ps1" "no inventory mutation"
Assert-FileContains "scripts\release\Validate-Phase11PaymentsReceiptsReturnsValidation.ps1" "PHASE 11.2 payments receipts and returns validation verified."
Assert-FileContains "scripts\release\Validate-Phase11PaymentsReceiptsReturnsValidation.ps1" "AcceptedChecks: 15"
Assert-FileContains "scripts\release\Validate-Phase11PaymentsReceiptsReturnsValidation.ps1" "BlockingIssues: 0"
Assert-FileContains "docs\POS_PAYMENTS_RECEIPTS_RETURNS_VALIDATION.md" "PHASE 11.2 payments receipts and returns validation documented"
Assert-FileContains "docs\PHASE_11_2_PAYMENTS_RECEIPTS_RETURNS_VALIDATION.md" "556 tests passed"
Assert-FileContains "docs\PHASE_11_2_PAYMENTS_RECEIPTS_RETURNS_VALIDATION.md" "572 tests passed"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_11_2.md" "Functional business validation advanced from 25% to 50%"
Assert-FileContains "README.md" "PHASE 11.2"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "PHASE 11.2"

Write-Host "PHASE 11.2 markers verified."
