$ErrorActionPreference = "Stop"

$required = @(
    @{ Path = "PosCore\Security\PosOfflineSyncRetryBackoffPolicyBaseline.cs"; Pattern = "POS Offline Sync Retry Backoff Policy Baseline" },
    @{ Path = "PosCore\ViewModels\InventoryViewModel.cs"; Pattern = "PosOfflineSyncRetryBackoffPolicyStatus" },
    @{ Path = "PosCore\ViewModels\InventoryViewModel.cs"; Pattern = "PosOfflineSyncRetryBackoffPolicyRequiredChecks" },
    @{ Path = "PosCore\ViewModels\InventoryViewModel.cs"; Pattern = "PreparePosOfflineSyncRetryBackoffPolicyBaseline" },
    @{ Path = "PosCore\Views\InventoryWindow.xaml"; Pattern = "Retry Backoff" },
    @{ Path = "PosCore\Views\InventoryWindow.xaml"; Pattern = "no ejecuta sync real" },
    @{ Path = "docs\POS_OFFLINE_SYNC_RETRY_BACKOFF_POLICY_BASELINE.md"; Pattern = "offline sync retry backoff policy baseline only" },
    @{ Path = "docs\PHASE_4D_POS_OFFLINE_SYNC_RETRY_BACKOFF_POLICY_BASELINE.md"; Pattern = "No production sync execution" },
    @{ Path = "docs\PROJECT_PROGRESS_REPORT_PHASE_4D.md"; Pattern = "30% -> 40%" }
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

Write-Host "PHASE 4D markers verified."
