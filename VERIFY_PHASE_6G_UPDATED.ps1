$ErrorActionPreference = "Stop"

$requiredFiles = @(
    "PosCore\Security\PosProductionSyncConflictDetectionRuntimeImplementation.cs",
    "PosCore\ViewModels\InventoryViewModel.cs",
    "PosCore\Views\InventoryWindow.xaml",
    "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs",
    "docs\POS_PRODUCTION_SYNC_CONFLICT_DETECTION_RUNTIME_IMPLEMENTATION.md",
    "docs\PHASE_6G_PRODUCTION_SYNC_CONFLICT_DETECTION_RUNTIME_IMPLEMENTATION.md",
    "docs\PROJECT_PROGRESS_REPORT_PHASE_6G.md",
    "README.md",
    "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md"
)

foreach ($file in $requiredFiles) {
    if (!(Test-Path $file)) {
        throw "Missing required PHASE 6G file: $file"
    }
}

$markers = @(
    "PosProductionSyncConflictDetectionRuntimeImplementation",
    "POS Production Sync Conflict Detection Runtime Implementation",
    "production sync conflict detection runtime implementation documented",
    "RequiredConflictDetectionRuntimeImplementationChecks",
    "PosProductionSyncConflictDetectionRuntimeImplementationStatus",
    "PosProductionSyncConflictDetectionRuntimeImplementationRequiredChecks",
    "PreparePosProductionSyncConflictDetectionRuntimeImplementationCommand",
    "conflict detection contract documented",
    "local version evidence documented",
    "server version evidence documented",
    "checkpoint comparison documented",
    "tenant scoped conflict detection documented",
    "device scoped conflict detection documented",
    "queue item conflict matching documented",
    "lease ownership conflict guard documented",
    "idempotency key conflict guard documented",
    "correlation id conflict evidence documented",
    "durable acknowledgement prerequisite documented",
    "checkpoint prerequisite documented",
    "conflict classification documented",
    "manual resolution handoff documented",
    "operator approval evidence documented",
    "no automatic conflict resolution",
    "no production sync execution",
    "no sync enablement",
    "no real checkpoint commit",
    "no queue payload writes",
    "no item processing",
    "no inventory mutation",
    "no checkout changes",
    "no schema change",
    "no migrations",
    "60% -> 70%",
    "PHASE 6H"
)

$allText = ""
foreach ($file in $requiredFiles) {
    $allText += "`n" + (Get-Content $file -Raw)
}

foreach ($marker in $markers) {
    if ($allText.IndexOf($marker, [StringComparison]::OrdinalIgnoreCase) -lt 0) {
        throw "Missing PHASE 6G marker: $marker"
    }
}

Write-Host "PHASE 6G markers verified."
