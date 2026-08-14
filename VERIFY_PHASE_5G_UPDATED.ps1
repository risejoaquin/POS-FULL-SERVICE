$ErrorActionPreference = "Stop"

$requiredPaths = @(
    "PosCore\Security\PosProductionSyncDeadLetterManualInterventionBaseline.cs",
    "docs\POS_PRODUCTION_SYNC_DEAD_LETTER_QUEUE_MANUAL_INTERVENTION_BASELINE.md",
    "docs\PHASE_5G_PRODUCTION_SYNC_DEAD_LETTER_QUEUE_MANUAL_INTERVENTION_BASELINE.md",
    "docs\PROJECT_PROGRESS_REPORT_PHASE_5G.md",
    "PosCore\ViewModels\InventoryViewModel.cs",
    "PosCore\Views\InventoryWindow.xaml",
    "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs"
)

foreach ($path in $requiredPaths) {
    if (!(Test-Path $path)) {
        throw "Missing required PHASE 5G file: $path"
    }
}

$allText = ($requiredPaths | ForEach-Object { Get-Content $_ -Raw }) -join "`n"

$requiredMarkers = @(
    "PosProductionSyncDeadLetterManualInterventionBaseline",
    "POS Production Sync Dead-Letter Queue & Manual Intervention Baseline",
    "PosProductionSyncDeadLetterManualInterventionStatus",
    "PosProductionSyncDeadLetterManualInterventionRequiredChecks",
    "PreparePosProductionSyncDeadLetterManualInterventionBaselineCommand",
    "dead-letter queue contract documented",
    "terminal failure criteria documented",
    "manual intervention workflow documented",
    "operator assignment requirement documented",
    "support escalation requirement documented",
    "evidence package requirement documented",
    "tenant device scope evidence documented",
    "idempotency key evidence documented",
    "checkpoint freeze requirement documented",
    "audit trail requirement documented",
    "operator-safe dead-letter message documented",
    "no production sync execution",
    "no queue writes",
    "no dead-letter move",
    "no manual intervention execution",
    "no inventory mutation",
    "60% -> 70%"
)

foreach ($marker in $requiredMarkers) {
    if ($allText -notlike "*$marker*") {
        throw "Missing PHASE 5G marker: $marker"
    }
}

Write-Host "PHASE 5G markers verified."
