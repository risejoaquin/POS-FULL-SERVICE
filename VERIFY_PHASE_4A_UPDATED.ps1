$ErrorActionPreference = "Stop"

$checks = @(
    @{ Path = ".\PosCore\Security\PosOfflineSyncReliabilityBaseline.cs"; Pattern = "POS Offline Sync Reliability Baseline" },
    @{ Path = ".\PosCore\ViewModels\InventoryViewModel.cs"; Pattern = "PosOfflineSyncReliabilityRequiredChecks" },
    @{ Path = ".\PosCore\ViewModels\InventoryViewModel.cs"; Pattern = "PreparePosOfflineSyncReliabilityBaseline" },
    @{ Path = ".\PosCore\Views\InventoryWindow.xaml"; Pattern = "Sync Reliability" },
    @{ Path = ".\docs\POS_OFFLINE_SYNC_RELIABILITY_BASELINE.md"; Pattern = "offline sync reliability baseline only" },
    @{ Path = ".\docs\PHASE_4A_POS_OFFLINE_SYNC_RELIABILITY_BASELINE.md"; Pattern = "No production sync execution" }
)

foreach ($check in $checks) {
    if (!(Test-Path $check.Path)) {
        throw "Missing file: $($check.Path)"
    }

    $match = Select-String -Path $check.Path -Pattern $check.Pattern -SimpleMatch -Quiet
    if (-not $match) {
        throw "Missing marker '$($check.Pattern)' in $($check.Path)"
    }
}

Write-Host "PHASE 4A markers verified."
