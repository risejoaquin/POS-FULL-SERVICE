$ErrorActionPreference = "Stop"

$files = @(
    "PosCore\Security\PosProductionSyncQueueProcessorDryRunExecutionImplementation.cs",
    "PosCore\ViewModels\InventoryViewModel.cs",
    "PosCore\Views\InventoryWindow.xaml",
    "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs",
    "docs\POS_PRODUCTION_SYNC_QUEUE_PROCESSOR_DRY_RUN_EXECUTION_IMPLEMENTATION.md",
    "docs\PHASE_6C_PRODUCTION_SYNC_QUEUE_PROCESSOR_DRY_RUN_EXECUTION_IMPLEMENTATION.md",
    "docs\PROJECT_PROGRESS_REPORT_PHASE_6C.md",
    "README.md",
    "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md"
)

$markers = @(
    "PosProductionSyncQueueProcessorDryRunExecutionImplementation",
    "POS Production Sync Queue Processor Dry-Run Execution Implementation",
    "production sync queue processor dry-run execution implementation documented",
    "queue processor dry-run mode documented",
    "read-only queue scan documented",
    "no queue claim documented",
    "no queue writes documented",
    "no item status transition documented",
    "no checkpoint advancement documented",
    "feature flag read requirement documented",
    "kill switch enforcement requirement documented",
    "tenant scoped dry-run documented",
    "device scoped dry-run documented",
    "idempotency key inspection documented",
    "correlation id dry-run evidence documented",
    "dry-run decision evidence documented",
    "operator approval evidence documented",
    "dry-run result summary documented",
    "rollback-safe dry-run documented",
    "operator-safe dry-run message documented",
    "no production sync execution",
    "no sync enablement",
    "no queue claim",
    "no queue writes",
    "no runtime flag toggle",
    "no checkpoint advancement",
    "no checkout changes",
    "no inventory mutation",
    "no schema change",
    "no migrations",
    "PosProductionSyncQueueProcessorDryRunExecutionImplementationStatus",
    "PosProductionSyncQueueProcessorDryRunExecutionImplementationRequiredChecks",
    "PreparePosProductionSyncQueueProcessorDryRunExecutionImplementationCommand",
    "20% -> 30%",
    "PHASE 6D BLOCKED"
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
        throw "Missing PHASE 6C marker: $marker"
    }
}

Write-Host "PHASE 6C markers verified."
