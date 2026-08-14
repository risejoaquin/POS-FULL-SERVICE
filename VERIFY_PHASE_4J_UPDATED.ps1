$ErrorActionPreference = "Stop"

$requiredFiles = @(
    "PosCore/Security/PosOfflineSyncOperationalClosureBaseline.cs",
    "PosCore/ViewModels/InventoryViewModel.cs",
    "PosCore/Views/InventoryWindow.xaml",
    "PosInfrastructure.Tests/Architecture/InventoryLedgerConcurrencyBaselineTests.cs",
    "docs/POS_OFFLINE_SYNC_OPERATIONAL_CLOSURE_BASELINE.md",
    "docs/PHASE_4J_OFFLINE_SYNC_OPERATIONAL_CLOSURE.md",
    "docs/PROJECT_PROGRESS_REPORT_PHASE_4J.md"
)

foreach ($file in $requiredFiles) {
    if (!(Test-Path $file)) {
        throw "Missing PHASE 4J file: $file"
    }
}

$allText = ($requiredFiles | ForEach-Object { Get-Content $_ -Raw }) -join "`n"

$checks = @(
    "PosOfflineSyncOperationalClosureBaseline",
    "POS Offline Sync Operational Closure Baseline",
    "offline sync operational closure baseline only",
    "PosOfflineSyncOperationalClosureStatus",
    "PosOfflineSyncOperationalClosureRequiredChecks",
    "PreparePosOfflineSyncOperationalClosureBaselineCommand",
    "final readiness checklist",
    "evidence archive",
    "manual recovery closure criteria",
    "queue health closure criteria",
    "checkpoint closure criteria",
    "correlation evidence closure",
    "tenant device ownership closure",
    "idempotency closure",
    "retry backoff closure",
    "conflict detection closure",
    "observability closure",
    "production sync enablement gate",
    "rollback escalation path",
    "operator-safe closure message",
    "no production sync execution",
    "no queue writes",
    "no operational closure execution",
    "no checkpoint advancement",
    "no inventory mutation",
    "no checkout changes",
    "no schema change",
    "no migrations"
)

foreach ($check in $checks) {
    if ($allText -notmatch [regex]::Escape($check)) {
        throw "Missing PHASE 4J marker: $check"
    }
}

Write-Host "PHASE 4J markers verified."
