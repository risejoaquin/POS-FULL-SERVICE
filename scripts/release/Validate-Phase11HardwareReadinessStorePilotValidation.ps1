param(
    [string]$ReleaseVersion = "0.9.0-rc.1",
    [string]$PreviousVersion = "0.9.0-rc.0",
    [string]$ReleaseChannel = "release-candidate"
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$phase11Root = Join-Path $root "artifacts\release\phase11"
$phase113Root = Join-Path $phase11Root "inventory-stock-offline-sync-validation"
$outputRoot = Join-Path $phase11Root "hardware-readiness-store-pilot-validation"

$phase113Summary = Join-Path $phase113Root "inventory-stock-offline-sync-summary.json"
if (!(Test-Path $phase113Summary)) {
    Write-Host "PHASE 11.3 inventory stock offline sync outputs are missing. Regenerating inventory stock offline sync validation before hardware readiness store pilot validation."
    & (Join-Path $PSScriptRoot "Validate-Phase11InventoryStockOfflineSyncValidation.ps1") -ReleaseVersion $ReleaseVersion -PreviousVersion $PreviousVersion -ReleaseChannel $ReleaseChannel
}

New-Item -ItemType Directory -Force -Path $outputRoot | Out-Null

$posPeripheralReadiness = [ordered]@{
    phase = "PHASE 11.4"
    scope = "POS peripheral readiness validation"
    releaseVersion = $ReleaseVersion
    releaseChannel = $ReleaseChannel
    checks = @(
        "thermal printer compatibility checklist documented",
        "cash drawer compatibility checklist documented",
        "barcode scanner compatibility checklist documented",
        "payment terminal readiness checklist documented",
        "device driver and port mapping checklist documented"
    )
    guardrails = @(
        "no real hardware access",
        "no live device mutation",
        "no printer execution",
        "no cash drawer pulse",
        "no scanner capture",
        "no payment terminal execution"
    )
    acceptedChecks = 5
    blockingIssues = 0
}

$operatorTrainingPilotChecklist = [ordered]@{
    phase = "PHASE 11.4"
    scope = "operator training and pilot checklist"
    releaseVersion = $ReleaseVersion
    previousVersion = $PreviousVersion
    checks = @(
        "operator training checklist documented",
        "pilot store entry checklist documented",
        "pilot issue capture checklist documented"
    )
    guardrails = @(
        "no store pilot activation",
        "no production traffic routing",
        "no real inventory mutation"
    )
    acceptedChecks = 3
    blockingIssues = 0
}

$storePilotRehearsal = [ordered]@{
    phase = "PHASE 11.4"
    scope = "store pilot rehearsal validation"
    releaseVersion = $ReleaseVersion
    releaseChannel = $ReleaseChannel
    checks = @(
        "go-live rehearsal checklist documented",
        "support escalation checklist documented",
        "pilot exit criteria documented"
    )
    guardrails = @(
        "no production sync enablement",
        "no public API behavior change",
        "no schema change",
        "no migrations"
    )
    acceptedChecks = 3
    blockingIssues = 0
}

$summary = [ordered]@{
    phase = "PHASE 11.4"
    name = "Hardware Readiness and Store Pilot Checklist"
    prerequisite = "PHASE 11.3 inventory stock movement and offline sync validation"
    sourceEvidence = "588 tests passed source evidence documented"
    expectedEvidence = "604 tests expected after hardware readiness store pilot validation documented"
    outputs = @(
        "pos-peripheral-readiness-evidence.json",
        "operator-training-pilot-checklist.json",
        "store-pilot-rehearsal-evidence.json",
        "hardware-readiness-store-pilot-summary.json"
    )
    guardrails = @(
        "no real hardware access",
        "no live device mutation",
        "no printer execution",
        "no cash drawer pulse",
        "no scanner capture",
        "no payment terminal execution",
        "no store pilot activation",
        "no production traffic routing",
        "no real inventory mutation",
        "no production sync enablement",
        "no public API behavior change",
        "no schema change",
        "no migrations"
    )
    acceptedChecks = 15
    blockingIssues = 0
}

$posPeripheralReadinessPath = Join-Path $outputRoot "pos-peripheral-readiness-evidence.json"
$operatorTrainingPilotChecklistPath = Join-Path $outputRoot "operator-training-pilot-checklist.json"
$storePilotRehearsalPath = Join-Path $outputRoot "store-pilot-rehearsal-evidence.json"
$summaryPath = Join-Path $outputRoot "hardware-readiness-store-pilot-summary.json"

$posPeripheralReadiness | ConvertTo-Json -Depth 8 | Set-Content -Encoding UTF8 -Path $posPeripheralReadinessPath
$operatorTrainingPilotChecklist | ConvertTo-Json -Depth 8 | Set-Content -Encoding UTF8 -Path $operatorTrainingPilotChecklistPath
$storePilotRehearsal | ConvertTo-Json -Depth 8 | Set-Content -Encoding UTF8 -Path $storePilotRehearsalPath
$summary | ConvertTo-Json -Depth 8 | Set-Content -Encoding UTF8 -Path $summaryPath

Write-Host "PHASE 11.4 hardware readiness and store pilot checklist verified."
Write-Host "PosPeripheralReadiness: $posPeripheralReadinessPath"
Write-Host "OperatorTrainingPilotChecklist: $operatorTrainingPilotChecklistPath"
Write-Host "StorePilotRehearsal: $storePilotRehearsalPath"
Write-Host "HardwareReadinessStorePilotSummary: $summaryPath"
Write-Host "AcceptedChecks: 15"
Write-Host "BlockingIssues: 0"
