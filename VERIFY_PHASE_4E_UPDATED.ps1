$ErrorActionPreference = "Stop"

$required = @(
    @{ Path = "PosCore\Security\PosOfflineSyncConflictDetectionStrategyBaseline.cs"; Pattern = "POS Offline Sync Conflict Detection Strategy Baseline" },
    @{ Path = "PosCore\ViewModels\InventoryViewModel.cs"; Pattern = "PosOfflineSyncConflictDetectionStrategyStatus" },
    @{ Path = "PosCore\ViewModels\InventoryViewModel.cs"; Pattern = "PosOfflineSyncConflictDetectionStrategyRequiredChecks" },
    @{ Path = "PosCore\ViewModels\InventoryViewModel.cs"; Pattern = "PreparePosOfflineSyncConflictDetectionStrategyBaseline" },
    @{ Path = "PosCore\Views\InventoryWindow.xaml"; Pattern = "Conflict Detection" },
    @{ Path = "PosCore\Views\InventoryWindow.xaml"; Pattern = "no ejecuta sync real" },
    @{ Path = "PosCore\Views\InventoryWindow.xaml"; Pattern = "no resuelve conflictos" },
    @{ Path = "docs\POS_OFFLINE_SYNC_CONFLICT_DETECTION_STRATEGY_BASELINE.md"; Pattern = "offline sync conflict detection strategy baseline only" },
    @{ Path = "docs\PHASE_4E_POS_OFFLINE_SYNC_CONFLICT_DETECTION_STRATEGY_BASELINE.md"; Pattern = "No production sync execution" },
    @{ Path = "docs\PROJECT_PROGRESS_REPORT_PHASE_4E.md"; Pattern = "40% -> 50%" }
)

foreach ($item in $required) {
    if (!(Test-Path $item.Path)) {
        throw "Missing file: $($item.Path)"
    }

    $match = Select-String -Path $item.Path -Pattern $item.Pattern -SimpleMatch -Quiet
    if (-not $match) {
        throw "Missing marker '$($item.Pattern)' in $($item.Path)"
    }
}

Write-Host "PHASE 4E markers verified."
