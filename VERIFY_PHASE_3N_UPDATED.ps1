$ErrorActionPreference = "Stop"

$target = Join-Path (Get-Location) "PosCore\ViewModels\InventoryViewModel.cs"
if (!(Test-Path $target)) {
    throw "No se encontró PosCore\ViewModels\InventoryViewModel.cs. Ejecuta este script desde la raíz del proyecto POS."
}

$matches = Select-String -Path $target -Pattern "InventoryDriftReconciliationAuditRequired" -SimpleMatch
if (!$matches) {
    throw "Verificación fallida: falta InventoryDriftReconciliationAuditRequired en PosCore\ViewModels\InventoryViewModel.cs"
}

Write-Host "Verificación OK: InventoryDriftReconciliationAuditRequired existe en InventoryViewModel.cs" -ForegroundColor Green
$matches | ForEach-Object { Write-Host ("Line {0}: {1}" -f $_.LineNumber, $_.Line.Trim()) }
Write-Host "Ahora puedes correr: dotnet test; dotnet build -c Release Pos.sln" -ForegroundColor Cyan
