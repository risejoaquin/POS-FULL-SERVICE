$ErrorActionPreference = "Stop"

$requiredFiles = @(
    "PosCore\Security\PosProductionSyncCheckpointCommitRuntimeImplementation.cs",
    "PosCore\ViewModels\InventoryViewModel.cs",
    "PosCore\Views\InventoryWindow.xaml",
    "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs",
    "docs\POS_PRODUCTION_SYNC_CHECKPOINT_COMMIT_RUNTIME_IMPLEMENTATION.md",
    "docs\PHASE_6F_PRODUCTION_SYNC_CHECKPOINT_COMMIT_RUNTIME_IMPLEMENTATION.md",
    "docs\PROJECT_PROGRESS_REPORT_PHASE_6F.md",
    "README.md",
    "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md"
)

foreach ($file in $requiredFiles) {
    if (!(Test-Path $file)) { throw "Missing required PHASE 6F file: $file" }
}

$allText = ($requiredFiles | ForEach-Object { Get-Content $_ -Raw }) -join "`n"
$requiredMarkers = @(
    "PosProductionSyncCheckpointCommitRuntimeImplementation",
    "POS Production Sync Checkpoint Commit Runtime Implementation",
    "production sync checkpoint commit runtime implementation documented",
    "checkpoint commit contract documented",
    "durable acknowledgement prerequisite documented",
    "checkpoint candidate state documented",
    "checkpoint monotonicity guard documented",
    "tenant scoped checkpoint documented",
    "device scoped checkpoint documented",
    "queue item checkpoint matching documented",
    "lease ownership checkpoint guard documented",
    "idempotency key checkpoint guard documented",
    "correlation id checkpoint evidence documented",
    "last success state update boundary documented",
    "checkpoint rollback boundary documented",
    "retryable checkpoint failure documented",
    "terminal checkpoint failure documented",
    "checkpoint audit evidence documented",
    "operator approval evidence documented",
    "no checkpoint commit during preparation documented",
    "operator-safe checkpoint message documented",
    "PosProductionSyncCheckpointCommitRuntimeImplementationStatus",
    "PosProductionSyncCheckpointCommitRuntimeImplementationRequiredChecks",
    "PreparePosProductionSyncCheckpointCommitRuntimeImplementationCommand",
    "no production sync execution",
    "no sync enablement",
    "no real checkpoint commit",
    "no queue payload writes",
    "no item processing",
    "no real server acknowledgement send",
    "no runtime flag toggle",
    "no checkout changes",
    "no inventory mutation",
    "no schema change",
    "no migrations",
    "50% -> 60%",
    "PHASE 6G"
)
foreach ($marker in $requiredMarkers) {
    if ($allText -notlike "*$marker*") { throw "Missing PHASE 6F marker: $marker" }
}
Write-Host "PHASE 6F markers verified."
