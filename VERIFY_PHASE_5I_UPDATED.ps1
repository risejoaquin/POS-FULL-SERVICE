$ErrorActionPreference = "Stop"

$paths = @(
    "PosCore\Security\PosProductionSyncOperationalRunbookSupportHandoffBaseline.cs",
    "PosCore\ViewModels\InventoryViewModel.cs",
    "PosCore\Views\InventoryWindow.xaml",
    "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs",
    "docs\POS_PRODUCTION_SYNC_OPERATIONAL_RUNBOOK_SUPPORT_HANDOFF_BASELINE.md",
    "docs\PHASE_5I_PRODUCTION_SYNC_OPERATIONAL_RUNBOOK_SUPPORT_HANDOFF_BASELINE.md",
    "docs\PROJECT_PROGRESS_REPORT_PHASE_5I.md",
    "README.md",
    "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md"
)

foreach ($path in $paths) {
    if (!(Test-Path $path)) {
        throw "Missing PHASE 5I required file: $path"
    }
}

$allText = ($paths | ForEach-Object { Get-Content $_ -Raw }) -join "`n"

$requiredMarkers = @(
    "PosProductionSyncOperationalRunbookSupportHandoffBaseline",
    "POS Production Sync Operational Runbook & Support Handoff Baseline",
    "production sync operational runbook and support handoff baseline only",
    "RequiredOperationalRunbookSupportHandoffChecks",
    "PosProductionSyncOperationalRunbookSupportHandoffStatus",
    "PosProductionSyncOperationalRunbookSupportHandoffRequiredChecks",
    "PreparePosProductionSyncOperationalRunbookSupportHandoffBaselineCommand",
    "operational runbook documented",
    "support handoff workflow documented",
    "incident severity classification documented",
    "first response checklist documented",
    "escalation matrix documented",
    "support evidence package documented",
    "queue snapshot evidence documented",
    "runtime metrics evidence documented",
    "correlation id evidence documented",
    "tenant/device evidence documented",
    "idempotency key evidence documented",
    "checkpoint state evidence documented",
    "feature flag state evidence documented",
    "kill switch state evidence documented",
    "dead-letter state evidence documented",
    "operator communication template documented",
    "support closure criteria documented",
    "operator-safe runbook message documented",
    "no production sync execution",
    "no queue writes",
    "no support handoff execution",
    "no runtime operation change",
    "no checkpoint commit",
    "no checkout changes",
    "no inventory mutation",
    "no schema change",
    "no migrations",
    "80% -> 90%",
    "PHASE 5J"
)

foreach ($marker in $requiredMarkers) {
    if ($allText -notlike "*$marker*") {
        throw "Missing PHASE 5I marker: $marker"
    }
}

Write-Host "PHASE 5I markers verified."
