$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $root

$viewModel = Join-Path $root "PosCore\ViewModels\InventoryViewModel.cs"
$xaml = Join-Path $root "PosCore\Views\InventoryWindow.xaml"
$helper = Join-Path $root "PosCore\Security\InventoryDriftControlledReconciliationExecutionDesign.cs"

$required = @(
    @{ Path = $viewModel; Pattern = "InventoryDriftControlledReconciliationExecutionDesignStatus" },
    @{ Path = $viewModel; Pattern = "InventoryDriftControlledReconciliationExecutionDesignRequiredPreconditions" },
    @{ Path = $viewModel; Pattern = "PrepareInventoryDriftControlledReconciliationExecutionDesign" },
    @{ Path = $xaml; Pattern = "PrepareInventoryDriftControlledReconciliationExecutionDesignCommand" },
    @{ Path = $xaml; Pattern = "no ejecuta reconciliación real" },
    @{ Path = $helper; Pattern = "inventory.drift.controlled.reconciliation.execution.design.baseline" }
)

foreach ($item in $required) {
    if (!(Test-Path $item.Path)) {
        throw "Missing file: $($item.Path)"
    }

    $match = Select-String -Path $item.Path -Pattern $item.Pattern -SimpleMatch
    if (!$match) {
        throw "Missing required pattern '$($item.Pattern)' in $($item.Path)"
    }
}

Write-Host "PHASE 3P verification markers found." -ForegroundColor Green
