$ErrorActionPreference = "Stop"

$checks = @(
    @{ Path = "PosCore\Security\PosOfflineSyncQueueDiagnosticsBaseline.cs"; Pattern = "POS Offline Sync Queue Inventory & Diagnostics Baseline" },
    @{ Path = "PosCore\ViewModels\InventoryViewModel.cs"; Pattern = "PosOfflineSyncQueueDiagnosticsStatus" },
    @{ Path = "PosCore\ViewModels\InventoryViewModel.cs"; Pattern = "PosOfflineSyncQueueDiagnosticsRequiredChecks" },
    @{ Path = "PosCore\Views\InventoryWindow.xaml"; Pattern = "Queue Diagnostics" },
    @{ Path = "PosCore\Views\InventoryWindow.xaml"; Pattern = "no escribe cola" },
    @{ Path = "docs\POS_OFFLINE_SYNC_QUEUE_DIAGNOSTICS_BASELINE.md"; Pattern = "offline sync queue diagnostics baseline only" },
    @{ Path = "docs\PHASE_4B_POS_OFFLINE_SYNC_QUEUE_INVENTORY_DIAGNOSTICS_BASELINE.md"; Pattern = "No queue writes" }
)

foreach ($check in $checks) {
    if (-not (Test-Path $check.Path)) { throw "Missing file: $($check.Path)" }
    $match = Select-String -Path $check.Path -Pattern $check.Pattern -SimpleMatch -Quiet
    if (-not $match) { throw "Missing marker '$($check.Pattern)' in $($check.Path)" }
}

Write-Host "PHASE 4B markers verified."
