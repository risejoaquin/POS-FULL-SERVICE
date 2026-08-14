$ErrorActionPreference = "Stop"

$requiredPaths = @(
    "PosCore\Security\PosProductionSyncServerAcknowledgementCheckpointCommitBaseline.cs",
    "docs\POS_PRODUCTION_SYNC_SERVER_ACKNOWLEDGEMENT_CHECKPOINT_COMMIT_BASELINE.md",
    "docs\PHASE_5E_PRODUCTION_SYNC_SERVER_ACKNOWLEDGEMENT_CHECKPOINT_COMMIT_BASELINE.md",
    "docs\PROJECT_PROGRESS_REPORT_PHASE_5E.md",
    "PosCore\ViewModels\InventoryViewModel.cs",
    "PosCore\Views\InventoryWindow.xaml",
    "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs"
)

foreach ($path in $requiredPaths) {
    if (-not (Test-Path $path)) {
        throw "Missing PHASE 5E file: $path"
    }
}

$allText = ($requiredPaths | ForEach-Object { Get-Content $_ -Raw }) -join "`n"

$requiredMarkers = @(
    "PosProductionSyncServerAcknowledgementCheckpointCommitBaseline",
    "POS Production Sync Server Acknowledgement & Checkpoint Commit Baseline",
    "PosProductionSyncServerAcknowledgementCheckpointCommitStatus",
    "PosProductionSyncServerAcknowledgementCheckpointCommitRequiredChecks",
    "PreparePosProductionSyncServerAcknowledgementCheckpointCommitBaselineCommand",
    "server acknowledgement contract documented",
    "acknowledgement status validation documented",
    "durable acknowledgement evidence documented",
    "correlation id acknowledgement matching documented",
    "idempotency key acknowledgement matching documented",
    "tenant id acknowledgement matching documented",
    "device id acknowledgement matching documented",
    "queue item id acknowledgement matching documented",
    "checkpoint commit boundary documented",
    "no checkpoint commit on partial failure documented",
    "no production sync execution",
    "no queue writes",
    "no acknowledgement send",
    "no checkpoint commit",
    "no inventory mutation",
    "40% -> 50%"
)

foreach ($marker in $requiredMarkers) {
    if ($allText -notlike "*$marker*") {
        throw "Missing PHASE 5E marker: $marker"
    }
}

Write-Host "PHASE 5E markers verified."
