$ErrorActionPreference = "Stop"

$paths = @(
    "PosCore\Security\PosOfflineSyncManualRecoveryRunbookBaseline.cs",
    "PosCore\ViewModels\InventoryViewModel.cs",
    "PosCore\Views\InventoryWindow.xaml",
    "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs",
    "docs\POS_OFFLINE_SYNC_MANUAL_RECOVERY_RUNBOOK_BASELINE.md",
    "docs\PHASE_4I_OFFLINE_SYNC_MANUAL_RECOVERY_RUNBOOK.md",
    "docs\PROJECT_PROGRESS_REPORT_PHASE_4I.md",
    "README.md",
    "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md"
)

$allText = ($paths | ForEach-Object {
    if (!(Test-Path $_)) {
        throw "Missing PHASE 4I file: $_"
    }
    Get-Content $_ -Raw
}) -join "`n"

$checks = @(
    "PosOfflineSyncManualRecoveryRunbookBaseline",
    "POS Offline Sync Manual Recovery Runbook Baseline",
    "offline sync manual recovery runbook baseline only",
    "RequiredManualRecoveryRunbookChecks",
    "PosOfflineSyncManualRecoveryRunbookStatus",
    "PosOfflineSyncManualRecoveryRunbookRequiredChecks",
    "PreparePosOfflineSyncManualRecoveryRunbookBaselineCommand",
    "manual recovery entry criteria documented",
    "operator triage workflow documented",
    "queue snapshot before recovery documented",
    "checkpoint freeze before recovery documented",
    "correlation id evidence collection documented",
    "tenant id evidence collection documented",
    "device id evidence collection documented",
    "idempotency key validation documented",
    "retry/backoff state review documented",
    "conflict detection state review documented",
    "dead-letter review workflow documented",
    "support handoff package documented",
    "operator-safe recovery message documented",
    "rollback prohibition documented",
    "no production sync execution",
    "no queue writes",
    "no manual recovery execution",
    "no checkpoint advancement",
    "no inventory mutation",
    "no checkout changes",
    "no schema change",
    "no migrations",
    "80% -> 90%",
    "PHASE 4J BLOCKED"
)

foreach ($check in $checks) {
    if ($allText -notlike "*$check*") {
        throw "Missing PHASE 4I marker: $check"
    }
}

Write-Host "PHASE 4I markers verified."
