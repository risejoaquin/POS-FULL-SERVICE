$ErrorActionPreference = "Stop"

$checks = @(
    @{ Path = "PosCore\Security\PosProductionSyncExecutionGateSafeEnablementBaseline.cs"; Text = "Production Sync Execution Gate Safe Enablement Baseline" },
    @{ Path = "PosCore\ViewModels\InventoryViewModel.cs"; Text = "PosProductionSyncExecutionGateSafeEnablementStatus" },
    @{ Path = "PosCore\ViewModels\InventoryViewModel.cs"; Text = "PosProductionSyncExecutionGateSafeEnablementRequiredChecks" },
    @{ Path = "PosCore\ViewModels\InventoryViewModel.cs"; Text = "PreparePosProductionSyncExecutionGateSafeEnablementBaseline" },
    @{ Path = "PosCore\Views\InventoryWindow.xaml"; Text = "Sync Gate" },
    @{ Path = "PosCore\Views\InventoryWindow.xaml"; Text = "no ejecuta sync real" },
    @{ Path = "PosCore\Views\InventoryWindow.xaml"; Text = "no habilita sync" },
    @{ Path = "docs\POS_PRODUCTION_SYNC_EXECUTION_GATE_SAFE_ENABLEMENT_BASELINE.md"; Text = "production sync execution gate and safe enablement baseline only" },
    @{ Path = "docs\PHASE_5A_PRODUCTION_SYNC_EXECUTION_GATE_SAFE_ENABLEMENT_BASELINE.md"; Text = "No sync enablement" },
    @{ Path = "docs\PROJECT_PROGRESS_REPORT_PHASE_5A.md"; Text = "0% -> 10%" }
)

foreach ($check in $checks) {
    if (!(Test-Path $check.Path)) {
        throw "Missing PHASE 5A file: $($check.Path)"
    }

    $text = Get-Content $check.Path -Raw
    if ($text -notlike "*$($check.Text)*") {
        throw "Missing PHASE 5A marker: $($check.Text)"
    }
}

Write-Host "PHASE 5A markers verified."
