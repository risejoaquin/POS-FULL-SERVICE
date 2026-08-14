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
$smokeRoot = Join-Path $artifactRoot "smoke-install"
$simulatedInstallRoot = Join-Path $smokeRoot "$PackageName-$ReleaseVersion"
$smokeEvidencePath = Join-Path $smokeRoot "smoke-install-evidence.json"
$integrityVerifier = Join-Path $root "scripts\release\Verify-Phase9InstallerPackageIntegrity.ps1"

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
    if (!(Test-Path $integrityVerifier)) {
        throw "Missing PHASE 9C installer package outputs and integrity verifier script was not found: $integrityVerifier"
    }

    Write-Host "PHASE 9C installer package outputs are missing. Regenerating and verifying package before smoke install simulation."
    & $integrityVerifier -ReleaseVersion $ReleaseVersion -ReleaseChannel $ReleaseChannel -PackageName $PackageName
}

foreach ($requiredOutput in $requiredInstallerOutputs) {
    if (!(Test-Path $requiredOutput)) {
        throw "Missing smoke install simulation input after prerequisite verification: $requiredOutput"
    }
}

if (Test-Path $simulatedInstallRoot) {
    Remove-Item -Recurse -Force $simulatedInstallRoot
}

New-Item -ItemType Directory -Force -Path $simulatedInstallRoot | Out-Null
Expand-Archive -Path $packageArchivePath -DestinationPath $simulatedInstallRoot -Force

$requiredSimulatedInstallContents = @(
    "poscore-win-x64",
    "posbuilder-win-x64",
    "posserver",
    "release-manifest.json",
    "checksums.sha256",
    "installer-checksums.sha256"
)

foreach ($requiredContent in $requiredSimulatedInstallContents) {
    $contentPath = Join-Path $simulatedInstallRoot $requiredContent
    if (!(Test-Path $contentPath)) {
        throw "Missing required simulated install content after package extraction: $requiredContent"
    }
}

$posCoreFiles = Get-ChildItem -Path (Join-Path $simulatedInstallRoot "poscore-win-x64") -Recurse -File
$posBuilderFiles = Get-ChildItem -Path (Join-Path $simulatedInstallRoot "posbuilder-win-x64") -Recurse -File
$posServerFiles = Get-ChildItem -Path (Join-Path $simulatedInstallRoot "posserver") -Recurse -File
$allFiles = Get-ChildItem -Path $simulatedInstallRoot -Recurse -File
$executableCandidates = Get-ChildItem -Path $simulatedInstallRoot -Recurse -File | Where-Object { $_.Extension -in @(".exe", ".dll") } | Select-Object -ExpandProperty FullName

if ($posCoreFiles.Count -eq 0) { throw "PosCore simulated install content verification failed: no files found." }
if ($posBuilderFiles.Count -eq 0) { throw "PosBuilder simulated install content verification failed: no files found." }
if ($posServerFiles.Count -eq 0) { throw "PosServer simulated install content verification failed: no files found." }
if ($allFiles.Count -eq 0) { throw "Simulated install file count evidence failed: no files found." }
if ($executableCandidates.Count -eq 0) { throw "Simulated install executable candidate discovery failed: no executable or dll candidates found." }

$evidence = [ordered]@{
    phase = "PHASE 9D"
    releaseVersion = $ReleaseVersion
    releaseChannel = $ReleaseChannel
    packageArchive = $packageArchivePath
    installerManifest = $packageManifestPath
    installerChecksums = $packageChecksumPath
    simulatedInstallRoot = $simulatedInstallRoot
    totalFileCount = $allFiles.Count
    posCoreFileCount = $posCoreFiles.Count
    posBuilderFileCount = $posBuilderFiles.Count
    posServerFileCount = $posServerFiles.Count
    executableCandidateCount = $executableCandidates.Count
    generatedAtUtc = [DateTime]::UtcNow.ToString("o")
    noRealInstallerExecution = $true
    noDeploymentExecution = $true
}

$evidence | ConvertTo-Json -Depth 5 | Set-Content -Path $smokeEvidencePath -Encoding UTF8

Write-Host "PHASE 9D installer smoke install simulation verified."
Write-Host "Package: $packageArchivePath"
Write-Host "SimulatedInstall: $simulatedInstallRoot"
Write-Host "SmokeEvidence: $smokeEvidencePath"
Write-Host "TotalFiles: $($allFiles.Count)"
