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

Assert-FileContains "PosCore\Security\PosAuthServiceRemainingNullabilityHygiene.cs" "PosAuthServiceRemainingNullabilityHygiene"
Assert-FileContains "PosCore\Security\PosAuthServiceRemainingNullabilityHygiene.cs" "POS AuthService Remaining Nullability Hygiene"
Assert-FileContains "PosCore\Security\PosAuthServiceRemainingNullabilityHygiene.cs" "CS8602 AuthService login username dereference hygiene documented"
Assert-FileContains "PosCore\Security\PosAuthServiceRemainingNullabilityHygiene.cs" "login username local variable boundary implemented"
Assert-FileContains "PosCore\Security\PosAuthServiceRemainingNullabilityHygiene.cs" "nullable entity username guard implemented"
Assert-FileContains "PosCore\Security\PosAuthServiceRemainingNullabilityHygiene.cs" "no checkout behavior change"
Assert-FileContains "PosCore\Security\PosAuthServiceRemainingNullabilityHygiene.cs" "no inventory mutation"
Assert-FileContains "PosCore\Security\PosAuthServiceRemainingNullabilityHygiene.cs" "no production sync enablement"
Assert-FileContains "PosCore\Security\PosAuthServiceRemainingNullabilityHygiene.cs" "no schema change"
Assert-FileContains "PosCore\Security\PosAuthServiceRemainingNullabilityHygiene.cs" "no migrations"

Assert-FileContains "PosInfrastructure\Services\Server\AuthService.cs" "PHASE 7H AuthService remaining nullability hygiene applied"
Assert-FileContains "PosInfrastructure\Services\Server\AuthService.cs" "var loginUsername = request.Username;"
Assert-FileContains "PosInfrastructure\Services\Server\AuthService.cs" "var loginPassword = request.Password;"
Assert-FileContains "PosInfrastructure\Services\Server\AuthService.cs" "var usernameLower = loginUsername.ToLowerInvariant();"
Assert-FileContains "PosInfrastructure\Services\Server\AuthService.cs" "u.Username != null && u.Username.ToLower() == usernameLower"

Assert-FileContains "docs\POS_AUTHSERVICE_REMAINING_NULLABILITY_HYGIENE.md" "AuthService remaining nullability hygiene documented"
Assert-FileContains "docs\POS_AUTHSERVICE_REMAINING_NULLABILITY_HYGIENE.md" "nullable entity username guard implemented"
Assert-FileContains "docs\PHASE_7H_AUTHSERVICE_REMAINING_NULLABILITY_HYGIENE.md" "380 tests passed"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_7H.md" "70% -> 80%"

Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "PosAuthServiceRemainingNullabilityHygiene_Should_Define_AuthService_Nullability_Checks"
Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "Phase7H_AuthService_Should_Apply_Remaining_Login_Nullability_Hygiene"
Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "VerifyPhase7H_Should_Require_AuthService_Remaining_Nullability_Hygiene_Markers"

Write-Host "PHASE 7H markers verified."
