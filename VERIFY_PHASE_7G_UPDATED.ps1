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

Assert-FileContains "PosCore\Security\PosSyncServiceNullabilityHygiene.cs" "PosSyncServiceNullabilityHygiene"
Assert-FileContains "PosCore\Security\PosSyncServiceNullabilityHygiene.cs" "POS SyncService Nullability Hygiene"
Assert-FileContains "PosCore\Security\PosSyncServiceNullabilityHygiene.cs" "CS8602 SyncService username dereference hygiene documented"
Assert-FileContains "PosCore\Security\PosSyncServiceNullabilityHygiene.cs" "cloud username null guard implemented"
Assert-FileContains "PosCore\Security\PosSyncServiceNullabilityHygiene.cs" "local username null guard implemented"
Assert-FileContains "PosCore\Security\PosSyncServiceNullabilityHygiene.cs" "pull updates behavior preserved"
Assert-FileContains "PosCore\Security\PosSyncServiceNullabilityHygiene.cs" "no checkout behavior change"
Assert-FileContains "PosCore\Security\PosSyncServiceNullabilityHygiene.cs" "no inventory mutation"
Assert-FileContains "PosCore\Security\PosSyncServiceNullabilityHygiene.cs" "no production sync enablement"
Assert-FileContains "PosCore\Security\PosSyncServiceNullabilityHygiene.cs" "no schema change"
Assert-FileContains "PosCore\Security\PosSyncServiceNullabilityHygiene.cs" "no migrations"

Assert-FileContains "PosCore\Services\SyncService.cs" "PHASE 7G SyncService nullability hygiene applied"
Assert-FileContains "PosCore\Services\SyncService.cs" "var cloudUsername = cloudUser.Username;"
Assert-FileContains "PosCore\Services\SyncService.cs" "string.IsNullOrWhiteSpace(cloudUsername)"
Assert-FileContains "PosCore\Services\SyncService.cs" "var normalizedCloudUsername = cloudUsername.ToLowerInvariant();"
Assert-FileContains "PosCore\Services\SyncService.cs" "u.Username != null && u.Username.ToLower() == normalizedCloudUsername"

Assert-FileContains "docs\POS_SYNCSERVICE_NULLABILITY_HYGIENE.md" "SyncService nullability hygiene documented"
Assert-FileContains "docs\POS_SYNCSERVICE_NULLABILITY_HYGIENE.md" "cloud username null guard implemented"
Assert-FileContains "docs\PHASE_7G_SYNCSERVICE_NULLABILITY_HYGIENE.md" "375 tests passed"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_7G.md" "60% -> 70%"

Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "PosSyncServiceNullabilityHygiene_Should_Define_SyncService_Nullability_Checks"
Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "Phase7G_SyncService_Should_Apply_Targeted_Username_Nullability_Hygiene"
Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "VerifyPhase7G_Should_Require_SyncService_Nullability_Hygiene_Markers"

Write-Host "PHASE 7G markers verified."
