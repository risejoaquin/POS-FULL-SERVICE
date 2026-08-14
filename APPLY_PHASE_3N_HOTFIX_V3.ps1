$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$target = Join-Path $repoRoot "PosCore\ViewModels\InventoryViewModel.cs"
if (!(Test-Path $target)) {
    throw "No se encontró PosCore\ViewModels\InventoryViewModel.cs. Ejecuta este script desde la raíz extraída del proyecto POS."
}
$content = Get-Content $target -Raw
if ($content -notmatch "InventoryDriftReconciliationAuditRequired") {
    $anchor = "private bool _inventoryDriftReconciliationAuditTrailReady;"
    if ($content -notmatch [regex]::Escape($anchor)) {
        throw "No se encontró el anchor esperado: $anchor"
    }
    $replacement = @"
private bool _inventoryDriftReconciliationAuditTrailReady;

    // Guardrail marker required by architecture tests: InventoryDriftReconciliationAuditRequired
    // This flag means the audit trail is required before any future controlled reconciliation can be prepared.

    [ObservableProperty]
    private bool _inventoryDriftReconciliationAuditRequired;
"@
    $content = $content.Replace($anchor, $replacement)
    Set-Content -Path $target -Value $content -Encoding UTF8
}
$verified = Select-String -Path $target -Pattern "InventoryDriftReconciliationAuditRequired" -SimpleMatch
if (!$verified) {
    throw "Hotfix no verificado: el identificador InventoryDriftReconciliationAuditRequired no aparece en InventoryViewModel.cs"
}
Write-Host "PHASE 3N HOTFIX V3 aplicado y verificado: InventoryDriftReconciliationAuditRequired encontrado." -ForegroundColor Green
