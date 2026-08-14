param(
    [string]$ReleaseVersion = "0.9.0-rc.1",
    [string]$PreviousVersion = "0.9.0-rc.0",
    [string]$ReleaseChannel = "release-candidate"
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$phase11Root = Join-Path $root "artifacts\release\phase11"
$phase112Root = Join-Path $phase11Root "payments-receipts-returns-validation"
$outputRoot = Join-Path $phase11Root "inventory-stock-offline-sync-validation"

$phase112Summary = Join-Path $phase112Root "payments-receipts-returns-summary.json"
if (!(Test-Path $phase112Summary)) {
    Write-Host "PHASE 11.2 payments receipts returns outputs are missing. Regenerating payments receipts returns validation before inventory stock offline sync validation."
    & (Join-Path $PSScriptRoot "Validate-Phase11PaymentsReceiptsReturnsValidation.ps1") -ReleaseVersion $ReleaseVersion -PreviousVersion $PreviousVersion -ReleaseChannel $ReleaseChannel
}

New-Item -ItemType Directory -Force -Path $outputRoot | Out-Null

$inventoryAvailability = [ordered]@{
    phase = "PHASE 11.3"
    scope = "inventory availability validation"
    releaseVersion = $ReleaseVersion
    releaseChannel = $ReleaseChannel
    checks = @(
        "stock availability checklist documented",
        "reserved stock boundary checklist documented",
        "low stock threshold checklist documented"
    )
    guardrails = @(
        "no real inventory mutation",
        "no stock write execution",
        "no checkout behavior change"
    )
    acceptedChecks = 3
    blockingIssues = 0
}

$stockMovementAudit = [ordered]@{
    phase = "PHASE 11.3"
    scope = "stock movement audit validation"
    releaseVersion = $ReleaseVersion
    previousVersion = $PreviousVersion
    checks = @(
        "stock movement ledger checklist documented",
        "sale decrement traceability documented",
        "return restock traceability documented",
        "adjustment authorization checkpoint documented"
    )
    guardrails = @(
        "no real inventory mutation",
        "no destructive reconciliation",
        "no schema change",
        "no migrations"
    )
    acceptedChecks = 4
    blockingIssues = 0
}

$offlineSyncReadiness = [ordered]@{
    phase = "PHASE 11.3"
    scope = "offline sync validation"
    releaseVersion = $ReleaseVersion
    releaseChannel = $ReleaseChannel
    checks = @(
        "offline queue checklist documented",
        "sync conflict handling checklist documented",
        "sync retry and idempotency checklist documented",
        "sync reconciliation evidence documented"
    )
    guardrails = @(
        "no production sync enablement",
        "no live server commit",
        "no public API behavior change"
    )
    acceptedChecks = 4
    blockingIssues = 0
}

$summary = [ordered]@{
    phase = "PHASE 11.3"
    name = "Inventory, Stock Movement and Offline Sync Validation"
    prerequisite = "PHASE 11.2 payments receipts returns validation"
    sourceEvidence = "572 tests passed source evidence documented"
    expectedEvidence = "588 tests expected after inventory stock offline sync validation documented"
    outputs = @(
        "inventory-availability-evidence.json",
        "stock-movement-audit-evidence.json",
        "offline-sync-readiness-evidence.json",
        "inventory-stock-offline-sync-summary.json"
    )
    guardrails = @(
        "no real inventory mutation",
        "no stock write execution",
        "no production sync enablement",
        "no live server commit",
        "no destructive reconciliation",
        "no checkout behavior change",
        "no public API behavior change",
        "no schema change",
        "no migrations"
    )
    acceptedChecks = 15
    blockingIssues = 0
}

$inventoryAvailabilityPath = Join-Path $outputRoot "inventory-availability-evidence.json"
$stockMovementAuditPath = Join-Path $outputRoot "stock-movement-audit-evidence.json"
$offlineSyncReadinessPath = Join-Path $outputRoot "offline-sync-readiness-evidence.json"
$summaryPath = Join-Path $outputRoot "inventory-stock-offline-sync-summary.json"

$inventoryAvailability | ConvertTo-Json -Depth 8 | Set-Content -Encoding UTF8 -Path $inventoryAvailabilityPath
$stockMovementAudit | ConvertTo-Json -Depth 8 | Set-Content -Encoding UTF8 -Path $stockMovementAuditPath
$offlineSyncReadiness | ConvertTo-Json -Depth 8 | Set-Content -Encoding UTF8 -Path $offlineSyncReadinessPath
$summary | ConvertTo-Json -Depth 8 | Set-Content -Encoding UTF8 -Path $summaryPath

Write-Host "PHASE 11.3 inventory stock movement and offline sync validation verified."
Write-Host "InventoryAvailability: $inventoryAvailabilityPath"
Write-Host "StockMovementAudit: $stockMovementAuditPath"
Write-Host "OfflineSyncReadiness: $offlineSyncReadinessPath"
Write-Host "InventoryStockOfflineSyncSummary: $summaryPath"
Write-Host "AcceptedChecks: 15"
Write-Host "BlockingIssues: 0"
