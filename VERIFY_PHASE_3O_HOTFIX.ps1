$ErrorActionPreference = "Stop"
$vm = ".\PosCore\ViewModels\InventoryViewModel.cs"
$xaml = ".\PosCore\Views\InventoryWindow.xaml"
$requiredVm = @(
  "InventoryDriftReconciliationSyncSafetyRequiredChecks",
  "Sincronización segura para reconciliación preparada"
)
$requiredXaml = @(
  "PrepareInventoryDriftReconciliationSyncSafetyGuardCommand",
  "no modifica sync",
  "no ejecuta ajustes de stock"
)
foreach ($pattern in $requiredVm) {
  if (-not (Select-String -Path $vm -Pattern $pattern -SimpleMatch -Quiet)) {
    throw "Missing required InventoryViewModel marker: $pattern"
  }
}
foreach ($pattern in $requiredXaml) {
  if (-not (Select-String -Path $xaml -Pattern $pattern -SimpleMatch -Quiet)) {
    throw "Missing required InventoryWindow marker: $pattern"
  }
}
Write-Host "PHASE 3O hotfix verification passed." -ForegroundColor Green
