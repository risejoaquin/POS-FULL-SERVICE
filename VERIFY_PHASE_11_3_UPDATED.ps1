$ErrorActionPreference = "Stop"

function Assert-FileContains {
    param(
        [string]$Path,
        [string]$Text
    )

    if (!(Test-Path $Path)) {
        throw "Missing file: $Path"
    }

    $content = Get-Content -Raw -Path $Path
    if (!$content.Contains($Text)) {
        throw "Missing marker in ${Path}: $Text"
    }
}

Assert-FileContains "VERIFY_PHASE_11_2_UPDATED.ps1" "PHASE 11.2 markers verified."
Assert-FileContains "PosCore\Security\PosInventoryStockOfflineSyncValidation.cs" "PosInventoryStockOfflineSyncValidation"
Assert-FileContains "PosCore\Security\PosInventoryStockOfflineSyncValidation.cs" "PHASE 11.3 inventory stock movement and offline sync validation documented"
Assert-FileContains "PosCore\Security\PosInventoryStockOfflineSyncValidation.cs" "PHASE 11G inventory availability validation documented"
Assert-FileContains "PosCore\Security\PosInventoryStockOfflineSyncValidation.cs" "PHASE 11H stock movement audit validation documented"
Assert-FileContains "PosCore\Security\PosInventoryStockOfflineSyncValidation.cs" "PHASE 11I offline sync validation documented"
Assert-FileContains "PosCore\Security\PosInventoryStockOfflineSyncValidation.cs" "PHASE 11.2 payments receipts returns prerequisite documented"
Assert-FileContains "PosCore\Security\PosInventoryStockOfflineSyncValidation.cs" "572 tests passed source evidence documented"
Assert-FileContains "PosCore\Security\PosInventoryStockOfflineSyncValidation.cs" "588 tests expected after inventory stock offline sync validation documented"
Assert-FileContains "PosCore\Security\PosInventoryStockOfflineSyncValidation.cs" "inventory-availability-evidence.json generation documented"
Assert-FileContains "PosCore\Security\PosInventoryStockOfflineSyncValidation.cs" "stock-movement-audit-evidence.json generation documented"
Assert-FileContains "PosCore\Security\PosInventoryStockOfflineSyncValidation.cs" "offline-sync-readiness-evidence.json generation documented"
Assert-FileContains "PosCore\Security\PosInventoryStockOfflineSyncValidation.cs" "inventory-stock-offline-sync-summary.json generation documented"
Assert-FileContains "PosCore\Security\PosInventoryStockOfflineSyncValidation.cs" "stock availability checklist documented"
Assert-FileContains "PosCore\Security\PosInventoryStockOfflineSyncValidation.cs" "reserved stock boundary checklist documented"
Assert-FileContains "PosCore\Security\PosInventoryStockOfflineSyncValidation.cs" "low stock threshold checklist documented"
Assert-FileContains "PosCore\Security\PosInventoryStockOfflineSyncValidation.cs" "stock movement ledger checklist documented"
Assert-FileContains "PosCore\Security\PosInventoryStockOfflineSyncValidation.cs" "sale decrement traceability documented"
Assert-FileContains "PosCore\Security\PosInventoryStockOfflineSyncValidation.cs" "return restock traceability documented"
Assert-FileContains "PosCore\Security\PosInventoryStockOfflineSyncValidation.cs" "adjustment authorization checkpoint documented"
Assert-FileContains "PosCore\Security\PosInventoryStockOfflineSyncValidation.cs" "offline queue checklist documented"
Assert-FileContains "PosCore\Security\PosInventoryStockOfflineSyncValidation.cs" "sync conflict handling checklist documented"
Assert-FileContains "PosCore\Security\PosInventoryStockOfflineSyncValidation.cs" "sync retry and idempotency checklist documented"
Assert-FileContains "PosCore\Security\PosInventoryStockOfflineSyncValidation.cs" "sync reconciliation evidence documented"
Assert-FileContains "PosCore\Security\PosInventoryStockOfflineSyncValidation.cs" "no real inventory mutation"
Assert-FileContains "PosCore\Security\PosInventoryStockOfflineSyncValidation.cs" "no stock write execution"
Assert-FileContains "PosCore\Security\PosInventoryStockOfflineSyncValidation.cs" "no production sync enablement"
Assert-FileContains "PosCore\Security\PosInventoryStockOfflineSyncValidation.cs" "no live server commit"
Assert-FileContains "PosCore\Security\PosInventoryStockOfflineSyncValidation.cs" "no destructive reconciliation"
Assert-FileContains "PosCore\Security\PosInventoryStockOfflineSyncValidation.cs" "no checkout behavior change"
Assert-FileContains "PosCore\Security\PosInventoryStockOfflineSyncValidation.cs" "no public API behavior change"
Assert-FileContains "PosCore\Security\PosInventoryStockOfflineSyncValidation.cs" "no schema change"
Assert-FileContains "PosCore\Security\PosInventoryStockOfflineSyncValidation.cs" "no migrations"
Assert-FileContains "scripts\release\Validate-Phase11InventoryStockOfflineSyncValidation.ps1" "param("
Assert-FileContains "scripts\release\Validate-Phase11InventoryStockOfflineSyncValidation.ps1" "Validate-Phase11PaymentsReceiptsReturnsValidation.ps1"
Assert-FileContains "scripts\release\Validate-Phase11InventoryStockOfflineSyncValidation.ps1" "PHASE 11.2 payments receipts returns outputs are missing"
Assert-FileContains "scripts\release\Validate-Phase11InventoryStockOfflineSyncValidation.ps1" "inventory-availability-evidence.json"
Assert-FileContains "scripts\release\Validate-Phase11InventoryStockOfflineSyncValidation.ps1" "stock-movement-audit-evidence.json"
Assert-FileContains "scripts\release\Validate-Phase11InventoryStockOfflineSyncValidation.ps1" "offline-sync-readiness-evidence.json"
Assert-FileContains "scripts\release\Validate-Phase11InventoryStockOfflineSyncValidation.ps1" "inventory-stock-offline-sync-summary.json"
Assert-FileContains "scripts\release\Validate-Phase11InventoryStockOfflineSyncValidation.ps1" "no real inventory mutation"
Assert-FileContains "scripts\release\Validate-Phase11InventoryStockOfflineSyncValidation.ps1" "no stock write execution"
Assert-FileContains "scripts\release\Validate-Phase11InventoryStockOfflineSyncValidation.ps1" "no production sync enablement"
Assert-FileContains "scripts\release\Validate-Phase11InventoryStockOfflineSyncValidation.ps1" "no live server commit"
Assert-FileContains "scripts\release\Validate-Phase11InventoryStockOfflineSyncValidation.ps1" "PHASE 11.3 inventory stock movement and offline sync validation verified."
Assert-FileContains "scripts\release\Validate-Phase11InventoryStockOfflineSyncValidation.ps1" "AcceptedChecks: 15"
Assert-FileContains "scripts\release\Validate-Phase11InventoryStockOfflineSyncValidation.ps1" "BlockingIssues: 0"
Assert-FileContains "docs\POS_INVENTORY_STOCK_OFFLINE_SYNC_VALIDATION.md" "PHASE 11.3 inventory stock movement and offline sync validation documented"
Assert-FileContains "docs\PHASE_11_3_INVENTORY_STOCK_OFFLINE_SYNC_VALIDATION.md" "572 tests passed"
Assert-FileContains "docs\PHASE_11_3_INVENTORY_STOCK_OFFLINE_SYNC_VALIDATION.md" "588 tests passed"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_11_3.md" "Functional business validation advanced from 50% to 75%"
Assert-FileContains "README.md" "PHASE 11.3"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "PHASE 11.3"

Write-Host "PHASE 11.3 markers verified."
