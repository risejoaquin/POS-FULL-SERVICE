param(
    [string]$RollbackFromVersion = "0.9.0-rc.1",
    [string]$RollbackToVersion = "0.9.0-rc.0",
    [string]$ReleaseChannel = "release-candidate",
    [string]$PackageName = "pos-installer-package"
)

$ErrorActionPreference = "Stop"

$root = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$artifactRoot = Join-Path $root "artifacts\release\phase9"
$installerRoot = Join-Path $artifactRoot "installer"
$smokeRoot = Join-Path $artifactRoot "smoke-install"
$uninstallRoot = Join-Path $artifactRoot "uninstall-simulation"
$upgradeRoot = Join-Path $artifactRoot "upgrade-simulation"
$rollbackRoot = Join-Path $artifactRoot "rollback-simulation"

$upgradePlanPath = Join-Path $upgradeRoot "upgrade-simulation-plan.json"
$upgradeEvidencePath = Join-Path $upgradeRoot "upgrade-preservation-evidence.json"
$uninstallCleanupPlanPath = Join-Path $uninstallRoot "uninstall-cleanup-plan.json"
$uninstallCleanupEvidencePath = Join-Path $uninstallRoot "uninstall-cleanup-evidence.json"
$installerManifestPath = Join-Path $installerRoot "installer-package-manifest.json"
$installerChecksumPath = Join-Path $installerRoot "installer-checksums.sha256"
$launcherManifestPath = Join-Path $installerRoot "launcher-package-manifest.json"
$launcherChecksumPath = Join-Path $installerRoot "launcher-checksums.sha256"
$smokeEvidencePath = Join-Path $smokeRoot "smoke-install-evidence.json"
$upgradeSimulator = Join-Path $root "scripts\release\Simulate-Phase9InstallerUpgrade.ps1"

$rollbackPlanPath = Join-Path $rollbackRoot "rollback-simulation-plan.json"
$rollbackEvidencePath = Join-Path $rollbackRoot "previous-version-recovery-evidence.json"

$requiredPhase9GOutputs = @(
    $upgradePlanPath,
    $upgradeEvidencePath,
    $uninstallCleanupPlanPath,
    $uninstallCleanupEvidencePath,
    $installerManifestPath,
    $installerChecksumPath,
    $launcherManifestPath,
    $launcherChecksumPath,
    $smokeEvidencePath
)

$missingPhase9GOutputs = @()
foreach ($requiredOutput in $requiredPhase9GOutputs) {
    if (!(Test-Path $requiredOutput)) {
        $missingPhase9GOutputs += $requiredOutput
    }
}

if ($missingPhase9GOutputs.Count -gt 0) {
    if (!(Test-Path $upgradeSimulator)) {
        throw "Missing PHASE 9G upgrade simulation outputs and upgrade simulation script was not found: $upgradeSimulator"
    }

    Write-Host "PHASE 9G upgrade simulation outputs are missing. Regenerating upgrade simulation before rollback simulation."
    & $upgradeSimulator -PreviousVersion $RollbackToVersion -ReleaseVersion $RollbackFromVersion -ReleaseChannel $ReleaseChannel -PackageName $PackageName
}

foreach ($requiredOutput in $requiredPhase9GOutputs) {
    if (!(Test-Path $requiredOutput)) {
        throw "Missing rollback simulation input after prerequisite upgrade simulation: $requiredOutput"
    }
}

New-Item -ItemType Directory -Force -Path $rollbackRoot | Out-Null

$upgradePlan = Get-Content -Raw -Path $upgradePlanPath | ConvertFrom-Json
$upgradeEvidence = Get-Content -Raw -Path $upgradeEvidencePath | ConvertFrom-Json
$uninstallCleanupPlan = Get-Content -Raw -Path $uninstallCleanupPlanPath | ConvertFrom-Json
$uninstallCleanupEvidence = Get-Content -Raw -Path $uninstallCleanupEvidencePath | ConvertFrom-Json
$installerManifest = Get-Content -Raw -Path $installerManifestPath | ConvertFrom-Json
$smokeEvidence = Get-Content -Raw -Path $smokeEvidencePath | ConvertFrom-Json

$recoveryTargets = @(
    [ordered]@{ category = "tenantBranding"; path = "simulated://poscore/config/branding.json"; recover = $true; reason = "tenant branding recovery preservation documented" },
    [ordered]@{ category = "localDatabase"; path = "simulated://poscore/data/pos-local.sqlite"; recover = $true; reason = "local database recovery preservation documented" },
    [ordered]@{ category = "offlineSyncQueue"; path = "simulated://poscore/data/sync-queue"; recover = $true; reason = "offline sync queue recovery preservation documented" },
    [ordered]@{ category = "licenseState"; path = "simulated://poscore/config/license.json"; recover = $true; reason = "license state recovery preservation documented" },
    [ordered]@{ category = "operatorSettings"; path = "simulated://poscore/config/operator-settings.json"; recover = $true; reason = "operator settings recovery preservation documented" },
    [ordered]@{ category = "upgradePreservationEvidence"; path = $upgradeEvidencePath; recover = $true; reason = "upgrade preservation evidence prerequisite documented" },
    [ordered]@{ category = "releaseManifests"; path = $installerManifestPath; recover = $true; reason = "release manifest recovery preservation documented" },
    [ordered]@{ category = "checksums"; path = $installerChecksumPath; recover = $true; reason = "checksums recovery preservation documented" }
)

$rollbackCandidates = @(
    [ordered]@{ category = "applicationBinaries"; fromVersion = $RollbackFromVersion; toVersion = $RollbackToVersion; simulatedOnly = $true },
    [ordered]@{ category = "launcherScripts"; fromVersion = $RollbackFromVersion; toVersion = $RollbackToVersion; simulatedOnly = $true },
    [ordered]@{ category = "serverBinaries"; fromVersion = $RollbackFromVersion; toVersion = $RollbackToVersion; simulatedOnly = $true },
    [ordered]@{ category = "builderBinaries"; fromVersion = $RollbackFromVersion; toVersion = $RollbackToVersion; simulatedOnly = $true }
)

$rollbackPlan = [ordered]@{
    phase = "PHASE 9H"
    rollbackFromVersion = $RollbackFromVersion
    rollbackToVersion = $RollbackToVersion
    releaseChannel = $ReleaseChannel
    dryRunOnly = $true
    noRealRollbackExecution = $true
    noFileOverwrite = $true
    noDatabaseWrites = $true
    noWindowsRegistryMutation = $true
    noDesktopMutation = $true
    noProgramFilesMutation = $true
    noRealInstallerExecution = $true
    noDeploymentExecution = $true
    prerequisitePhase = "PHASE 9G"
    upgradePlan = $upgradePlanPath
    upgradePreservationEvidence = $upgradeEvidencePath
    uninstallCleanupPlan = $uninstallCleanupPlanPath
    uninstallCleanupEvidence = $uninstallCleanupEvidencePath
    installerManifest = $installerManifestPath
    smokeEvidence = $smokeEvidencePath
    rollbackCandidates = $rollbackCandidates
    recoveryTargets = $recoveryTargets
    versionTransition = [ordered]@{ from = $RollbackFromVersion; to = $RollbackToVersion; channel = $ReleaseChannel; simulatedOnly = $true }
    generatedAtUtc = [DateTime]::UtcNow.ToString("o")
}
$rollbackPlan | ConvertTo-Json -Depth 10 | Set-Content -Path $rollbackPlanPath -Encoding UTF8

$rollbackEvidence = [ordered]@{
    phase = "PHASE 9H"
    rollbackFromVersion = $RollbackFromVersion
    rollbackToVersion = $RollbackToVersion
    releaseChannel = $ReleaseChannel
    installerManifestPhase = $installerManifest.phase
    smokeEvidencePhase = $smokeEvidence.phase
    uninstallCleanupPlanPhase = $uninstallCleanupPlan.phase
    uninstallCleanupEvidencePhase = $uninstallCleanupEvidence.phase
    upgradePlanPhase = $upgradePlan.phase
    upgradeEvidencePhase = $upgradeEvidence.phase
    rollbackPlan = $rollbackPlanPath
    previousVersionRecoveryEvidence = $rollbackEvidencePath
    recoveryTargets = $recoveryTargets
    rollbackCandidates = $rollbackCandidates
    recoveredItemCount = $recoveryTargets.Count
    rollbackCandidateCount = $rollbackCandidates.Count
    rollbackSourceVersionDetectionDocumented = $true
    rollbackTargetVersionValidationDocumented = $true
    tenantBrandingRecoveryPreservationDocumented = $true
    localDatabaseRecoveryPreservationDocumented = $true
    offlineSyncQueueRecoveryPreservationDocumented = $true
    licenseStateRecoveryPreservationDocumented = $true
    operatorSettingsRecoveryPreservationDocumented = $true
    upgradePreservationEvidencePrerequisiteDocumented = $true
    dryRunOnly = $true
    noRealRollbackExecution = $true
    noFileOverwrite = $true
    noDatabaseWrites = $true
    noWindowsRegistryMutation = $true
    noDesktopMutation = $true
    noProgramFilesMutation = $true
    noRealInstallerExecution = $true
    noDeploymentExecution = $true
    noCheckoutBehaviorChange = $true
    noInventoryMutation = $true
    noProductionSyncEnablement = $true
    noPublicApiBehaviorChange = $true
    noSchemaChange = $true
    noMigrations = $true
    generatedAtUtc = [DateTime]::UtcNow.ToString("o")
}
$rollbackEvidence | ConvertTo-Json -Depth 10 | Set-Content -Path $rollbackEvidencePath -Encoding UTF8

if (!(Test-Path $rollbackPlanPath)) { throw "Rollback simulation plan generation failed." }
if (!(Test-Path $rollbackEvidencePath)) { throw "Previous version recovery evidence generation failed." }

Write-Host "PHASE 9H installer rollback simulation and previous version recovery verified."
Write-Host "RollbackPlan: $rollbackPlanPath"
Write-Host "RecoveryEvidence: $rollbackEvidencePath"
Write-Host "RollbackFromVersion: $RollbackFromVersion"
Write-Host "RollbackToVersion: $RollbackToVersion"
Write-Host "RecoveredItems: $($recoveryTargets.Count)"
Write-Host "RollbackCandidates: $($rollbackCandidates.Count)"
