$ErrorActionPreference = "Stop"

$requiredFiles = @(
    "PosCore\Security\PosProductionSyncDeadLetterQueuePersistenceImplementation.cs",
    "PosCore\ViewModels\InventoryViewModel.cs",
    "PosCore\Views\InventoryWindow.xaml",
    "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs",
    "docs\POS_PRODUCTION_SYNC_DEAD_LETTER_QUEUE_PERSISTENCE_IMPLEMENTATION.md",
    "docs\PHASE_6H_PRODUCTION_SYNC_DEAD_LETTER_QUEUE_PERSISTENCE_IMPLEMENTATION.md",
    "docs\PROJECT_PROGRESS_REPORT_PHASE_6H.md",
    "README.md",
    "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md"
)

foreach ($file in $requiredFiles) {
    if (!(Test-Path $file)) {
        throw "Missing required PHASE 6H file: $file"
    }
}

$allText = ""
foreach ($file in $requiredFiles) {
    $allText += Get-Content $file -Raw
    $allText += "`n"
}

$requiredMarkers = @(
    "PosProductionSyncDeadLetterQueuePersistenceImplementation",
    "POS Production Sync Dead-Letter Queue Persistence Implementation",
    "production sync dead-letter queue persistence implementation documented",
    "RequiredDeadLetterQueuePersistenceImplementationChecks",
    "PosProductionSyncDeadLetterQueuePersistenceImplementationStatus",
    "PosProductionSyncDeadLetterQueuePersistenceImplementationRequiredChecks",
    "PreparePosProductionSyncDeadLetterQueuePersistenceImplementationCommand",
    "dead-letter queue persistence contract documented",
    "dead-letter record envelope documented",
    "dead-letter reason code documented",
    "tenant scoped dead-letter persistence documented",
    "device scoped dead-letter persistence documented",
    "queue item dead-letter matching documented",
    "lease ownership dead-letter guard documented",
    "idempotency key dead-letter guard documented",
    "correlation id dead-letter evidence documented",
    "conflict detection prerequisite documented",
    "manual intervention prerequisite documented",
    "retry exhaustion prerequisite documented",
    "payload snapshot redaction documented",
    "dead-letter audit evidence documented",
    "dead-letter replay prohibition documented",
    "operator approval evidence documented",
    "operator-safe dead-letter message documented",
    "no production sync execution",
    "no sync enablement",
    "no automatic replay",
    "no item processing",
    "no queue payload mutation",
    "no real checkpoint commit",
    "no inventory mutation",
    "no checkout changes",
    "no schema change",
    "no migrations",
    "70% -> 80%",
    "PHASE 6I"
)

foreach ($marker in $requiredMarkers) {
    if ($allText -notlike "*$marker*") {
        throw "Missing PHASE 6H marker: $marker"
    }
}

Write-Host "PHASE 6H markers verified."
