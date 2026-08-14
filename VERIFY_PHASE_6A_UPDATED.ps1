$ErrorActionPreference = "Stop"

$requiredFiles = @(
    "PosCore\Security\PosProductionSyncFeatureFlagPersistenceImplementation.cs",
    "docs\POS_PRODUCTION_SYNC_FEATURE_FLAG_PERSISTENCE_IMPLEMENTATION.md",
    "docs\PHASE_6A_PRODUCTION_SYNC_FEATURE_FLAG_PERSISTENCE_IMPLEMENTATION.md",
    "docs\PROJECT_PROGRESS_REPORT_PHASE_6A.md",
    "PosCore\ViewModels\InventoryViewModel.cs",
    "PosCore\Views\InventoryWindow.xaml",
    "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs"
)

foreach ($file in $requiredFiles) {
    if (-not (Test-Path $file)) {
        throw "Required file missing: $file"
    }
}

$allText = ($requiredFiles | ForEach-Object { Get-Content $_ -Raw }) -join "`n"

$markers = @(
    "PosProductionSyncFeatureFlagPersistenceImplementation",
    "POS Production Sync Feature Flag Persistence Implementation",
    "PosProductionSyncFeatureFlagPersistenceImplementationStatus",
    "PosProductionSyncFeatureFlagPersistenceImplementationRequiredChecks",
    "PreparePosProductionSyncFeatureFlagPersistenceImplementationCommand",
    "production sync feature flag persistence implementation documented",
    "tenant scoped feature flag persistence documented",
    "device scoped feature flag persistence documented",
    "default disabled state documented",
    "operator approval evidence documented",
    "feature flag versioning documented",
    "kill switch precedence documented",
    "canary prerequisite documented",
    "idempotent feature flag write documented",
    "feature flag persistence verification documented",
    "no production sync execution",
    "no sync enablement",
    "no queue writes",
    "no runtime flag toggle",
    "no checkpoint advancement",
    "no inventory mutation",
    "0% -> 10%"
)

foreach ($marker in $markers) {
    if ($allText -notlike "*$marker*") {
        throw "Required marker missing: $marker"
    }
}

Write-Host "PHASE 6A markers verified."
