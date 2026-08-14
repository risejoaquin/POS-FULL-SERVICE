$ErrorActionPreference = "Stop"

$required = @(
    "PosCore/Security/PosOfflineSyncCheckpointLastSuccessStateBaseline.cs",
    "docs/POS_OFFLINE_SYNC_CHECKPOINT_LAST_SUCCESS_STATE_BASELINE.md",
    "docs/PHASE_4F_POS_OFFLINE_SYNC_CHECKPOINT_LAST_SUCCESS_STATE_BASELINE.md",
    "docs/PROJECT_PROGRESS_REPORT_PHASE_4F.md"
)

foreach ($file in $required) {
    if (!(Test-Path $file)) { throw "Missing required file: $file" }
}

$vm = Get-Content "PosCore/ViewModels/InventoryViewModel.cs" -Raw
$xaml = Get-Content "PosCore/Views/InventoryWindow.xaml" -Raw
$tests = Get-Content "PosInfrastructure.Tests/Architecture/InventoryLedgerConcurrencyBaselineTests.cs" -Raw
$doc = Get-Content "docs/POS_OFFLINE_SYNC_CHECKPOINT_LAST_SUCCESS_STATE_BASELINE.md" -Raw

$markers = @(
    "PosOfflineSyncCheckpointLastSuccessStateBaseline",
    "PosOfflineSyncCheckpointLastSuccessStateStatus",
    "PosOfflineSyncCheckpointLastSuccessStateRequiredChecks",
    "PreparePosOfflineSyncCheckpointLastSuccessStateBaselineCommand",
    "checkpoint strategy documented",
    "last successful sync timestamp",
    "last successful queue item id",
    "server cursor",
    "resume from checkpoint",
    "atomic checkpoint update",
    "duplicate replay prevention",
    "no production sync execution",
    "no queue writes",
    "no checkpoint advancement",
    "no inventory mutation",
    "no checkout changes",
    "PHASE 4F"
)

$all = $vm + $xaml + $tests + $doc
foreach ($marker in $markers) {
    if ($all -notmatch [regex]::Escape($marker)) { throw "Missing marker: $marker" }
}

Write-Host "PHASE 4F markers verified."
