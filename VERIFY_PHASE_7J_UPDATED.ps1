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

Assert-FileContains "PosCore\Security\PosSecurityHardeningClosureZeroWarningEvidence.cs" "PosSecurityHardeningClosureZeroWarningEvidence"
Assert-FileContains "PosCore\Security\PosSecurityHardeningClosureZeroWarningEvidence.cs" "POS Security Hardening Closure & Zero-Warning Evidence"
Assert-FileContains "PosCore\Security\PosSecurityHardeningClosureZeroWarningEvidence.cs" "zero-warning Release build evidence documented"
Assert-FileContains "PosCore\Security\PosSecurityHardeningClosureZeroWarningEvidence.cs" "zero-error Release build evidence documented"
Assert-FileContains "PosCore\Security\PosSecurityHardeningClosureZeroWarningEvidence.cs" "385 tests passed source evidence documented"
Assert-FileContains "PosCore\Security\PosSecurityHardeningClosureZeroWarningEvidence.cs" "390 tests expected after closure verification documented"
Assert-FileContains "PosCore\Security\PosSecurityHardeningClosureZeroWarningEvidence.cs" "warning regression guardrails documented"
Assert-FileContains "PosCore\Security\PosSecurityHardeningClosureZeroWarningEvidence.cs" "no checkout behavior change"
Assert-FileContains "PosCore\Security\PosSecurityHardeningClosureZeroWarningEvidence.cs" "no inventory mutation"
Assert-FileContains "PosCore\Security\PosSecurityHardeningClosureZeroWarningEvidence.cs" "no production sync enablement"
Assert-FileContains "PosCore\Security\PosSecurityHardeningClosureZeroWarningEvidence.cs" "no schema change"
Assert-FileContains "PosCore\Security\PosSecurityHardeningClosureZeroWarningEvidence.cs" "no migrations"

Assert-FileContains "docs\POS_SECURITY_HARDENING_CLOSURE_ZERO_WARNING_EVIDENCE.md" "Security hardening closure documented"
Assert-FileContains "docs\POS_SECURITY_HARDENING_CLOSURE_ZERO_WARNING_EVIDENCE.md" "zero-warning Release build evidence documented"
Assert-FileContains "docs\POS_SECURITY_HARDENING_CLOSURE_ZERO_WARNING_EVIDENCE.md" "0 Advertencia(s)"
Assert-FileContains "docs\POS_SECURITY_HARDENING_CLOSURE_ZERO_WARNING_EVIDENCE.md" "0 Errores"
Assert-FileContains "docs\POS_SECURITY_HARDENING_CLOSURE_ZERO_WARNING_EVIDENCE.md" "warning regression guardrails documented"
Assert-FileContains "docs\PHASE_7J_SECURITY_HARDENING_CLOSURE_ZERO_WARNING_EVIDENCE.md" "390 tests passed"
Assert-FileContains "docs\PHASE_7J_SECURITY_HARDENING_CLOSURE_ZERO_WARNING_EVIDENCE.md" "385 tests passed"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_7J.md" "90% -> 100%"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_7J.md" "0 Advertencia(s)"

Assert-FileContains "README.md" "PHASE 7J"
Assert-FileContains "README.md" "Security Hardening Closure"
Assert-FileContains "README.md" "Zero-Warning Evidence"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "PHASE 7J"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "Security Hardening Closure"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "Zero-Warning Evidence"
Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "PosSecurityHardeningClosureZeroWarningEvidence_Should_Define_Closure_Evidence_Checks"
Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "Phase7J_Documentation_Should_Describe_Zero_Warning_Closure_Evidence"
Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "VerifyPhase7J_Should_Require_Security_Hardening_Closure_Markers"

Write-Host "PHASE 7J markers verified."
