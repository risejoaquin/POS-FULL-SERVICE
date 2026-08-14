$ErrorActionPreference = 'Stop'

$requiredFiles = @(
    'PosCore\Security\PosProductionSyncRuntimeMetricsEmissionImplementation.cs',
    'PosCore\ViewModels\InventoryViewModel.cs',
    'PosCore\Views\InventoryWindow.xaml',
    'PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs',
    'docs\POS_PRODUCTION_SYNC_RUNTIME_METRICS_EMISSION_IMPLEMENTATION.md',
    'docs\PHASE_6I_PRODUCTION_SYNC_RUNTIME_METRICS_EMISSION_IMPLEMENTATION.md',
    'docs\PROJECT_PROGRESS_REPORT_PHASE_6I.md',
    'README.md',
    'ROADMAP_FINALIZACION_POS_ACTUALIZADO.md'
)

foreach ($file in $requiredFiles) {
    if (-not (Test-Path $file)) {
        throw "Missing required file: $file"
    }
}

$allText = ($requiredFiles | ForEach-Object { Get-Content $_ -Raw }) -join "`n"

$markers = @(
    'PosProductionSyncRuntimeMetricsEmissionImplementation.cs',
    'POS Production Sync Runtime Metrics Emission Implementation',
    'production sync runtime metrics emission implementation',
    'RequiredRuntimeMetricsEmissionImplementationChecks',
    'PosProductionSyncRuntimeMetricsEmissionImplementationStatus',
    'PosProductionSyncRuntimeMetricsEmissionImplementationRequiredChecks',
    'PreparePosProductionSyncRuntimeMetricsEmissionImplementationCommand',
    'runtime metrics emission contract documented',
    'queue depth metric documented',
    'processing latency metric documented',
    'acknowledgement latency metric documented',
    'checkpoint lag metric documented',
    'retry rate metric documented',
    'dead-letter rate metric documented',
    'conflict rate metric documented',
    'error rate metric documented',
    'sync throughput metric documented',
    'tenant scoped metrics documented',
    'device scoped metrics documented',
    'correlation id metric evidence documented',
    'idempotency key metric evidence documented',
    'redacted metric tags documented',
    'alert threshold metric handoff documented',
    'operator dashboard metric handoff documented',
    'operator-safe runtime metrics message documented',
    'no production sync execution',
    'no sync enablement',
    'no external telemetry emission',
    'no item processing',
    'no queue payload mutation',
    'no real checkpoint commit',
    'no inventory mutation',
    'no checkout changes',
    'no schema change',
    'no migrations',
    '80% -> 90%',
    'PHASE 6J'
)

foreach ($marker in $markers) {
    if ($allText -notlike "*$marker*") {
        throw "Missing PHASE 6I marker: $marker"
    }
}

Write-Host 'PHASE 6I markers verified.'
