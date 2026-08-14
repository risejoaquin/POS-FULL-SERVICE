$ErrorActionPreference = "Stop"

$requiredFiles = @(
    "PosCore/Security/PosProductionSyncFeatureFlagKillSwitchBaseline.cs",
    "docs/POS_PRODUCTION_SYNC_FEATURE_FLAG_KILL_SWITCH_BASELINE.md",
    "docs/PHASE_5B_PRODUCTION_SYNC_FEATURE_FLAG_KILL_SWITCH_BASELINE.md",
    "docs/PROJECT_PROGRESS_REPORT_PHASE_5B.md"
)

foreach ($file in $requiredFiles) {
    if (-not (Test-Path $file)) {
        throw "Missing PHASE 5B file: $file"
    }
}

$allText = @(
    Get-Content "PosCore/Security/PosProductionSyncFeatureFlagKillSwitchBaseline.cs" -Raw
    Get-Content "PosCore/ViewModels/InventoryViewModel.cs" -Raw
    Get-Content "PosCore/Views/InventoryWindow.xaml" -Raw
    Get-Content "PosInfrastructure.Tests/Architecture/InventoryLedgerConcurrencyBaselineTests.cs" -Raw
    Get-Content "docs/POS_PRODUCTION_SYNC_FEATURE_FLAG_KILL_SWITCH_BASELINE.md" -Raw
    Get-Content "docs/PHASE_5B_PRODUCTION_SYNC_FEATURE_FLAG_KILL_SWITCH_BASELINE.md" -Raw
    Get-Content "docs/PROJECT_PROGRESS_REPORT_PHASE_5B.md" -Raw
) -join "`n"

$checks = @(
    "PosProductionSyncFeatureFlagKillSwitchBaseline",
    "PosProductionSyncFeatureFlagKillSwitchStatus",
    "PosProductionSyncFeatureFlagKillSwitchRequiredChecks",
    "PreparePosProductionSyncFeatureFlagKillSwitchBaselineCommand",
    "production sync feature flag documented",
    "kill switch documented",
    "safe disable behavior documented",
    "default disabled state documented",
    "tenant scoped feature flag documented",
    "device scoped feature flag documented",
    "canary rollout flag documented",
    "emergency rollback trigger documented",
    "queue processing pause behavior documented",
    "checkpoint freeze on disable documented",
    "idempotency preservation on disable documented",
    "audit log requirement documented",
    "no production sync execution",
    "no queue writes",
    "no sync enablement",
    "no runtime flag toggle",
    "no inventory mutation",
    "PHASE 5B"
)

foreach ($check in $checks) {
    if ($allText -notmatch [regex]::Escape($check)) {
        throw "Missing PHASE 5B marker: $check"
    }
}

Write-Host "PHASE 5B markers verified."
