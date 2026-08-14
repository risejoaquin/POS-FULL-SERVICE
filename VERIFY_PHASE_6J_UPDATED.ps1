$ErrorActionPreference = "Stop"

$files = @(
    "PosCore\Security\PosProductionSyncCanaryTenantDeviceControlledEnablement.cs",
    "PosCore\ViewModels\InventoryViewModel.cs",
    "PosCore\Views\InventoryWindow.xaml",
    "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs",
    "docs\POS_PRODUCTION_SYNC_CANARY_TENANT_DEVICE_CONTROLLED_ENABLEMENT.md",
    "docs\PHASE_6J_PRODUCTION_SYNC_CANARY_TENANT_DEVICE_CONTROLLED_ENABLEMENT.md",
    "docs\PROJECT_PROGRESS_REPORT_PHASE_6J.md",
    "README.md",
    "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md"
)

foreach ($file in $files) { if (-not (Test-Path $file)) { throw "Missing required file: $file" } }

$content = ""
foreach ($file in $files) { $content += Get-Content $file -Raw; $content += "`n" }

$markers = @(
    "PosProductionSyncCanaryTenantDeviceControlledEnablement",
    "POS Production Sync Canary Tenant/Device Controlled Enablement",
    "production sync canary tenant/device controlled enablement only",
    "RequiredCanaryTenantDeviceControlledEnablementChecks",
    "PosProductionSyncCanaryTenantDeviceControlledEnablementStatus",
    "PosProductionSyncCanaryTenantDeviceControlledEnablementRequiredChecks",
    "PreparePosProductionSyncCanaryTenantDeviceControlledEnablementCommand",
    "canary enablement contract documented",
    "tenant scoped canary enablement documented",
    "device scoped canary enablement documented",
    "feature flag prerequisite documented",
    "kill switch prerequisite documented",
    "dry-run prerequisite documented",
    "queue claim lease prerequisite documented",
    "server acknowledgement prerequisite documented",
    "checkpoint prerequisite documented",
    "conflict detection prerequisite documented",
    "dead-letter prerequisite documented",
    "runtime metrics prerequisite documented",
    "operator approval evidence documented",
    "canary blast radius documented",
    "canary rollback boundary documented",
    "canary monitoring window documented",
    "operator-safe canary enablement message documented",
    "no global sync enablement",
    "no production-wide rollout",
    "no automatic tenant expansion",
    "no automatic device expansion",
    "no queue payload mutation",
    "no unchecked checkpoint commit",
    "no conflict auto-resolution",
    "no dead-letter replay",
    "no checkout changes",
    "no inventory mutation",
    "no schema change",
    "no migrations",
    "90% -> 100%",
    "PHASE 7"
)

foreach ($marker in $markers) { if ($content -notlike "*$marker*") { throw "Missing marker: $marker" } }

Write-Host "PHASE 6J markers verified."
