$ErrorActionPreference = "Stop"

$requiredFiles = @(
    "PosCore\Security\PosProductionSyncCanaryRolloutBaseline.cs",
    "docs\POS_PRODUCTION_SYNC_CANARY_ROLLOUT_BASELINE.md",
    "docs\PHASE_5C_PRODUCTION_SYNC_CANARY_ROLLOUT_BASELINE.md",
    "docs\PROJECT_PROGRESS_REPORT_PHASE_5C.md"
)

foreach ($file in $requiredFiles) {
    if (-not (Test-Path $file)) {
        throw "Missing PHASE 5C file: $file"
    }
}

$allText = @(
    Get-Content "PosCore\ViewModels\InventoryViewModel.cs" -Raw
    Get-Content "PosCore\Views\InventoryWindow.xaml" -Raw
    Get-Content "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" -Raw
    Get-Content "PosCore\Security\PosProductionSyncCanaryRolloutBaseline.cs" -Raw
    Get-Content "docs\POS_PRODUCTION_SYNC_CANARY_ROLLOUT_BASELINE.md" -Raw
    Get-Content "docs\PHASE_5C_PRODUCTION_SYNC_CANARY_ROLLOUT_BASELINE.md" -Raw
    Get-Content "docs\PROJECT_PROGRESS_REPORT_PHASE_5C.md" -Raw
) -join "`n"

$checks = @(
    "PosProductionSyncCanaryRolloutStatus",
    "PosProductionSyncCanaryRolloutBaselineReady",
    "PosProductionSyncCanaryRolloutReviewedAt",
    "PosProductionSyncCanaryRolloutRequiredChecks",
    "PosProductionSyncCanaryRolloutSummary",
    "PreparePosProductionSyncCanaryRolloutBaselineCommand",
    "POS Production Sync Canary Rollout Baseline",
    "production sync canary rollout documented",
    "canary cohort selection documented",
    "tenant canary scope documented",
    "device canary scope documented",
    "canary percentage cap documented",
    "failure thresholds documented",
    "automatic pause criteria documented",
    "manual rollback criteria documented",
    "kill switch integration documented",
    "feature flag promotion gate documented",
    "no production sync execution",
    "no queue writes",
    "no sync enablement",
    "no runtime flag toggle",
    "no inventory mutation",
    "no checkout changes",
    "Phase5C_Should_Be_Documented_As_Production_Sync_CanaryRollout_Baseline_Only"
)

foreach ($check in $checks) {
    if ($allText -notlike "*$check*") {
        throw "Missing PHASE 5C marker: $check"
    }
}

Write-Host "PHASE 5C markers verified."
