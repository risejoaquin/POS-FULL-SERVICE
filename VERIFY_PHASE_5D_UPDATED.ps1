$ErrorActionPreference = "Stop"

$requiredFiles = @(
    "PosCore\Security\PosProductionSyncQueueProcessorExecutionBaseline.cs",
    "docs\POS_PRODUCTION_SYNC_QUEUE_PROCESSOR_EXECUTION_BASELINE.md",
    "docs\PHASE_5D_PRODUCTION_SYNC_QUEUE_PROCESSOR_EXECUTION_BASELINE.md",
    "docs\PROJECT_PROGRESS_REPORT_PHASE_5D.md"
)

foreach ($file in $requiredFiles) {
    if (!(Test-Path $file)) {
        throw "Missing PHASE 5D file: $file"
    }
}

$allText = @(
    Get-Content "PosCore\Security\PosProductionSyncQueueProcessorExecutionBaseline.cs" -Raw
    Get-Content "PosCore\ViewModels\InventoryViewModel.cs" -Raw
    Get-Content "PosCore\Views\InventoryWindow.xaml" -Raw
    Get-Content "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" -Raw
    Get-Content "docs\POS_PRODUCTION_SYNC_QUEUE_PROCESSOR_EXECUTION_BASELINE.md" -Raw
    Get-Content "docs\PHASE_5D_PRODUCTION_SYNC_QUEUE_PROCESSOR_EXECUTION_BASELINE.md" -Raw
    Get-Content "docs\PROJECT_PROGRESS_REPORT_PHASE_5D.md" -Raw
) -join "`n"

$checks = @(
    "PosProductionSyncQueueProcessorExecutionStatus",
    "PosProductionSyncQueueProcessorExecutionBaselineReady",
    "PosProductionSyncQueueProcessorExecutionReviewedAt",
    "PosProductionSyncQueueProcessorExecutionRequiredChecks",
    "PosProductionSyncQueueProcessorExecutionSummary",
    "PreparePosProductionSyncQueueProcessorExecutionBaselineCommand",
    "POS Production Sync Queue Processor Execution Baseline",
    "production sync queue processor execution baseline only",
    "queue processor ownership documented",
    "feature flag prerequisite documented",
    "kill switch prerequisite documented",
    "canary rollout prerequisite documented",
    "tenant device scope validation documented",
    "queue claim strategy documented",
    "idempotency enforcement documented",
    "checkpoint commit boundary documented",
    "dead-letter handoff documented",
    "manual recovery handoff documented",
    "dry-run evidence requirement documented",
    "no production sync execution",
    "no queue writes",
    "no queue item claim",
    "no inventory mutation",
    "no checkout changes",
    "Phase5D_Should_Be_Documented_As_Production_Sync_QueueProcessorExecution_Baseline_Only"
)

foreach ($check in $checks) {
    if ($allText -notlike "*$check*") {
        throw "Missing PHASE 5D marker: $check"
    }
}

Write-Host "PHASE 5D markers verified."
