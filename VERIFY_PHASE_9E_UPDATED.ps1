$ErrorActionPreference = "Stop"

function Assert-FileContains {
    param(
        [string]$Path,
        [string]$Text
    )

    if (!(Test-Path $Path)) {
        throw "Missing file: $Path"
    }

    $content = Get-Content -Raw -Path $Path
    if (!$content.Contains($Text)) {
        throw "Missing marker in ${Path}: $Text"
    }
}

Assert-FileContains "PosCore\Security\PosInstallerLaunchScriptDesktopShortcutPackaging.cs" "PosInstallerLaunchScriptDesktopShortcutPackaging"
Assert-FileContains "PosCore\Security\PosInstallerLaunchScriptDesktopShortcutPackaging.cs" "POS Installer Launch Script and Desktop Shortcut Packaging"
Assert-FileContains "PosCore\Security\PosInstallerLaunchScriptDesktopShortcutPackaging.cs" "installer launch script desktop shortcut packaging documented"
Assert-FileContains "PosCore\Security\PosInstallerLaunchScriptDesktopShortcutPackaging.cs" "PHASE 9D smoke install simulation prerequisite documented"
Assert-FileContains "PosCore\Security\PosInstallerLaunchScriptDesktopShortcutPackaging.cs" "460 tests passed source evidence documented"
Assert-FileContains "PosCore\Security\PosInstallerLaunchScriptDesktopShortcutPackaging.cs" "465 tests expected after installer launch script desktop shortcut packaging documented"
Assert-FileContains "PosCore\Security\PosInstallerLaunchScriptDesktopShortcutPackaging.cs" "desktop shortcut creation script packaged but not executed"
Assert-FileContains "PosCore\Security\PosInstallerLaunchScriptDesktopShortcutPackaging.cs" "launch package archive generation documented"
Assert-FileContains "PosCore\Security\PosInstallerLaunchScriptDesktopShortcutPackaging.cs" "no real shortcut creation"
Assert-FileContains "PosCore\Security\PosInstallerLaunchScriptDesktopShortcutPackaging.cs" "no real installer execution"
Assert-FileContains "PosCore\Security\PosInstallerLaunchScriptDesktopShortcutPackaging.cs" "no checkout behavior change"
Assert-FileContains "PosCore\Security\PosInstallerLaunchScriptDesktopShortcutPackaging.cs" "no inventory mutation"
Assert-FileContains "PosCore\Security\PosInstallerLaunchScriptDesktopShortcutPackaging.cs" "no production sync enablement"
Assert-FileContains "PosCore\Security\PosInstallerLaunchScriptDesktopShortcutPackaging.cs" "no deployment execution"
Assert-FileContains "PosCore\Security\PosInstallerLaunchScriptDesktopShortcutPackaging.cs" "no public API behavior change"
Assert-FileContains "PosCore\Security\PosInstallerLaunchScriptDesktopShortcutPackaging.cs" "no schema change"
Assert-FileContains "PosCore\Security\PosInstallerLaunchScriptDesktopShortcutPackaging.cs" "no migrations"
Assert-FileContains "scripts\release\Generate-Phase9LaunchAndShortcutPackage.ps1" "param("
Assert-FileContains "scripts\release\Generate-Phase9LaunchAndShortcutPackage.ps1" "Simulate-Phase9InstallerSmokeInstall.ps1"
Assert-FileContains "scripts\release\Generate-Phase9LaunchAndShortcutPackage.ps1" "Start-PosCore.ps1"
Assert-FileContains "scripts\release\Generate-Phase9LaunchAndShortcutPackage.ps1" "Start-PosBuilder.ps1"
Assert-FileContains "scripts\release\Generate-Phase9LaunchAndShortcutPackage.ps1" "Start-PosServer.ps1"
Assert-FileContains "scripts\release\Generate-Phase9LaunchAndShortcutPackage.ps1" "desktop-shortcut-spec.json"
Assert-FileContains "scripts\release\Generate-Phase9LaunchAndShortcutPackage.ps1" "Create-DesktopShortcuts.ps1"
Assert-FileContains "scripts\release\Generate-Phase9LaunchAndShortcutPackage.ps1" "launcher-package-manifest.json"
Assert-FileContains "scripts\release\Generate-Phase9LaunchAndShortcutPackage.ps1" "launcher-checksums.sha256"
Assert-FileContains "scripts\release\Generate-Phase9LaunchAndShortcutPackage.ps1" "PHASE 9E installer launch script and desktop shortcut package generated."
Assert-FileContains "docs\POS_INSTALLER_LAUNCH_SCRIPT_DESKTOP_SHORTCUT_PACKAGING.md" "installer launch script desktop shortcut packaging documented"
Assert-FileContains "docs\PHASE_9E_INSTALLER_LAUNCH_SCRIPT_DESKTOP_SHORTCUT_PACKAGING.md" "465 tests passed"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_9E.md" "Release Execution advanced from 40% to 50%"
Assert-FileContains "README.md" "PHASE 9E"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "PHASE 9E"

Write-Host "PHASE 9E markers verified."
