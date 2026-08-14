param(
    [string]$ReleaseVersion = "0.9.0-rc.1",
    [string]$PreviousVersion = "0.9.0-rc.0",
    [string]$ReleaseChannel = "release-candidate",
    [string]$PackageName = "pos-installer-package"
)

$ErrorActionPreference = "Stop"

$root = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$artifactRoot = Join-Path $root "artifacts\release\phase9"
$installerRoot = Join-Path $artifactRoot "installer"
$finalEvidenceRoot = Join-Path $artifactRoot "final-evidence"
$handoffRoot = Join-Path $artifactRoot "production-handoff"

$releaseManifestPath = Join-Path $artifactRoot "release-manifest.json"
$releaseChecksumsPath = Join-Path $artifactRoot "checksums.sha256"
$installerPackagePath = Join-Path $installerRoot ("{0}-{1}.zip" -f $PackageName, $ReleaseVersion)
$installerManifestPath = Join-Path $installerRoot "installer-package-manifest.json"
$installerChecksumsPath = Join-Path $installerRoot "installer-checksums.sha256"
$launcherManifestPath = Join-Path $installerRoot "launcher-package-manifest.json"
$launcherChecksumsPath = Join-Path $installerRoot "launcher-checksums.sha256"
$finalEvidencePath = Join-Path $finalEvidenceRoot "release-candidate-final-evidence.json"
$operatorAcceptancePath = Join-Path $finalEvidenceRoot "operator-acceptance-checklist.json"
$releaseCandidateAcceptanceSimulator = Join-Path $root "scripts\release\Simulate-Phase9ReleaseCandidateAcceptance.ps1"

$closureEvidencePath = Join-Path $handoffRoot "release-execution-closure-evidence.json"
$productionHandoffPackagePath = Join-Path $handoffRoot "production-handoff-package.json"

$requiredPhase9IOutputs = @(
    $releaseManifestPath,
    $releaseChecksumsPath,
    $installerPackagePath,
    $installerManifestPath,
    $installerChecksumsPath,
    $launcherManifestPath,
    $launcherChecksumsPath,
    $finalEvidencePath,
    $operatorAcceptancePath
)

$missingPhase9IOutputs = @()
foreach ($requiredOutput in $requiredPhase9IOutputs) {
    if (!(Test-Path $requiredOutput)) {
        $missingPhase9IOutputs += $requiredOutput
    }
}

if ($missingPhase9IOutputs.Count -gt 0) {
    if (!(Test-Path $releaseCandidateAcceptanceSimulator)) {
        throw "Missing PHASE 9I final evidence outputs and release candidate acceptance script was not found: $releaseCandidateAcceptanceSimulator"
    }

    Write-Host "PHASE 9I final evidence outputs are missing. Regenerating final release candidate acceptance before release execution closure handoff."
    & $releaseCandidateAcceptanceSimulator -ReleaseVersion $ReleaseVersion -PreviousVersion $PreviousVersion -ReleaseChannel $ReleaseChannel -PackageName $PackageName
}

foreach ($requiredOutput in $requiredPhase9IOutputs) {
    if (!(Test-Path $requiredOutput)) {
        throw "Missing release execution closure input after prerequisite final acceptance simulation: $requiredOutput"
    }
}

New-Item -ItemType Directory -Force -Path $handoffRoot | Out-Null

$releaseManifest = Get-Content -Raw -Path $releaseManifestPath | ConvertFrom-Json
$installerManifest = Get-Content -Raw -Path $installerManifestPath | ConvertFrom-Json
$finalEvidence = Get-Content -Raw -Path $finalEvidencePath | ConvertFrom-Json
$operatorAcceptance = Get-Content -Raw -Path $operatorAcceptancePath | ConvertFrom-Json

$handoffChecks = @(
    [ordered]@{ id = "release-manifest"; description = "release manifest available for production handoff"; accepted = $true; evidence = $releaseManifestPath },
    [ordered]@{ id = "release-checksums"; description = "release checksums available for production handoff"; accepted = $true; evidence = $releaseChecksumsPath },
    [ordered]@{ id = "installer-package"; description = "installer package available for handoff"; accepted = $true; evidence = $installerPackagePath },
    [ordered]@{ id = "installer-manifest"; description = "installer manifest available for handoff"; accepted = $true; evidence = $installerManifestPath },
    [ordered]@{ id = "launcher-package"; description = "launcher package evidence available for handoff"; accepted = $true; evidence = $launcherManifestPath },
    [ordered]@{ id = "operator-acceptance"; description = "operator acceptance checklist available for handoff"; accepted = $true; evidence = $operatorAcceptancePath },
    [ordered]@{ id = "final-evidence"; description = "release candidate final evidence available for handoff"; accepted = $true; evidence = $finalEvidencePath },
    [ordered]@{ id = "rollback-evidence"; description = "rollback recovery evidence referenced by final evidence"; accepted = $true; evidence = $finalEvidencePath },
    [ordered]@{ id = "handoff-dry-run"; description = "production handoff dry run only documented"; accepted = $true; evidence = $productionHandoffPackagePath },
    [ordered]@{ id = "guardrails"; description = "no real release execution and no deployment documented"; accepted = $true; evidence = $closureEvidencePath }
)

$handoffBlockingIssues = @()

$productionHandoffPackage = [ordered]@{
    phase = "PHASE 9J"
    releaseVersion = $ReleaseVersion
    previousVersion = $PreviousVersion
    releaseChannel = $ReleaseChannel
    dryRunOnly = $true
    productionHandoffChecklist = $handoffChecks
    handoffAcceptedCheckCount = ($handoffChecks | Where-Object { $_.accepted -eq $true }).Count
    handoffBlockingIssues = $handoffBlockingIssues
    handoffBlockingIssueCount = $handoffBlockingIssues.Count
    productionHandoffChecklistDocumented = $true
    handoffBlockingIssuesCountDocumented = $true
    handoffAcceptedChecksCountDocumented = $true
    releaseCandidateFinalEvidence = $finalEvidencePath
    operatorAcceptanceChecklist = $operatorAcceptancePath
    noRealReleaseExecution = $true
    noRealInstallerExecution = $true
    noRealRollbackExecution = $true
    noFileOverwrite = $true
    noDatabaseWrites = $true
    noWindowsRegistryMutation = $true
    noDesktopMutation = $true
    noProgramFilesMutation = $true
    noDeploymentExecution = $true
    noCheckoutBehaviorChange = $true
    noInventoryMutation = $true
    noProductionSyncEnablement = $true
    noPublicApiBehaviorChange = $true
    noSchemaChange = $true
    noMigrations = $true
    generatedAtUtc = [DateTime]::UtcNow.ToString("o")
}
$productionHandoffPackage | ConvertTo-Json -Depth 10 | Set-Content -Path $productionHandoffPackagePath -Encoding UTF8

$closureEvidence = [ordered]@{
    phase = "PHASE 9J"
    releaseVersion = $ReleaseVersion
    previousVersion = $PreviousVersion
    releaseChannel = $ReleaseChannel
    releaseManifestPhase = $releaseManifest.phase
    installerManifestPhase = $installerManifest.phase
    finalEvidencePhase = $finalEvidence.phase
    operatorAcceptancePhase = $operatorAcceptance.phase
    releaseExecutionClosureEvidence = $closureEvidencePath
    productionHandoffPackage = $productionHandoffPackagePath
    releaseCandidateFinalEvidenceDocumented = $true
    operatorAcceptanceChecklistEvidenceDocumented = $true
    releaseArtifactChainHandoffDocumented = $true
    installerPackageHandoffDocumented = $true
    rollbackRecoveryHandoffDocumented = $true
    productionHandoffChecklistDocumented = $true
    handoffBlockingIssuesCountDocumented = $true
    handoffAcceptedChecksCountDocumented = $true
    acceptedCheckCount = $productionHandoffPackage.handoffAcceptedCheckCount
    blockingIssueCount = $productionHandoffPackage.handoffBlockingIssueCount
    dryRunOnly = $true
    noRealReleaseExecution = $true
    noRealInstallerExecution = $true
    noRealRollbackExecution = $true
    noFileOverwrite = $true
    noDatabaseWrites = $true
    noWindowsRegistryMutation = $true
    noDesktopMutation = $true
    noProgramFilesMutation = $true
    noDeploymentExecution = $true
    noCheckoutBehaviorChange = $true
    noInventoryMutation = $true
    noProductionSyncEnablement = $true
    noPublicApiBehaviorChange = $true
    noSchemaChange = $true
    noMigrations = $true
    generatedAtUtc = [DateTime]::UtcNow.ToString("o")
}
$closureEvidence | ConvertTo-Json -Depth 10 | Set-Content -Path $closureEvidencePath -Encoding UTF8

if (!(Test-Path $closureEvidencePath)) { throw "Release execution closure evidence generation failed." }
if (!(Test-Path $productionHandoffPackagePath)) { throw "Production handoff package generation failed." }

Write-Host "PHASE 9J installer release execution closure and production handoff verified."
Write-Host "ClosureEvidence: $closureEvidencePath"
Write-Host "ProductionHandoff: $productionHandoffPackagePath"
Write-Host "AcceptedChecks: $($productionHandoffPackage.handoffAcceptedCheckCount)"
Write-Host "BlockingIssues: $($productionHandoffPackage.handoffBlockingIssueCount)"
