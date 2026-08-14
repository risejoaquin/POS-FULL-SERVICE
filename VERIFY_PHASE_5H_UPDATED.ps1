$ErrorActionPreference = "Stop"

$paths = @(
    "PosCore\Security\PosProductionSyncObservabilityRuntimeMetricsBaseline.cs",
    "PosCore\ViewModels\InventoryViewModel.cs",
    "PosCore\Views\InventoryWindow.xaml",
    "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs",
    "docs\POS_PRODUCTION_SYNC_OBSERVABILITY_RUNTIME_METRICS_BASELINE.md",
    "docs\PHASE_5H_PRODUCTION_SYNC_OBSERVABILITY_RUNTIME_METRICS_BASELINE.md",
    "docs\PROJECT_PROGRESS_REPORT_PHASE_5H.md",
    "README.md",
    "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md"
)

foreach ($path in $paths) {
    if (-not (Test-Path $path)) {
        throw "Missing PHASE 5H file: $path"
    }
}

$allText = ($paths | ForEach-Object { Get-Content $_ -Raw }) -join "`n"

$markers = @(
    "PosProductionSyncObservabilityRuntimeMetricsBaseline",
    "POS Production Sync Observability Runtime Metrics Baseline",
    "production sync observability runtime metrics baseline only",
    "RequiredObservabilityRuntimeMetricsChecks",
    "PosProductionSyncObservabilityRuntimeMetricsStatus",
    "PosProductionSyncObservabilityRuntimeMetricsRequiredChecks",
    "PreparePosProductionSyncObservabilityRuntimeMetricsBaselineCommand",
    "runtime metrics contract documented",
    "queue depth metric documented",
    "processing latency metric documented",
    "acknowledgement latency metric documented",
    "checkpoint lag metric documented",
    "retry rate metric documented",
    "dead-letter rate metric documented",
    "conflict rate metric documented",
    "error rate metric documented",
    "sync throughput metric documented",
    "tenant/device metric dimensions documented",
    "correlation id trace metric documented",
    "sensitive data redaction documented",
    "alert threshold requirement documented",
    "operator dashboard requirement documented",
    "operator-safe metrics message documented",
    "no production sync execution",
    "no queue writes",
    "no runtime metrics emission",
    "no alerting configuration change",
    "no checkpoint commit",
    "no checkout changes",
    "no inventory mutation",
    "no schema change",
    "no migrations",
    "70% -> 80%",
    "PHASE 5I BLOCKED"
)

foreach ($marker in $markers) {
    if ($allText -notmatch [regex]::Escape($marker)) {
        throw "Missing PHASE 5H marker: $marker"
    }
}

Write-Host "PHASE 5H markers verified."
