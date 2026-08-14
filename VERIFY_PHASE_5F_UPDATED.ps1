$ErrorActionPreference = "Stop"

$requiredFiles = @(
    "PosCore\Security\PosProductionSyncConflictResolutionExecutionGateBaseline.cs",
    "docs\POS_PRODUCTION_SYNC_CONFLICT_RESOLUTION_EXECUTION_GATE_BASELINE.md",
    "docs\PHASE_5F_PRODUCTION_SYNC_CONFLICT_RESOLUTION_EXECUTION_GATE_BASELINE.md",
    "docs\PROJECT_PROGRESS_REPORT_PHASE_5F.md"
)

foreach ($file in $requiredFiles) {
    if (!(Test-Path $file)) {
        throw "Missing PHASE 5F file: $file"
    }
}

$allText = @(
    Get-Content "PosCore\Security\PosProductionSyncConflictResolutionExecutionGateBaseline.cs" -Raw
    Get-Content "PosCore\ViewModels\InventoryViewModel.cs" -Raw
    Get-Content "PosCore\Views\InventoryWindow.xaml" -Raw
    Get-Content "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" -Raw
    Get-Content "docs\POS_PRODUCTION_SYNC_CONFLICT_RESOLUTION_EXECUTION_GATE_BASELINE.md" -Raw
    Get-Content "docs\PHASE_5F_PRODUCTION_SYNC_CONFLICT_RESOLUTION_EXECUTION_GATE_BASELINE.md" -Raw
    Get-Content "docs\PROJECT_PROGRESS_REPORT_PHASE_5F.md" -Raw
) -join "`n"

$checks = @(
    "PosProductionSyncConflictResolutionExecutionGateStatus",
    "PosProductionSyncConflictResolutionExecutionGateBaselineReady",
    "PosProductionSyncConflictResolutionExecutionGateReviewedAt",
    "PosProductionSyncConflictResolutionExecutionGateRequiredChecks",
    "PosProductionSyncConflictResolutionExecutionGateSummary",
    "PreparePosProductionSyncConflictResolutionExecutionGateBaselineCommand",
    "POS Production Sync Conflict Resolution Execution Gate Baseline",
    "production sync conflict resolution execution gate baseline only",
    "conflict resolution execution gate documented",
    "server acknowledgement prerequisite documented",
    "checkpoint commit prerequisite documented",
    "conflict type classification documented",
    "deterministic resolution rule documented",
    "manual approval requirement documented",
    "tenant device scope validation documented",
    "idempotency key evidence documented",
    "inventory mutation prohibition before approval documented",
    "rollback plan prerequisite documented",
    "dead-letter handoff documented",
    "manual recovery handoff documented",
    "audit log requirement documented",
    "no production sync execution",
    "no conflict resolution execution",
    "no queue writes",
    "no checkpoint confirmation",
    "no inventory mutation",
    "no checkout changes",
    "Phase5F_Should_Be_Documented_As_Production_Sync_ConflictResolutionExecutionGate_Baseline_Only"
)

foreach ($check in $checks) {
    if ($allText -notlike "*$check*") {
        throw "Missing PHASE 5F marker: $check"
    }
}

Write-Host "PHASE 5F markers verified."
