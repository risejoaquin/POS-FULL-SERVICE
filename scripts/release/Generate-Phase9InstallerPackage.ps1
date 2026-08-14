param(
    [string]$ReleaseVersion = "0.9.0-rc.1",
    [string]$ReleaseChannel = "release-candidate",
    [string]$PackageName = "pos-installer-package"
)

$ErrorActionPreference = "Stop"

$root = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$artifactRoot = Join-Path $root "artifacts\release\phase9"
$publishRoot = Join-Path $artifactRoot "publish"
$installerRoot = Join-Path $artifactRoot "installer"
$stagingRoot = Join-Path $installerRoot "staging"
$packageRoot = Join-Path $stagingRoot "$PackageName-$ReleaseVersion"
$packageManifestPath = Join-Path $installerRoot "installer-package-manifest.json"
$packageChecksumPath = Join-Path $installerRoot "installer-checksums.sha256"
$packageArchivePath = Join-Path $installerRoot "$PackageName-$ReleaseVersion.zip"
$releaseManifestPath = Join-Path $artifactRoot "release-manifest.json"
$releaseChecksumPath = Join-Path $artifactRoot "checksums.sha256"

$requiredPaths = @(
    (Join-Path $publishRoot "poscore-win-x64"),
    (Join-Path $publishRoot "posbuilder-win-x64"),
    (Join-Path $publishRoot "posserver"),
    $releaseManifestPath,
    $releaseChecksumPath
)

$missingRequiredPaths = @()
foreach ($requiredPath in $requiredPaths) {
    if (!(Test-Path $requiredPath)) {
        $missingRequiredPaths += $requiredPath
    }
}

if ($missingRequiredPaths.Count -gt 0) {
    $phase9AGenerator = Join-Path $root "scripts\release\Generate-Phase9ReleaseArtifacts.ps1"
    if (!(Test-Path $phase9AGenerator)) {
        throw "Missing PHASE 9A release artifact inputs and generator script was not found: $phase9AGenerator"
    }

    Write-Host "PHASE 9A release artifact inputs are missing. Regenerating PHASE 9A artifacts before installer package generation."
    & $phase9AGenerator -Configuration Release -RuntimeIdentifier win-x64 -ReleaseVersion $ReleaseVersion -ReleaseChannel $ReleaseChannel

    $stillMissingRequiredPaths = @()
    foreach ($requiredPath in $requiredPaths) {
        if (!(Test-Path $requiredPath)) {
            $stillMissingRequiredPaths += $requiredPath
        }
    }

    if ($stillMissingRequiredPaths.Count -gt 0) {
        throw "Missing PHASE 9A release artifact input after regeneration: $($stillMissingRequiredPaths -join ', ')"
    }
}

if (Test-Path $packageRoot) {
    Remove-Item -Recurse -Force $packageRoot
}

New-Item -ItemType Directory -Force -Path $packageRoot | Out-Null
New-Item -ItemType Directory -Force -Path $installerRoot | Out-Null

Copy-Item -Recurse -Force (Join-Path $publishRoot "poscore-win-x64") (Join-Path $packageRoot "poscore-win-x64")
Copy-Item -Recurse -Force (Join-Path $publishRoot "posbuilder-win-x64") (Join-Path $packageRoot "posbuilder-win-x64")
Copy-Item -Recurse -Force (Join-Path $publishRoot "posserver") (Join-Path $packageRoot "posserver")
Copy-Item -Force $releaseManifestPath (Join-Path $packageRoot "release-manifest.json")
Copy-Item -Force $releaseChecksumPath (Join-Path $packageRoot "checksums.sha256")

$packageChecksumEntries = @()
Get-ChildItem $packageRoot -Recurse -File | Sort-Object FullName | ForEach-Object {
    $hash = Get-FileHash -Algorithm SHA256 -Path $_.FullName
    $relativePath = Resolve-Path -Path $_.FullName -Relative
    $packageChecksumEntries += "$($hash.Hash.ToLowerInvariant())  $relativePath"
}

$packageChecksumEntries | Set-Content -Path $packageChecksumPath -Encoding UTF8
Copy-Item -Force $packageChecksumPath (Join-Path $packageRoot "installer-checksums.sha256")

if (Test-Path $packageArchivePath) {
    Remove-Item -Force $packageArchivePath
}

Compress-Archive -Path (Join-Path $packageRoot "*") -DestinationPath $packageArchivePath -Force

$archiveHash = Get-FileHash -Algorithm SHA256 -Path $packageArchivePath
$packageManifest = [ordered]@{
    phase = "PHASE 9B"
    releaseName = "Installer Package Generation Execution"
    releaseVersion = $ReleaseVersion
    releaseChannel = $ReleaseChannel
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString("o")
    generatedBy = $env:USERNAME
    sourcePublishRoot = $publishRoot
    installerRoot = $installerRoot
    packageRoot = $packageRoot
    packageArchive = $packageArchivePath
    packageArchiveSha256 = $archiveHash.Hash.ToLowerInvariant()
    releaseManifestInput = $releaseManifestPath
    releaseChecksumInput = $releaseChecksumPath
    packageChecksumManifest = $packageChecksumPath
    packageContents = @(
        "poscore-win-x64",
        "posbuilder-win-x64",
        "posserver",
        "release-manifest.json",
        "checksums.sha256",
        "installer-checksums.sha256"
    )
    safetyBoundaries = @(
        "no checkout behavior change",
        "no inventory mutation",
        "no production sync enablement",
        "no deployment execution",
        "no public API behavior change",
        "no schema change",
        "no migrations"
    )
}

$packageManifest | ConvertTo-Json -Depth 8 | Set-Content -Path $packageManifestPath -Encoding UTF8
Copy-Item -Force $packageManifestPath (Join-Path $packageRoot "installer-package-manifest.json")

Write-Host "PHASE 9B installer package generated."
Write-Host "Package: $packageArchivePath"
Write-Host "Manifest: $packageManifestPath"
Write-Host "Checksums: $packageChecksumPath"
