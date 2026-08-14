$ErrorActionPreference = "Stop"

$requiredPaths = @(
    "PosCore\Security\PosOfflineSyncObservabilityCorrelationBaseline.cs",
    "docs\POS_OFFLINE_SYNC_OBSERVABILITY_CORRELATION_BASELINE.md",
    "docs\PHASE_4H_OFFLINE_SYNC_OBSERVABILITY_CORRELATION_BASELINE.md",
    "docs\PROJECT_PROGRESS_REPORT_PHASE_4H.md",
    "PosCore\ViewModels\InventoryViewModel.cs",
    "PosCore\Views\InventoryWindow.xaml",
    "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs"
)

foreach ($path in $requiredPaths) {
    if (-not (Test-Path $path)) {
        throw "Missing required PHASE 4H file: $path"
    }
}

$checks = @(
    "PosOfflineSyncObservabilityCorrelationBaseline",
    "POS Offline Sync Observability & Correlation Baseline",
    "PosOfflineSyncObservabilityCorrelationStatus",
    "PosOfflineSyncObservabilityCorrelationRequiredChecks",
    "PreparePosOfflineSyncObservabilityCorrelationBaselineCommand",
    "correlation id strategy documented",
    "tenant id log scope documented",
    "device id log scope documented",
    "sync operation id documented",
    "queue item id log scope documented",
    "idempotency key log scope documented",
    "sensitive data redaction documented",
    "no production sync execution",
    "no queue writes",
    "no telemetry emission",
    "no inventory mutation",
    "70% -> 80%"
)

$allText = (Get-Content "PosCore\Security\PosOfflineSyncObservabilityCorrelationBaseline.cs", "docs\POS_OFFLINE_SYNC_OBSERVABILITY_CORRELATION_BASELINE.md", "docs\PHASE_4H_OFFLINE_SYNC_OBSERVABILITY_CORRELATION_BASELINE.md", "docs\PROJECT_PROGRESS_REPORT_PHASE_4H.md", "PosCore\ViewModels\InventoryViewModel.cs", "PosCore\Views\InventoryWindow.xaml", "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" -Raw) -join "`n"

foreach ($check in $checks) {
    if ($allText -notlike "*$check*") {
        throw "Missing PHASE 4H marker: $check"
    }
}

Write-Host "PHASE 4H markers verified."
