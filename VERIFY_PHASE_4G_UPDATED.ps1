$ErrorActionPreference = "Stop"

$requiredPaths = @(
    "PosCore\Security\PosOfflineSyncTenantDeviceBoundarySyncOwnershipBaseline.cs",
    "docs\POS_OFFLINE_SYNC_TENANT_DEVICE_BOUNDARY_SYNC_OWNERSHIP_BASELINE.md",
    "docs\PHASE_4G_POS_OFFLINE_SYNC_TENANT_DEVICE_BOUNDARY_SYNC_OWNERSHIP_BASELINE.md",
    "docs\PROJECT_PROGRESS_REPORT_PHASE_4G.md"
)

foreach ($path in $requiredPaths) {
    if (!(Test-Path $path)) {
        throw "Missing required PHASE 4G file: $path"
    }
}

$viewModel = Get-Content "PosCore\ViewModels\InventoryViewModel.cs" -Raw
$xaml = Get-Content "PosCore\Views\InventoryWindow.xaml" -Raw
$tests = Get-Content "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" -Raw
$baseline = Get-Content "PosCore\Security\PosOfflineSyncTenantDeviceBoundarySyncOwnershipBaseline.cs" -Raw

$requiredMarkers = @(
    "PosOfflineSyncTenantDeviceBoundarySyncOwnershipStatus",
    "PosOfflineSyncTenantDeviceBoundarySyncOwnershipBaselineReady",
    "PosOfflineSyncTenantDeviceBoundarySyncOwnershipReviewedAt",
    "PosOfflineSyncTenantDeviceBoundarySyncOwnershipRequiredChecks",
    "PosOfflineSyncTenantDeviceBoundarySyncOwnershipSummary",
    "PreparePosOfflineSyncTenantDeviceBoundarySyncOwnershipBaselineCommand",
    "POS Offline Sync Tenant/Device Boundary & Sync Ownership Baseline",
    "tenant id boundary documented",
    "device id boundary documented",
    "local queue owner documented",
    "sync ownership boundary documented",
    "single writer ownership rule documented",
    "no production sync execution",
    "no queue writes",
    "no sync ownership claim",
    "no inventory mutation",
    "no checkout changes",
    "Phase4G_Should_Be_Documented_As_Pos_Offline_Sync_Tenant_Device_Boundary_SyncOwnership_Baseline_Only"
)

$combined = $viewModel + "`n" + $xaml + "`n" + $tests + "`n" + $baseline
foreach ($marker in $requiredMarkers) {
    if ($combined -notmatch [regex]::Escape($marker)) {
        throw "Missing PHASE 4G marker: $marker"
    }
}

Write-Host "PHASE 4G markers verified."
