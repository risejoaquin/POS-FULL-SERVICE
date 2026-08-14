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

Assert-FileContains "PosCore\Security\PosInstallerReadinessSetupPackagingBaseline.cs" "PosInstallerReadinessSetupPackagingBaseline"
Assert-FileContains "PosCore\Security\PosInstallerReadinessSetupPackagingBaseline.cs" "POS Installer Readiness and Setup Packaging Baseline"
Assert-FileContains "PosCore\Security\PosInstallerReadinessSetupPackagingBaseline.cs" "installer readiness and setup packaging baseline documented"
Assert-FileContains "PosCore\Security\PosInstallerReadinessSetupPackagingBaseline.cs" "PHASE 8D checksum artifact verification prerequisite documented"
Assert-FileContains "PosCore\Security\PosInstallerReadinessSetupPackagingBaseline.cs" "410 tests passed source evidence documented"
Assert-FileContains "PosCore\Security\PosInstallerReadinessSetupPackagingBaseline.cs" "415 tests expected after installer readiness baseline documented"
Assert-FileContains "PosCore\Security\PosInstallerReadinessSetupPackagingBaseline.cs" "Windows installer target documented"
Assert-FileContains "PosCore\Security\PosInstallerReadinessSetupPackagingBaseline.cs" "setup packaging input artifact checklist documented"
Assert-FileContains "PosCore\Security\PosInstallerReadinessSetupPackagingBaseline.cs" "installer output naming convention documented"
Assert-FileContains "PosCore\Security\PosInstallerReadinessSetupPackagingBaseline.cs" "installer version stamp checklist documented"
Assert-FileContains "PosCore\Security\PosInstallerReadinessSetupPackagingBaseline.cs" "installer checksum linkage documented"
Assert-FileContains "PosCore\Security\PosInstallerReadinessSetupPackagingBaseline.cs" "installer signing readiness checklist documented"
Assert-FileContains "PosCore\Security\PosInstallerReadinessSetupPackagingBaseline.cs" "installer smoke test checklist documented"
Assert-FileContains "PosCore\Security\PosInstallerReadinessSetupPackagingBaseline.cs" "install path verification checklist documented"
Assert-FileContains "PosCore\Security\PosInstallerReadinessSetupPackagingBaseline.cs" "upgrade path verification checklist documented"
Assert-FileContains "PosCore\Security\PosInstallerReadinessSetupPackagingBaseline.cs" "uninstall path verification checklist documented"
Assert-FileContains "PosCore\Security\PosInstallerReadinessSetupPackagingBaseline.cs" "operator installer review checklist documented"
Assert-FileContains "PosCore\Security\PosInstallerReadinessSetupPackagingBaseline.cs" "installer failure handling checklist documented"
Assert-FileContains "PosCore\Security\PosInstallerReadinessSetupPackagingBaseline.cs" "setup packaging audit evidence documented"
Assert-FileContains "PosCore\Security\PosInstallerReadinessSetupPackagingBaseline.cs" "no checkout behavior change"
Assert-FileContains "PosCore\Security\PosInstallerReadinessSetupPackagingBaseline.cs" "no inventory mutation"
Assert-FileContains "PosCore\Security\PosInstallerReadinessSetupPackagingBaseline.cs" "no production sync enablement"
Assert-FileContains "PosCore\Security\PosInstallerReadinessSetupPackagingBaseline.cs" "no packaging execution"
Assert-FileContains "PosCore\Security\PosInstallerReadinessSetupPackagingBaseline.cs" "no installer execution"
Assert-FileContains "PosCore\Security\PosInstallerReadinessSetupPackagingBaseline.cs" "no deployment execution"
Assert-FileContains "PosCore\Security\PosInstallerReadinessSetupPackagingBaseline.cs" "no schema change"
Assert-FileContains "PosCore\Security\PosInstallerReadinessSetupPackagingBaseline.cs" "no migrations"
Assert-FileContains "docs\POS_INSTALLER_READINESS_SETUP_PACKAGING_BASELINE.md" "installer readiness and setup packaging baseline documented"
Assert-FileContains "docs\POS_INSTALLER_READINESS_SETUP_PACKAGING_BASELINE.md" "PHASE 8D checksum artifact verification prerequisite documented"
Assert-FileContains "docs\POS_INSTALLER_READINESS_SETUP_PACKAGING_BASELINE.md" "Windows installer target documented"
Assert-FileContains "docs\POS_INSTALLER_READINESS_SETUP_PACKAGING_BASELINE.md" "setup packaging input artifact checklist documented"
Assert-FileContains "docs\POS_INSTALLER_READINESS_SETUP_PACKAGING_BASELINE.md" "installer output naming convention documented"
Assert-FileContains "docs\POS_INSTALLER_READINESS_SETUP_PACKAGING_BASELINE.md" "installer signing readiness checklist documented"
Assert-FileContains "docs\PHASE_8E_INSTALLER_READINESS_SETUP_PACKAGING_BASELINE.md" "415 tests passed"
Assert-FileContains "docs\PHASE_8E_INSTALLER_READINESS_SETUP_PACKAGING_BASELINE.md" "0 Advertencia(s)"
Assert-FileContains "docs\PHASE_8E_INSTALLER_READINESS_SETUP_PACKAGING_BASELINE.md" "0 Errores"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_8E.md" "40% -> 50%"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_8E.md" "Release Packaging and Operational Readiness"
Assert-FileContains "README.md" "PHASE 8E"
Assert-FileContains "README.md" "Installer Readiness and Setup Packaging Baseline"
Assert-FileContains "README.md" "415 tests passed"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "PHASE 8E"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "Installer Readiness and Setup Packaging Baseline"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "Release Packaging and Operational Readiness: 40% -> 50%"
Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "PosInstallerReadinessSetupPackagingBaseline_Should_Define_Installer_Readiness_Checks"
Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "Phase8E_Documentation_Should_Describe_Installer_Readiness_Baseline"
Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "VerifyPhase8E_Should_Require_Installer_Readiness_Markers"

Write-Host "PHASE 8E markers verified."
