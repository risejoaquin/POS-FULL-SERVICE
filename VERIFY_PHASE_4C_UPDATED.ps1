$ErrorActionPreference = "Stop"

$required = @(
    @{ Path = ".\PosCore\Security\PosOfflineSyncIdempotencyKeyStrategyBaseline.cs"; Pattern = "POS Offline Sync Idempotency Key Strategy Baseline" },
    @{ Path = ".\PosCore\ViewModels\InventoryViewModel.cs"; Pattern = "PosOfflineSyncIdempotencyKeyStrategyStatus" },
    @{ Path = ".\PosCore\ViewModels\InventoryViewModel.cs"; Pattern = "PosOfflineSyncIdempotencyKeyStrategyRequiredChecks" },
    @{ Path = ".\PosCore\ViewModels\InventoryViewModel.cs"; Pattern = "PreparePosOfflineSyncIdempotencyKeyStrategyBaseline" },
    @{ Path = ".\PosCore\Views\InventoryWindow.xaml"; Pattern = "PreparePosOfflineSyncIdempotencyKeyStrategyBaselineCommand" },
    @{ Path = ".\PosCore\Views\InventoryWindow.xaml"; Pattern = "no ejecuta sync real" },
    @{ Path = ".\PosCore\Views\InventoryWindow.xaml"; Pattern = "no escribe cola" },
    @{ Path = ".\docs\POS_OFFLINE_SYNC_IDEMPOTENCY_KEY_STRATEGY_BASELINE.md"; Pattern = "offline sync idempotency key strategy baseline only" },
    @{ Path = ".\docs\PHASE_4C_POS_OFFLINE_SYNC_IDEMPOTENCY_KEY_STRATEGY_BASELINE.md"; Pattern = "No production sync execution" },
    @{ Path = ".\docs\PROJECT_PROGRESS_REPORT_PHASE_4C.md"; Pattern = "20% -> 30%" }
)

foreach ($item in $required) {
    if (!(Test-Path $item.Path)) {
        throw "Missing file: $($item.Path)"
    }

    if (!(Select-String -Path $item.Path -Pattern $item.Pattern -SimpleMatch -Quiet)) {
        throw "Missing marker '$($item.Pattern)' in $($item.Path)"
    }
}

Write-Host "PHASE 4C markers verified."
