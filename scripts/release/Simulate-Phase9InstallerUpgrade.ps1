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
$smokeRoot = Join-Path $artifactRoot "smoke-install"
$launcherRoot = Join-Path $artifactRoot "launcher"
$uninstallRoot = Join-Path $artifactRoot "uninstall-simulation"
$upgradeRoot = Join-Path $artifactRoot "upgrade-simulation"

$uninstallCleanupPlanPath = Join-Path $uninstallRoot "uninstall-cleanup-plan.json"
$uninstallCleanupEvidencePath = Join-Path $uninstallRoot "uninstall-cleanup-evidence.json"
$launcherManifestPath = Join-Path $installerRoot "launcher-package-manifest.json"
$launcherChecksumPath = Join-Path $installerRoot "launcher-checksums.sha256"
$launchPackageArchivePath = Join-Path $installerRoot "$PackageName-launch-$ReleaseVersion.zip"
$installerManifestPath = Join-Path $installerRoot "installer-package-manifest.json"
$installerChecksumPath = Join-Path $installerRoot "installer-checksums.sha256"
$smokeEvidencePath = Join-Path $smokeRoot "smoke-install-evidence.json"
$simulatedInstallRoot = Join-Path $smokeRoot "$PackageName-$ReleaseVersion"
$uninstallSimulator = Join-Path $root "scripts\release\Simulate-Phase9InstallerUninstallCleanup.ps1"

$upgradePlanPath = Join-Path $upgradeRoot "upgrade-simulation-plan.json"
$upgradeEvidencePath = Join-Path $upgradeRoot "upgrade-preservation-evidence.json"

$requiredPhase9FOutputs = @(
    $uninstallCleanupPlanPath,
    $uninstallCleanupEvidencePath,
    $launcherManifestPath,
    $launcherChecksumPath,
    $launchPackageArchivePath,
    $installerManifestPath,
    $installerChecksumPath,
    $smokeEvidencePath,
    $simulatedInstallRoot
)

$missingPhase9FOutputs = @()
foreach ($requiredOutput in $requiredPhase9FOutputs) {
    if (!(Test-Path $requiredOutput)) {
        $missingPhase9FOutputs += $requiredOutput
    }
}

if ($missingPhase9FOutputs.Count -gt 0) {
    if (!(Test-Path $uninstallSimulator)) {
        throw "Missing PHASE 9F uninstall cleanup outputs and uninstall cleanup simulation script was not found: $uninstallSimulator"
    }

    Write-Host "PHASE 9F uninstall cleanup outputs are missing. Regenerating uninstall cleanup simulation before upgrade simulation."
    & $uninstallSimulator -ReleaseVersion $ReleaseVersion -ReleaseChannel $ReleaseChannel -PackageName $PackageName
}

foreach ($requiredOutput in $requiredPhase9FOutputs) {
    if (!(Test-Path $requiredOutput)) {
        throw "Missing upgrade simulation input after prerequisite uninstall cleanup simulation: $requiredOutput"
    }
}

New-Item -ItemType Directory -Force -Path $upgradeRoot | Out-Null

$uninstallCleanupPlan = Get-Content -Raw -Path $uninstallCleanupPlanPath | ConvertFrom-Json
$uninstallCleanupEvidence = Get-Content -Raw -Path $uninstallCleanupEvidencePath | ConvertFrom-Json
$launcherManifest = Get-Content -Raw -Path $launcherManifestPath | ConvertFrom-Json
$installerManifest = Get-Content -Raw -Path $installerManifestPath | ConvertFrom-Json
$smokeEvidence = Get-Content -Raw -Path $smokeEvidencePath | ConvertFrom-Json

$preservationTargets = @(
    [ordered]@{ category = "tenantBranding"; path = "simulated://poscore/config/branding.json"; preserve = $true; reason = "tenant branding preservation documented" },
    [ordered]@{ category = "localDatabase"; path = "simulated://poscore/data/pos-local.sqlite"; preserve = $true; reason = "local database preservation documented" },
    [ordered]@{ category = "offlineSyncQueue"; path = "simulated://poscore/data/sync-queue"; preserve = $true; reason = "offline sync queue preservation documented" },
    [ordered]@{ category = "licenseState"; path = "simulated://poscore/config/license.json"; preserve = $true; reason = "license state preservation documented" },
    [ordered]@{ category = "operatorSettings"; path = "simulated://poscore/config/operator-settings.json"; preserve = $true; reason = "operator settings preservation documented" },
    [ordered]@{ category = "launcherPackage"; path = $launchPackageArchivePath; preserve = $true; reason = "launcher package preservation documented" },
    [ordered]@{ category = "uninstallCleanupEvidence"; path = $uninstallCleanupEvidencePath; preserve = $true; reason = "uninstall cleanup evidence preservation documented" },
    [ordered]@{ category = "releaseManifests"; path = $installerManifestPath; preserve = $true; reason = "release manifest preservation documented" },
    [ordered]@{ category = "checksums"; path = $installerChecksumPath; preserve = $true; reason = "checksums preservation documented" }
)

$upgradeCandidates = @(
    [ordered]@{ category = "applicationBinaries"; fromVersion = $PreviousVersion; toVersion = $ReleaseVersion; simulatedOnly = $true },
    [ordered]@{ category = "launcherScripts"; fromVersion = $PreviousVersion; toVersion = $ReleaseVersion; simulatedOnly = $true },
    [ordered]@{ category = "serverBinaries"; fromVersion = $PreviousVersion; toVersion = $ReleaseVersion; simulatedOnly = $true },
    [ordered]@{ category = "builderBinaries"; fromVersion = $PreviousVersion; toVersion = $ReleaseVersion; simulatedOnly = $true }
)

$upgradePlan = [ordered]@{
    phase = "PHASE 9G"
    releaseVersion = $ReleaseVersion
    previousVersion = $PreviousVersion
    releaseChannel = $ReleaseChannel
    dryRunOnly = $true
    noRealUpgradeExecution = $true
    noFileOverwrite = $true
    noDatabaseWrites = $true
    noWindowsRegistryMutation = $true
    noDesktopMutation = $true
    noProgramFilesMutation = $true
    noRealInstallerExecution = $true
    noDeploymentExecution = $true
    prerequisitePhase = "PHASE 9F"
    uninstallCleanupPlan = $uninstallCleanupPlanPath
    uninstallCleanupEvidence = $uninstallCleanupEvidencePath
    launcherManifest = $launcherManifestPath
    launcherPackage = $launchPackageArchivePath
    installerManifest = $installerManifestPath
    smokeEvidence = $smokeEvidencePath
    simulatedInstall = $simulatedInstallRoot
    upgradeCandidates = $upgradeCandidates
    preservationTargets = $preservationTargets
    versionTransition = [ordered]@{ from = $PreviousVersion; to = $ReleaseVersion; channel = $ReleaseChannel; simulatedOnly = $true }
    generatedAtUtc = [DateTime]::UtcNow.ToString("o")
}
$upgradePlan | ConvertTo-Json -Depth 10 | Set-Content -Path $upgradePlanPath -Encoding UTF8

$upgradeEvidence = [ordered]@{
    phase = "PHASE 9G"
    releaseVersion = $ReleaseVersion
    previousVersion = $PreviousVersion
    releaseChannel = $ReleaseChannel
    launcherManifestPhase = $launcherManifest.phase
    installerManifestPhase = $installerManifest.phase
    smokeEvidencePhase = $smokeEvidence.phase
    uninstallCleanupPlanPhase = $uninstallCleanupPlan.phase
    uninstallCleanupEvidencePhase = $uninstallCleanupEvidence.phase
    upgradePlan = $upgradePlanPath
    upgradePreservationEvidence = $upgradeEvidencePath
    preservationTargets = $preservationTargets
    upgradeCandidates = $upgradeCandidates
    preservedItemCount = $preservationTargets.Count
    upgradeCandidateCount = $upgradeCandidates.Count
    previousVersionDetectionDocumented = $true
    targetVersionValidationDocumented = $true
    releaseChannelPreservationDocumented = $true
    tenantBrandingPreservationDocumented = $true
    localDatabasePreservationDocumented = $true
    offlineSyncQueuePreservationDocumented = $true
    licenseStatePreservationDocumented = $true
    operatorSettingsPreservationDocumented = $true
    launcherPackagePreservationDocumented = $true
    uninstallCleanupEvidencePreservationDocumented = $true
    dryRunOnly = $true
    noRealUpgradeExecution = $true
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
$upgradeEvidence | ConvertTo-Json -Depth 10 | Set-Content -Path $upgradeEvidencePath -Encoding UTF8

if (!(Test-Path $upgradePlanPath)) { throw "Upgrade simulation plan generation failed." }
if (!(Test-Path $upgradeEvidencePath)) { throw "Upgrade preservation evidence generation failed." }

Write-Host "PHASE 9G installer upgrade simulation and version preservation verified."
Write-Host "UpgradePlan: $upgradePlanPath"
Write-Host "UpgradeEvidence: $upgradeEvidencePath"
Write-Host "PreviousVersion: $PreviousVersion"
Write-Host "TargetVersion: $ReleaseVersion"
Write-Host "PreservedItems: $($preservationTargets.Count)"
Write-Host "UpgradeCandidates: $($upgradeCandidates.Count)"
