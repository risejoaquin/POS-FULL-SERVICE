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
$simulatedInstallRoot = Join-Path $smokeRoot "$PackageName-$ReleaseVersion"
$smokeEvidencePath = Join-Path $smokeRoot "smoke-install-evidence.json"
$smokeSimulator = Join-Path $root "scripts\release\Simulate-Phase9InstallerSmokeInstall.ps1"
$launchRoot = Join-Path $artifactRoot "launcher"
$launchStageRoot = Join-Path $launchRoot "$PackageName-launch-$ReleaseVersion"
$launchScriptDir = Join-Path $launchStageRoot "launch"
$launchPackageArchivePath = Join-Path $installerRoot "$PackageName-launch-$ReleaseVersion.zip"
$launcherManifestPath = Join-Path $installerRoot "launcher-package-manifest.json"
$launcherChecksumPath = Join-Path $installerRoot "launcher-checksums.sha256"

$requiredSmokeOutputs = @(
    $simulatedInstallRoot,
    $smokeEvidencePath
)

$missingSmokeOutputs = @()
foreach ($requiredOutput in $requiredSmokeOutputs) {
    if (!(Test-Path $requiredOutput)) {
        $missingSmokeOutputs += $requiredOutput
    }
}

if ($missingSmokeOutputs.Count -gt 0) {
    if (!(Test-Path $smokeSimulator)) {
        throw "Missing PHASE 9D smoke install outputs and smoke simulation script was not found: $smokeSimulator"
    }

    Write-Host "PHASE 9D smoke install outputs are missing. Regenerating smoke install simulation before launcher packaging."
    & $smokeSimulator -ReleaseVersion $ReleaseVersion -ReleaseChannel $ReleaseChannel -PackageName $PackageName
}

foreach ($requiredOutput in $requiredSmokeOutputs) {
    if (!(Test-Path $requiredOutput)) {
        throw "Missing launcher packaging input after prerequisite simulation: $requiredOutput"
    }
}

if (Test-Path $launchStageRoot) {
    Remove-Item -Recurse -Force $launchStageRoot
}

New-Item -ItemType Directory -Force -Path $launchStageRoot | Out-Null
New-Item -ItemType Directory -Force -Path $launchScriptDir | Out-Null
New-Item -ItemType Directory -Force -Path $installerRoot | Out-Null

Copy-Item -Path (Join-Path $simulatedInstallRoot "*") -Destination $launchStageRoot -Recurse -Force

$requiredApplicationFolders = @(
    "poscore-win-x64",
    "posbuilder-win-x64",
    "posserver"
)

foreach ($folder in $requiredApplicationFolders) {
    if (!(Test-Path (Join-Path $launchStageRoot $folder))) {
        throw "Missing required launcher package application folder: $folder"
    }
}

function New-LaunchScript {
    param(
        [string]$ScriptPath,
        [string]$RelativeFolder,
        [string]$PreferredExe,
        [string]$FallbackDll
    )

    $content = @"
param()
`$ErrorActionPreference = "Stop"
`$scriptRoot = Split-Path -Parent `$MyInvocation.MyCommand.Path
`$installRoot = Resolve-Path (Join-Path `$scriptRoot "..")
`$appRoot = Join-Path `$installRoot "$RelativeFolder"
`$exePath = Join-Path `$appRoot "$PreferredExe"
`$dllPath = Join-Path `$appRoot "$FallbackDll"

if (Test-Path `$exePath) {
    Start-Process -FilePath `$exePath -WorkingDirectory `$appRoot
    return
}

if (Test-Path `$dllPath) {
    Start-Process -FilePath "dotnet" -ArgumentList @(`$dllPath) -WorkingDirectory `$appRoot
    return
}

throw "Launch target not found for $RelativeFolder. Checked `$exePath and `$dllPath."
"@

    Set-Content -Path $ScriptPath -Value $content -Encoding UTF8
}

New-LaunchScript -ScriptPath (Join-Path $launchScriptDir "Start-PosCore.ps1") -RelativeFolder "poscore-win-x64" -PreferredExe "PosCore.exe" -FallbackDll "PosCore.dll"
New-LaunchScript -ScriptPath (Join-Path $launchScriptDir "Start-PosBuilder.ps1") -RelativeFolder "posbuilder-win-x64" -PreferredExe "PosBuilder.exe" -FallbackDll "PosBuilder.dll"
New-LaunchScript -ScriptPath (Join-Path $launchScriptDir "Start-PosServer.ps1") -RelativeFolder "posserver" -PreferredExe "PosServer.exe" -FallbackDll "PosServer.dll"

$desktopShortcutSpec = [ordered]@{
    phase = "PHASE 9E"
    releaseVersion = $ReleaseVersion
    releaseChannel = $ReleaseChannel
    packagedOnly = $true
    noRealShortcutCreation = $true
    shortcuts = @(
        [ordered]@{ name = "POS Core"; targetScript = "launch\Start-PosCore.ps1"; workingDirectory = "poscore-win-x64" },
        [ordered]@{ name = "POS Builder"; targetScript = "launch\Start-PosBuilder.ps1"; workingDirectory = "posbuilder-win-x64" },
        [ordered]@{ name = "POS Server"; targetScript = "launch\Start-PosServer.ps1"; workingDirectory = "posserver" }
    )
}
$desktopShortcutSpecPath = Join-Path $launchScriptDir "desktop-shortcut-spec.json"
$desktopShortcutSpec | ConvertTo-Json -Depth 8 | Set-Content -Path $desktopShortcutSpecPath -Encoding UTF8

$shortcutCreationScript = @"
param(
    [string]`$ShortcutDirectory = [Environment]::GetFolderPath("Desktop")
)

`$ErrorActionPreference = "Stop"
`$scriptRoot = Split-Path -Parent `$MyInvocation.MyCommand.Path
`$specPath = Join-Path `$scriptRoot "desktop-shortcut-spec.json"

if (!(Test-Path `$specPath)) {
    throw "Desktop shortcut specification not found: `$specPath"
}

Write-Host "PHASE 9E desktop shortcut creation script packaged only. Review desktop-shortcut-spec.json before execution."
Write-Host "No shortcut is created by packaging verification."
"@
Set-Content -Path (Join-Path $launchScriptDir "Create-DesktopShortcuts.ps1") -Value $shortcutCreationScript -Encoding UTF8

$requiredLaunchContents = @(
    "launch\Start-PosCore.ps1",
    "launch\Start-PosBuilder.ps1",
    "launch\Start-PosServer.ps1",
    "launch\Create-DesktopShortcuts.ps1",
    "launch\desktop-shortcut-spec.json"
)

foreach ($requiredContent in $requiredLaunchContents) {
    $contentPath = Join-Path $launchStageRoot $requiredContent
    if (!(Test-Path $contentPath)) {
        throw "Missing launcher package content: $requiredContent"
    }
}

if (Test-Path $launchPackageArchivePath) {
    Remove-Item -Force $launchPackageArchivePath
}

Compress-Archive -Path (Join-Path $launchStageRoot "*") -DestinationPath $launchPackageArchivePath -Force

$packageHash = Get-FileHash -Algorithm SHA256 -Path $launchPackageArchivePath
$allLaunchFiles = Get-ChildItem -Path $launchStageRoot -Recurse -File
$launchScripts = Get-ChildItem -Path $launchScriptDir -Filter "*.ps1" -File

$manifest = [ordered]@{
    phase = "PHASE 9E"
    releaseVersion = $ReleaseVersion
    releaseChannel = $ReleaseChannel
    sourceSmokeInstallRoot = $simulatedInstallRoot
    launcherStageRoot = $launchStageRoot
    launchPackageArchive = $launchPackageArchivePath
    launchPackageArchiveSha256 = $packageHash.Hash
    launchScripts = @($launchScripts | ForEach-Object { $_.FullName })
    desktopShortcutSpec = $desktopShortcutSpecPath
    totalFileCount = $allLaunchFiles.Count
    generatedAtUtc = [DateTime]::UtcNow.ToString("o")
    noRealShortcutCreation = $true
    noRealInstallerExecution = $true
    noDeploymentExecution = $true
}
$manifest | ConvertTo-Json -Depth 8 | Set-Content -Path $launcherManifestPath -Encoding UTF8

$checksumLines = @()
$checksumLines += "$($packageHash.Hash)  $launchPackageArchivePath"
foreach ($file in $allLaunchFiles) {
    $fileHash = Get-FileHash -Algorithm SHA256 -Path $file.FullName
    $checksumLines += "$($fileHash.Hash)  $($file.FullName)"
}
$checksumLines | Set-Content -Path $launcherChecksumPath -Encoding UTF8

if (!(Test-Path $launchPackageArchivePath)) { throw "Launch package archive generation failed." }
if (!(Test-Path $launcherManifestPath)) { throw "Launcher manifest generation failed." }
if (!(Test-Path $launcherChecksumPath)) { throw "Launcher checksum generation failed." }

Write-Host "PHASE 9E installer launch script and desktop shortcut package generated."
Write-Host "LaunchPackage: $launchPackageArchivePath"
Write-Host "Manifest: $launcherManifestPath"
Write-Host "Checksums: $launcherChecksumPath"
Write-Host "ShortcutSpec: $desktopShortcutSpecPath"
Write-Host "LaunchScripts: $($launchScripts.Count)"
