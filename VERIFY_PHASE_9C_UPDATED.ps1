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

Assert-FileContains "PosCore\Security\PosInstallerPackageVerificationIntegrityExecution.cs" "PosInstallerPackageVerificationIntegrityExecution"
Assert-FileContains "PosCore\Security\PosInstallerPackageVerificationIntegrityExecution.cs" "POS Installer Package Verification and Integrity Execution"
Assert-FileContains "PosCore\Security\PosInstallerPackageVerificationIntegrityExecution.cs" "installer package verification integrity execution documented"
Assert-FileContains "PosCore\Security\PosInstallerPackageVerificationIntegrityExecution.cs" "PHASE 9B installer package generation prerequisite documented"
Assert-FileContains "PosCore\Security\PosInstallerPackageVerificationIntegrityExecution.cs" "450 tests passed source evidence documented"
Assert-FileContains "PosCore\Security\PosInstallerPackageVerificationIntegrityExecution.cs" "455 tests expected after installer package verification integrity execution documented"
Assert-FileContains "PosCore\Security\PosInstallerPackageVerificationIntegrityExecution.cs" "installer package archive SHA-256 verification documented"
Assert-FileContains "PosCore\Security\PosInstallerPackageVerificationIntegrityExecution.cs" "installer package unzip verification documented"
Assert-FileContains "PosCore\Security\PosInstallerPackageVerificationIntegrityExecution.cs" "operator package verification command documented"
Assert-FileContains "PosCore\Security\PosInstallerPackageVerificationIntegrityExecution.cs" "no checkout behavior change"
Assert-FileContains "PosCore\Security\PosInstallerPackageVerificationIntegrityExecution.cs" "no inventory mutation"
Assert-FileContains "PosCore\Security\PosInstallerPackageVerificationIntegrityExecution.cs" "no production sync enablement"
Assert-FileContains "PosCore\Security\PosInstallerPackageVerificationIntegrityExecution.cs" "no deployment execution"
Assert-FileContains "PosCore\Security\PosInstallerPackageVerificationIntegrityExecution.cs" "no public API behavior change"
Assert-FileContains "PosCore\Security\PosInstallerPackageVerificationIntegrityExecution.cs" "no schema change"
Assert-FileContains "PosCore\Security\PosInstallerPackageVerificationIntegrityExecution.cs" "no migrations"

Assert-FileContains "scripts\release\Verify-Phase9InstallerPackageIntegrity.ps1" "param("
Assert-FileContains "scripts\release\Verify-Phase9InstallerPackageIntegrity.ps1" "installer-package-manifest.json"
Assert-FileContains "scripts\release\Verify-Phase9InstallerPackageIntegrity.ps1" "installer-checksums.sha256"
Assert-FileContains "scripts\release\Verify-Phase9InstallerPackageIntegrity.ps1" "packageArchiveSha256"
Assert-FileContains "scripts\release\Verify-Phase9InstallerPackageIntegrity.ps1" "Expand-Archive"
Assert-FileContains "scripts\release\Verify-Phase9InstallerPackageIntegrity.ps1" "poscore-win-x64"
Assert-FileContains "scripts\release\Verify-Phase9InstallerPackageIntegrity.ps1" "posbuilder-win-x64"
Assert-FileContains "scripts\release\Verify-Phase9InstallerPackageIntegrity.ps1" "posserver"
Assert-FileContains "scripts\release\Verify-Phase9InstallerPackageIntegrity.ps1" "PHASE 9C installer package integrity verified."

Assert-FileContains "docs\POS_INSTALLER_PACKAGE_VERIFICATION_INTEGRITY_EXECUTION.md" "installer package verification integrity execution documented"
Assert-FileContains "docs\PHASE_9C_INSTALLER_PACKAGE_VERIFICATION_INTEGRITY_EXECUTION.md" "455 tests passed"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_9C.md" "Release Execution from 20% to 30%"
Assert-FileContains "README.md" "PHASE 9C"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "PHASE 9C"

Write-Host "PHASE 9C markers verified."
