param(
    [string]$Configuration = "Release",
    [string]$RuntimeIdentifier = "win-x64",
    [string]$ReleaseVersion = "0.9.0-rc.1",
    [string]$ReleaseChannel = "release-candidate"
)

$ErrorActionPreference = "Stop"

$root = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$artifactRoot = Join-Path $root "artifacts\release\phase9"
$publishRoot = Join-Path $artifactRoot "publish"
$manifestPath = Join-Path $artifactRoot "release-manifest.json"
$checksumPath = Join-Path $artifactRoot "checksums.sha256"

New-Item -ItemType Directory -Force -Path $publishRoot | Out-Null

$projects = @(
    @{ Name = "poscore"; Path = "PosCore\PosCore.csproj"; Output = "poscore-win-x64"; Windows = $true },
    @{ Name = "posbuilder"; Path = "PosBuilder\PosBuilder.csproj"; Output = "posbuilder-win-x64"; Windows = $true },
    @{ Name = "posserver"; Path = "PosServer\PosServer.csproj"; Output = "posserver"; Windows = $false }
)

$publishedArtifacts = @()

foreach ($project in $projects) {
    $projectPath = Join-Path $root $project.Path
    $outputPath = Join-Path $publishRoot $project.Output

    if (!(Test-Path $projectPath)) {
        throw "Missing publish project: $projectPath"
    }

    New-Item -ItemType Directory -Force -Path $outputPath | Out-Null

    if ($project.Windows) {
        dotnet publish $projectPath -c $Configuration -r $RuntimeIdentifier --self-contained false -o $outputPath
    }
    else {
        dotnet publish $projectPath -c $Configuration --self-contained false -o $outputPath
    }

    $publishedArtifacts += [ordered]@{
        name = $project.Name
        project = $project.Path
        outputPath = $outputPath
        runtimeIdentifier = $(if ($project.Windows) { $RuntimeIdentifier } else { "portable" })
    }
}

$checksumEntries = @()
Get-ChildItem $publishRoot -Recurse -File | Sort-Object FullName | ForEach-Object {
    $hash = Get-FileHash -Algorithm SHA256 -Path $_.FullName
    $relativePath = Resolve-Path -Path $_.FullName -Relative
    $checksumEntries += "$($hash.Hash.ToLowerInvariant())  $relativePath"
}

$checksumEntries | Set-Content -Path $checksumPath -Encoding UTF8

$manifest = [ordered]@{
    phase = "PHASE 9A"
    releaseName = "Installer Generation and Release Artifact Execution"
    releaseVersion = $ReleaseVersion
    releaseChannel = $ReleaseChannel
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString("o")
    generatedBy = $env:USERNAME
    configuration = $Configuration
    runtimeIdentifier = $RuntimeIdentifier
    artifactRoot = $artifactRoot
    publishRoot = $publishRoot
    checksumManifest = $checksumPath
    artifacts = $publishedArtifacts
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

$manifest | ConvertTo-Json -Depth 8 | Set-Content -Path $manifestPath -Encoding UTF8

Write-Host "PHASE 9A release artifacts generated."
Write-Host "Manifest: $manifestPath"
Write-Host "Checksums: $checksumPath"
