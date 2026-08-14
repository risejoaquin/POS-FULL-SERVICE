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

Assert-FileContains "PosCore\Security\PosInstallerSmokeInstallSimulationPackageExtractionValidation.cs" "PosInstallerSmokeInstallSimulationPackageExtractionValidation"
Assert-FileContains "PosCore\Security\PosInstallerSmokeInstallSimulationPackageExtractionValidation.cs" "POS Installer Smoke Install Simulation and Package Extraction Validation"
Assert-FileContains "PosCore\Security\PosInstallerSmokeInstallSimulationPackageExtractionValidation.cs" "installer smoke install simulation package extraction validation documented"
Assert-FileContains "PosCore\Security\PosInstallerSmokeInstallSimulationPackageExtractionValidation.cs" "PHASE 9C installer package integrity verification prerequisite documented"
Assert-FileContains "PosCore\Security\PosInstallerSmokeInstallSimulationPackageExtractionValidation.cs" "455 tests passed source evidence documented"
Assert-FileContains "PosCore\Security\PosInstallerSmokeInstallSimulationPackageExtractionValidation.cs" "460 tests expected after installer smoke install simulation package extraction validation documented"
Assert-FileContains "PosCore\Security\PosInstallerSmokeInstallSimulationPackageExtractionValidation.cs" "simulated install directory creation documented"
Assert-FileContains "PosCore\Security\PosInstallerSmokeInstallSimulationPackageExtractionValidation.cs" "installer package extraction to simulated install directory documented"
Assert-FileContains "PosCore\Security\PosInstallerSmokeInstallSimulationPackageExtractionValidation.cs" "simulated install smoke evidence manifest documented"
Assert-FileContains "PosCore\Security\PosInstallerSmokeInstallSimulationPackageExtractionValidation.cs" "no real installer execution"
Assert-FileContains "PosCore\Security\PosInstallerSmokeInstallSimulationPackageExtractionValidation.cs" "no checkout behavior change"
Assert-FileContains "PosCore\Security\PosInstallerSmokeInstallSimulationPackageExtractionValidation.cs" "no inventory mutation"
Assert-FileContains "PosCore\Security\PosInstallerSmokeInstallSimulationPackageExtractionValidation.cs" "no production sync enablement"
Assert-FileContains "PosCore\Security\PosInstallerSmokeInstallSimulationPackageExtractionValidation.cs" "no deployment execution"
Assert-FileContains "PosCore\Security\PosInstallerSmokeInstallSimulationPackageExtractionValidation.cs" "no public API behavior change"
Assert-FileContains "PosCore\Security\PosInstallerSmokeInstallSimulationPackageExtractionValidation.cs" "no schema change"
Assert-FileContains "PosCore\Security\PosInstallerSmokeInstallSimulationPackageExtractionValidation.cs" "no migrations"

Assert-FileContains "scripts\release\Simulate-Phase9InstallerSmokeInstall.ps1" "param("
Assert-FileContains "scripts\release\Simulate-Phase9InstallerSmokeInstall.ps1" "Verify-Phase9InstallerPackageIntegrity.ps1"
Assert-FileContains "scripts\release\Simulate-Phase9InstallerSmokeInstall.ps1" "smoke-install"
Assert-FileContains "scripts\release\Simulate-Phase9InstallerSmokeInstall.ps1" "Expand-Archive"
Assert-FileContains "scripts\release\Simulate-Phase9InstallerSmokeInstall.ps1" "poscore-win-x64"
Assert-FileContains "scripts\release\Simulate-Phase9InstallerSmokeInstall.ps1" "posbuilder-win-x64"
Assert-FileContains "scripts\release\Simulate-Phase9InstallerSmokeInstall.ps1" "posserver"
Assert-FileContains "scripts\release\Simulate-Phase9InstallerSmokeInstall.ps1" "smoke-install-evidence.json"
Assert-FileContains "scripts\release\Simulate-Phase9InstallerSmokeInstall.ps1" "PHASE 9D installer smoke install simulation verified."

Assert-FileContains "docs\POS_INSTALLER_SMOKE_INSTALL_SIMULATION_PACKAGE_EXTRACTION_VALIDATION.md" "installer smoke install simulation package extraction validation documented"
Assert-FileContains "docs\PHASE_9D_INSTALLER_SMOKE_INSTALL_SIMULATION_PACKAGE_EXTRACTION_VALIDATION.md" "460 tests passed"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_9D.md" "Release Execution advanced from 30% to 40%"
Assert-FileContains "README.md" "PHASE 9D"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "PHASE 9D"

Write-Host "PHASE 9D markers verified."
