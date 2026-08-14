$ErrorActionPreference = "Stop"

function Assert-FileContains {
    param(
        [string]$Path,
        [string]$Text
    )

    if (!(Test-Path $Path)) {
        throw "Missing required file: $Path"
    }

    $content = Get-Content $Path -Raw
    if (!$content.Contains($Text)) {
        throw "Missing marker '$Text' in $Path"
    }
}

Assert-FileContains "PosCore\Security\PosInstallerPackageGenerationExecution.cs" "PosInstallerPackageGenerationExecution"
Assert-FileContains "PosCore\Security\PosInstallerPackageGenerationExecution.cs" "POS Installer Package Generation Execution"
Assert-FileContains "PosCore\Security\PosInstallerPackageGenerationExecution.cs" "installer package generation execution documented"
Assert-FileContains "PosCore\Security\PosInstallerPackageGenerationExecution.cs" "PHASE 9A release artifact execution prerequisite documented"
Assert-FileContains "PosCore\Security\PosInstallerPackageGenerationExecution.cs" "445 tests passed source evidence documented"
Assert-FileContains "PosCore\Security\PosInstallerPackageGenerationExecution.cs" "450 tests expected after installer package generation execution documented"
Assert-FileContains "PosCore\Security\PosInstallerPackageGenerationExecution.cs" "published artifact source directory documented"
Assert-FileContains "PosCore\Security\PosInstallerPackageGenerationExecution.cs" "installer package staging directory documented"
Assert-FileContains "PosCore\Security\PosInstallerPackageGenerationExecution.cs" "PosCore published artifact input documented"
Assert-FileContains "PosCore\Security\PosInstallerPackageGenerationExecution.cs" "PosBuilder published artifact input documented"
Assert-FileContains "PosCore\Security\PosInstallerPackageGenerationExecution.cs" "PosServer published artifact input documented"
Assert-FileContains "PosCore\Security\PosInstallerPackageGenerationExecution.cs" "release manifest input documented"
Assert-FileContains "PosCore\Security\PosInstallerPackageGenerationExecution.cs" "checksums input documented"
Assert-FileContains "PosCore\Security\PosInstallerPackageGenerationExecution.cs" "installer package manifest generation documented"
Assert-FileContains "PosCore\Security\PosInstallerPackageGenerationExecution.cs" "installer package checksum generation documented"
Assert-FileContains "PosCore\Security\PosInstallerPackageGenerationExecution.cs" "installer package zip archive generation documented"
Assert-FileContains "PosCore\Security\PosInstallerPackageGenerationExecution.cs" "operator package generation command documented"
Assert-FileContains "PosCore\Security\PosInstallerPackageGenerationExecution.cs" "no checkout behavior change"
Assert-FileContains "PosCore\Security\PosInstallerPackageGenerationExecution.cs" "no inventory mutation"
Assert-FileContains "PosCore\Security\PosInstallerPackageGenerationExecution.cs" "no production sync enablement"
Assert-FileContains "PosCore\Security\PosInstallerPackageGenerationExecution.cs" "no deployment execution"
Assert-FileContains "PosCore\Security\PosInstallerPackageGenerationExecution.cs" "no schema change"
Assert-FileContains "PosCore\Security\PosInstallerPackageGenerationExecution.cs" "no migrations"
Assert-FileContains "docs\POS_INSTALLER_PACKAGE_GENERATION_EXECUTION.md" "Generate-Phase9InstallerPackage.ps1"
Assert-FileContains "docs\POS_INSTALLER_PACKAGE_GENERATION_EXECUTION.md" "pos-installer-package-0.9.0-rc.1.zip"
Assert-FileContains "docs\PHASE_9B_INSTALLER_PACKAGE_GENERATION_EXECUTION.md" "445 tests passed"
Assert-FileContains "docs\PHASE_9B_INSTALLER_PACKAGE_GENERATION_EXECUTION.md" "450 tests passed"
Assert-FileContains "docs\PHASE_9B_INSTALLER_PACKAGE_GENERATION_EXECUTION.md" "0 Advertencia(s)"
Assert-FileContains "docs\PHASE_9B_INSTALLER_PACKAGE_GENERATION_EXECUTION.md" "0 Errores"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_9B.md" "Release Execution: 10% -> 20%"
Assert-FileContains "scripts\release\Generate-Phase9InstallerPackage.ps1" "Compress-Archive"
Assert-FileContains "scripts\release\Generate-Phase9InstallerPackage.ps1" "installer-package-manifest.json"
Assert-FileContains "scripts\release\Generate-Phase9InstallerPackage.ps1" "installer-checksums.sha256"
Assert-FileContains "scripts\release\Generate-Phase9InstallerPackage.ps1" "pos-installer-package"
Assert-FileContains "README.md" "PHASE 9B"
Assert-FileContains "README.md" "Installer Package Generation Execution"
Assert-FileContains "README.md" "450 tests passed"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "PHASE 9B"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "Installer Package Generation Execution"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "Release Execution: 10% -> 20%"
Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "PosInstallerPackageGenerationExecution_Should_Define_Installer_Package_Generation_Checks"
Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "Phase9B_Package_Script_Should_Document_Installer_Package_Generation"
Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "VerifyPhase9B_Should_Require_Installer_Package_Generation_Markers"

Write-Host "PHASE 9B markers verified."
