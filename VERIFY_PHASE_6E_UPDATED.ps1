$ErrorActionPreference = "Stop"

$requiredFiles = @(
    "PosCore\Security\PosProductionSyncServerAcknowledgementIntegrationImplementation.cs",
    "PosCore\ViewModels\InventoryViewModel.cs",
    "PosCore\Views\InventoryWindow.xaml",
    "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs",
    "docs\POS_PRODUCTION_SYNC_SERVER_ACKNOWLEDGEMENT_INTEGRATION_IMPLEMENTATION.md",
    "docs\PHASE_6E_PRODUCTION_SYNC_SERVER_ACKNOWLEDGEMENT_INTEGRATION_IMPLEMENTATION.md",
    "docs\PROJECT_PROGRESS_REPORT_PHASE_6E.md",
    "README.md",
    "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md"
)

foreach ($file in $requiredFiles) {
    if (!(Test-Path $file)) {
        throw "Missing required PHASE 6E file: $file"
    }
}

$allText = ($requiredFiles | ForEach-Object { Get-Content $_ -Raw }) -join "`n"

$requiredMarkers = @(
    "PosProductionSyncServerAcknowledgementIntegrationImplementation",
    "POS Production Sync Server Acknowledgement Integration Implementation",
    "production sync server acknowledgement integration implementation documented",
    "server acknowledgement contract documented",
    "acknowledgement request envelope documented",
    "acknowledgement response envelope documented",
    "acknowledgement status validation documented",
    "durable acknowledgement evidence documented",
    "tenant scoped acknowledgement documented",
    "device scoped acknowledgement documented",
    "queue item acknowledgement matching documented",
    "lease ownership acknowledgement guard documented",
    "idempotency key acknowledgement guard documented",
    "correlation id acknowledgement evidence documented",
    "retryable acknowledgement failure documented",
    "terminal acknowledgement failure documented",
    "checkpoint blocked until durable acknowledgement documented",
    "operator approval evidence documented",
    "no acknowledgement transmission during preparation documented",
    "operator-safe acknowledgement message documented",
    "PosProductionSyncServerAcknowledgementIntegrationImplementationStatus",
    "PosProductionSyncServerAcknowledgementIntegrationImplementationRequiredChecks",
    "PreparePosProductionSyncServerAcknowledgementIntegrationImplementationCommand",
    "no production sync execution",
    "no sync enablement",
    "no real server acknowledgement send",
    "no checkpoint advancement",
    "no queue payload writes",
    "no item processing",
    "no runtime flag toggle",
    "no checkout changes",
    "no inventory mutation",
    "no schema change",
    "no migrations",
    "40% -> 50%",
    "PHASE 6F"
)

foreach ($marker in $requiredMarkers) {
    if ($allText -notlike "*$marker*") {
        throw "Missing PHASE 6E marker: $marker"
    }
}

Write-Host "PHASE 6E markers verified."
