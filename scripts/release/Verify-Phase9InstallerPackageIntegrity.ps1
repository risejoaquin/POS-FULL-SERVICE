param(
    [string]$ReleaseVersion = "0.9.0-rc.1",
    [string]$ReleaseChannel = "release-candidate",
    [string]$PackageName = "pos-installer-package"
)

$ErrorActionPreference = "Stop"

$root = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$artifactRoot = Join-Path $root "artifacts\release\phase9"
$installerRoot = Join-Path $artifactRoot "installer"
$packageArchivePath = Join-Path $installerRoot "$PackageName-$ReleaseVersion.zip"
$packageManifestPath = Join-Path $installerRoot "installer-package-manifest.json"
$packageChecksumPath = Join-Path $installerRoot "installer-checksums.sha256"
$verificationRoot = Join-Path $artifactRoot "verification"
$expandedRoot = Join-Path $verificationRoot "$PackageName-$ReleaseVersion"

$requiredInstallerOutputs = @(
    $packageArchivePath,
    $packageManifestPath,
    $packageChecksumPath
)

$missingInstallerOutputs = @()
foreach ($requiredOutput in $requiredInstallerOutputs) {
    if (!(Test-Path $requiredOutput)) {
        $missingInstallerOutputs += $requiredOutput
    }
}

if ($missingInstallerOutputs.Count -gt 0) {
    $packageGenerator = Join-Path $root "scripts\release\Generate-Phase9InstallerPackage.ps1"
    if (!(Test-Path $packageGenerator)) {
        throw "Missing PHASE 9B installer package outputs and package generator script was not found: $packageGenerator"
    }

    Write-Host "PHASE 9B installer package outputs are missing. Regenerating PHASE 9B installer package before integrity verification."
    & $packageGenerator -ReleaseVersion $ReleaseVersion -ReleaseChannel $ReleaseChannel -PackageName $PackageName
}

foreach ($requiredOutput in $requiredInstallerOutputs) {
    if (!(Test-Path $requiredOutput)) {
        throw "Missing installer package verification input after regeneration: $requiredOutput"
    }
}

$manifest = Get-Content -Raw -Path $packageManifestPath | ConvertFrom-Json
if ($manifest.phase -ne "PHASE 9B") {
    throw "Unexpected installer package manifest phase: $($manifest.phase)"
}

if ($manifest.releaseVersion -ne $ReleaseVersion) {
    throw "Unexpected installer package manifest releaseVersion: $($manifest.releaseVersion)"
}

if ($manifest.releaseChannel -ne $ReleaseChannel) {
    throw "Unexpected installer package manifest releaseChannel: $($manifest.releaseChannel)"
}

if ([string]::IsNullOrWhiteSpace($manifest.packageArchiveSha256)) {
    throw "installer-package-manifest.json is missing packageArchiveSha256."
}

$actualArchiveHash = (Get-FileHash -Algorithm SHA256 -Path $packageArchivePath).Hash.ToLowerInvariant()
$expectedArchiveHash = $manifest.packageArchiveSha256.ToString().ToLowerInvariant()
if ($actualArchiveHash -ne $expectedArchiveHash) {
    throw "Installer package archive SHA-256 mismatch. Expected $expectedArchiveHash but found $actualArchiveHash"
}

$checksumLines = Get-Content -Path $packageChecksumPath
if ($checksumLines.Count -eq 0) {
    throw "installer-checksums.sha256 is empty."
}

if ($checksumLines[0] -notmatch "^[a-fA-F0-9]{64}\s+") {
    throw "installer-checksums.sha256 does not contain SHA-256 entries."
}

if (Test-Path $expandedRoot) {
    Remove-Item -Recurse -Force $expandedRoot
}

New-Item -ItemType Directory -Force -Path $expandedRoot | Out-Null
Expand-Archive -Path $packageArchivePath -DestinationPath $expandedRoot -Force

$requiredPackageContents = @(
    "poscore-win-x64",
    "posbuilder-win-x64",
    "posserver",
    "release-manifest.json",
    "checksums.sha256",
    "installer-checksums.sha256"
)

foreach ($requiredContent in $requiredPackageContents) {
    $contentPath = Join-Path $expandedRoot $requiredContent
    if (!(Test-Path $contentPath)) {
        throw "Missing required installer package content after unzip verification: $requiredContent"
    }
}

$posCoreFiles = Get-ChildItem -Path (Join-Path $expandedRoot "poscore-win-x64") -Recurse -File
$posBuilderFiles = Get-ChildItem -Path (Join-Path $expandedRoot "posbuilder-win-x64") -Recurse -File
$posServerFiles = Get-ChildItem -Path (Join-Path $expandedRoot "posserver") -Recurse -File

if ($posCoreFiles.Count -eq 0) { throw "PosCore package content verification failed: no files found." }
if ($posBuilderFiles.Count -eq 0) { throw "PosBuilder package content verification failed: no files found." }
if ($posServerFiles.Count -eq 0) { throw "PosServer package content verification failed: no files found." }

Write-Host "PHASE 9C installer package integrity verified."
Write-Host "Package: $packageArchivePath"
Write-Host "Manifest: $packageManifestPath"
Write-Host "Checksums: $packageChecksumPath"
Write-Host "VerificationExtract: $expandedRoot"
