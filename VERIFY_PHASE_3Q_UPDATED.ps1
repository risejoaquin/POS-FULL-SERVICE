$ErrorActionPreference = "Stop"

$required = @(
    @{ Path = ".\PosCore\Security\InventoryDriftReconciliationFinalRunbook.cs"; Pattern = "InventoryDriftReconciliationFinalRunbook" },
    @{ Path = ".\PosCore\ViewModels\InventoryViewModel.cs"; Pattern = "InventoryDriftReconciliationFinalRunbookOperationalClosureChecklist" },
    @{ Path = ".\PosCore\Views\InventoryWindow.xaml"; Pattern = "PrepareInventoryDriftReconciliationFinalRunbookOperationalClosureCommand" },
    @{ Path = ".\docs\INVENTORY_DRIFT_RECONCILIATION_FINAL_RUNBOOK.md"; Pattern = "final runbook closure only" },
    @{ Path = ".\docs\PROJECT_PROGRESS_REPORT_PHASE_3Q.md"; Pattern = "99.8% -> 100%" }
)

foreach ($item in $required) {
    if (-not (Test-Path $item.Path)) { throw "Missing file: $($item.Path)" }
    if (-not (Select-String -Path $item.Path -Pattern $item.Pattern -SimpleMatch -Quiet)) {
        throw "Missing pattern '$($item.Pattern)' in $($item.Path)"
    }
}

Write-Host "PHASE 3Q markers verified." -ForegroundColor Green
