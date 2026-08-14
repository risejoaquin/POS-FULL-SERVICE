param(
    [string]$ReleaseVersion = "0.9.0-rc.1",
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
$simulatedInstallRoot = Join-Path $smokeRoot "$PackageName-$ReleaseVersion"
$smokeEvidencePath = Join-Path $smokeRoot "smoke-install-evidence.json"
$launchPackageArchivePath = Join-Path $installerRoot "$PackageName-launch-$ReleaseVersion.zip"
$launcherManifestPath = Join-Path $installerRoot "launcher-package-manifest.json"
$launcherChecksumPath = Join-Path $installerRoot "launcher-checksums.sha256"
$launcherStageRoot = Join-Path $launcherRoot "$PackageName-launch-$ReleaseVersion"
$desktopShortcutSpecPath = Join-Path $launcherStageRoot "launch\desktop-shortcut-spec.json"
$launcherGenerator = Join-Path $root "scripts\release\Generate-Phase9LaunchAndShortcutPackage.ps1"
$cleanupPlanPath = Join-Path $uninstallRoot "uninstall-cleanup-plan.json"
$cleanupEvidencePath = Join-Path $uninstallRoot "uninstall-cleanup-evidence.json"

$requiredLauncherOutputs = @(
    $launchPackageArchivePath,
    $launcherManifestPath,
    $launcherChecksumPath,
    $launcherStageRoot,
    $desktopShortcutSpecPath,
    $smokeEvidencePath,
    $simulatedInstallRoot
)

$missingLauncherOutputs = @()
foreach ($requiredOutput in $requiredLauncherOutputs) {
    if (!(Test-Path $requiredOutput)) {
        $missingLauncherOutputs += $requiredOutput
    }
}

if ($missingLauncherOutputs.Count -gt 0) {
    if (!(Test-Path $launcherGenerator)) {
        throw "Missing PHASE 9E launcher outputs and launcher packaging script was not found: $launcherGenerator"
    }

    Write-Host "PHASE 9E launcher package outputs are missing. Regenerating launcher package before uninstall cleanup simulation."
    & $launcherGenerator -ReleaseVersion $ReleaseVersion -ReleaseChannel $ReleaseChannel -PackageName $PackageName
}

foreach ($requiredOutput in $requiredLauncherOutputs) {
    if (!(Test-Path $requiredOutput)) {
        throw "Missing uninstall cleanup simulation input after prerequisite launcher packaging: $requiredOutput"
    }
}

New-Item -ItemType Directory -Force -Path $uninstallRoot | Out-Null

$shortcutSpec = Get-Content -Raw -Path $desktopShortcutSpecPath | ConvertFrom-Json
$smokeEvidence = Get-Content -Raw -Path $smokeEvidencePath | ConvertFrom-Json
$launcherManifest = Get-Content -Raw -Path $launcherManifestPath | ConvertFrom-Json

$simulatedInstallFiles = @(Get-ChildItem -Path $simulatedInstallRoot -Recurse -File | ForEach-Object { $_.FullName })
$launcherFiles = @(Get-ChildItem -Path $launcherStageRoot -Recurse -File | ForEach-Object { $_.FullName })
$shortcutCandidates = @($shortcutSpec.shortcuts | ForEach-Object { [ordered]@{ name = $_.name; targetScript = $_.targetScript; workingDirectory = $_.workingDirectory; simulatedOnly = $true } })

$temporaryVerificationDirectories = @(
    (Join-Path $artifactRoot "verification"),
    $simulatedInstallRoot,
    $launcherStageRoot
)

$generatedInstallerArtifacts = @(
    (Join-Path $installerRoot "$PackageName-$ReleaseVersion.zip"),
    $launchPackageArchivePath
)

$preservedReleaseManifests = @(
    (Join-Path $artifactRoot "release-manifest.json"),
    (Join-Path $installerRoot "installer-package-manifest.json"),
    $launcherManifestPath
)

$preservedChecksums = @(
    (Join-Path $artifactRoot "checksums.sha256"),
    (Join-Path $installerRoot "installer-checksums.sha256"),
    $launcherChecksumPath
)

$preservedAuditEvidence = @(
    $smokeEvidencePath,
    $desktopShortcutSpecPath,
    $cleanupPlanPath,
    $cleanupEvidencePath
)

$cleanupPlan = [ordered]@{
    phase = "PHASE 9F"
    releaseVersion = $ReleaseVersion
    releaseChannel = $ReleaseChannel
    dryRunOnly = $true
    noRealFileDeletion = $true
    noRealShortcutDeletion = $true
    noProgramFilesMutation = $true
    noDesktopMutation = $true
    noWindowsRegistryMutation = $true
    noRealInstallerExecution = $true
    noDeploymentExecution = $true
    simulatedInstallDirectory = $simulatedInstallRoot
    launcherPackageDirectory = $launcherStageRoot
    desktopShortcutCandidates = $shortcutCandidates
    temporaryVerificationDirectories = $temporaryVerificationDirectories
    generatedInstallerArtifacts = $generatedInstallerArtifacts
    preservedReleaseManifests = $preservedReleaseManifests
    preservedChecksums = $preservedChecksums
    preservedAuditEvidence = $preservedAuditEvidence
    simulatedInstallFileCount = $simulatedInstallFiles.Count
    launcherFileCount = $launcherFiles.Count
    generatedAtUtc = [DateTime]::UtcNow.ToString("o")
}
$cleanupPlan | ConvertTo-Json -Depth 10 | Set-Content -Path $cleanupPlanPath -Encoding UTF8

$cleanupCandidates = @()
$cleanupCandidates += [ordered]@{ category = "simulatedInstallDirectory"; path = $simulatedInstallRoot; candidateOnly = $true }
$cleanupCandidates += [ordered]@{ category = "launcherPackageDirectory"; path = $launcherStageRoot; candidateOnly = $true }
foreach ($directory in $temporaryVerificationDirectories) {
    $cleanupCandidates += [ordered]@{ category = "temporaryVerificationDirectories"; path = $directory; candidateOnly = $true }
}
foreach ($shortcut in $shortcutCandidates) {
    $cleanupCandidates += [ordered]@{ category = "desktopShortcutCandidates"; name = $shortcut.name; targetScript = $shortcut.targetScript; candidateOnly = $true }
}

$preservedItems = @()
$preservedItems += $generatedInstallerArtifacts
$preservedItems += $preservedReleaseManifests
$preservedItems += $preservedChecksums
$preservedItems += $preservedAuditEvidence
$preservedItems = @($preservedItems | Select-Object -Unique)

$cleanupEvidence = [ordered]@{
    phase = "PHASE 9F"
    releaseVersion = $ReleaseVersion
    releaseChannel = $ReleaseChannel
    launcherManifestPhase = $launcherManifest.phase
    smokeEvidencePhase = $smokeEvidence.phase
    cleanupPlan = $cleanupPlanPath
    cleanupCandidates = $cleanupCandidates
    preservedItems = $preservedItems
    cleanupCandidateCount = $cleanupCandidates.Count
    preservedItemCount = $preservedItems.Count
    dryRunOnly = $true
    noRealFileDeletion = $true
    noRealShortcutDeletion = $true
    noProgramFilesMutation = $true
    noDesktopMutation = $true
    noWindowsRegistryMutation = $true
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
$cleanupEvidence | ConvertTo-Json -Depth 10 | Set-Content -Path $cleanupEvidencePath -Encoding UTF8

if (!(Test-Path $cleanupPlanPath)) { throw "Uninstall cleanup plan generation failed." }
if (!(Test-Path $cleanupEvidencePath)) { throw "Uninstall cleanup evidence generation failed." }

Write-Host "PHASE 9F installer uninstall and cleanup simulation verified."
Write-Host "CleanupPlan: $cleanupPlanPath"
Write-Host "CleanupEvidence: $cleanupEvidencePath"
Write-Host "PreservedItems: $($preservedItems.Count)"
Write-Host "CleanupCandidates: $($cleanupCandidates.Count)"
