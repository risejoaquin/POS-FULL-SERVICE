$ErrorActionPreference = "Stop"

$files = @(
    "PosCore\Security\PosProductionSyncQueueClaimLeaseImplementation.cs",
    "PosCore\ViewModels\InventoryViewModel.cs",
    "PosCore\Views\InventoryWindow.xaml",
    "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs",
    "docs\POS_PRODUCTION_SYNC_QUEUE_CLAIM_LEASE_IMPLEMENTATION.md",
    "docs\PHASE_6D_PRODUCTION_SYNC_QUEUE_CLAIM_LEASE_IMPLEMENTATION.md",
    "docs\PROJECT_PROGRESS_REPORT_PHASE_6D.md",
    "README.md",
    "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md"
)

$markers = @(
    "PosProductionSyncQueueClaimLeaseImplementation",
    "POS Production Sync Queue Claim & Lease Implementation",
    "production sync queue claim and lease implementation documented",
    "queue claim contract documented",
    "lease ownership contract documented",
    "tenant scoped queue claim documented",
    "device scoped queue claim documented",
    "claim only after feature flag read documented",
    "claim blocked by kill switch documented",
    "claim blocked before dry-run readiness documented",
    "lease expiration documented",
    "lease renewal boundary documented",
    "stale lease recovery documented",
    "idempotency key claim guard documented",
    "correlation id claim evidence documented",
    "operator approval evidence documented",
    "no payload mutation during claim documented",
    "claim result audit evidence documented",
    "rollback-safe lease release documented",
    "operator-safe claim lease message documented",
    "no production sync execution",
    "no sync enablement",
    "no queue payload writes",
    "no item processing",
    "no server acknowledgement",
    "no runtime flag toggle",
    "no checkpoint advancement",
    "no checkout changes",
    "no inventory mutation",
    "no schema change",
    "no migrations",
    "PosProductionSyncQueueClaimLeaseImplementationStatus",
    "PosProductionSyncQueueClaimLeaseImplementationRequiredChecks",
    "PreparePosProductionSyncQueueClaimLeaseImplementationCommand",
    "30% -> 40%",
    "PHASE 6E BLOCKED"
)

foreach ($file in $files) {
    if (-not (Test-Path $file)) {
        throw "Missing required file: $file"
    }
}

$all = ""
foreach ($file in $files) {
    $all += Get-Content $file -Raw
    $all += "`n"
}

foreach ($marker in $markers) {
    if ($all -notlike "*$marker*") {
        throw "Missing PHASE 6D marker: $marker"
    }
}

Write-Host "PHASE 6D markers verified."
