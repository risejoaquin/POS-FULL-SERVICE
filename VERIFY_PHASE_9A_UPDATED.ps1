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

Assert-FileContains "PosCore\Security\PosInstallerGenerationReleaseArtifactExecution.cs" "PosInstallerGenerationReleaseArtifactExecution"
Assert-FileContains "PosCore\Security\PosInstallerGenerationReleaseArtifactExecution.cs" "POS Installer Generation and Release Artifact Execution"
Assert-FileContains "PosCore\Security\PosInstallerGenerationReleaseArtifactExecution.cs" "installer generation and release artifact execution documented"
Assert-FileContains "PosCore\Security\PosInstallerGenerationReleaseArtifactExecution.cs" "PHASE 8J go no-go operational readiness prerequisite documented"
Assert-FileContains "PosCore\Security\PosInstallerGenerationReleaseArtifactExecution.cs" "440 tests passed source evidence documented"
Assert-FileContains "PosCore\Security\PosInstallerGenerationReleaseArtifactExecution.cs" "445 tests expected after installer generation execution baseline documented"
Assert-FileContains "PosCore\Security\PosInstallerGenerationReleaseArtifactExecution.cs" "dotnet publish PosCore artifact command documented"
Assert-FileContains "PosCore\Security\PosInstallerGenerationReleaseArtifactExecution.cs" "dotnet publish PosBuilder artifact command documented"
Assert-FileContains "PosCore\Security\PosInstallerGenerationReleaseArtifactExecution.cs" "dotnet publish PosServer artifact command documented"
Assert-FileContains "PosCore\Security\PosInstallerGenerationReleaseArtifactExecution.cs" "release artifact output directory documented"
Assert-FileContains "PosCore\Security\PosInstallerGenerationReleaseArtifactExecution.cs" "release manifest generation command documented"
Assert-FileContains "PosCore\Security\PosInstallerGenerationReleaseArtifactExecution.cs" "SHA-256 checksum generation command documented"
Assert-FileContains "PosCore\Security\PosInstallerGenerationReleaseArtifactExecution.cs" "release artifact execution script documented"
Assert-FileContains "PosCore\Security\PosInstallerGenerationReleaseArtifactExecution.cs" "installer input artifact checklist documented"
Assert-FileContains "PosCore\Security\PosInstallerGenerationReleaseArtifactExecution.cs" "setup package generation readiness documented"
Assert-FileContains "PosCore\Security\PosInstallerGenerationReleaseArtifactExecution.cs" "artifact verification after publish documented"
Assert-FileContains "PosCore\Security\PosInstallerGenerationReleaseArtifactExecution.cs" "operator execution command documented"
Assert-FileContains "PosCore\Security\PosInstallerGenerationReleaseArtifactExecution.cs" "release candidate artifact archive documented"
Assert-FileContains "PosCore\Security\PosInstallerGenerationReleaseArtifactExecution.cs" "execution failure handling checklist documented"
Assert-FileContains "PosCore\Security\PosInstallerGenerationReleaseArtifactExecution.cs" "no checkout behavior change"
Assert-FileContains "PosCore\Security\PosInstallerGenerationReleaseArtifactExecution.cs" "no inventory mutation"
Assert-FileContains "PosCore\Security\PosInstallerGenerationReleaseArtifactExecution.cs" "no production sync enablement"
Assert-FileContains "PosCore\Security\PosInstallerGenerationReleaseArtifactExecution.cs" "no deployment execution"
Assert-FileContains "PosCore\Security\PosInstallerGenerationReleaseArtifactExecution.cs" "no schema change"
Assert-FileContains "PosCore\Security\PosInstallerGenerationReleaseArtifactExecution.cs" "no migrations"
Assert-FileContains "docs\POS_INSTALLER_GENERATION_RELEASE_ARTIFACT_EXECUTION.md" "installer generation and release artifact execution documented"
Assert-FileContains "docs\POS_INSTALLER_GENERATION_RELEASE_ARTIFACT_EXECUTION.md" "Generate-Phase9ReleaseArtifacts.ps1"
Assert-FileContains "docs\POS_INSTALLER_GENERATION_RELEASE_ARTIFACT_EXECUTION.md" "dotnet publish PosCore\PosCore.csproj"
Assert-FileContains "docs\POS_INSTALLER_GENERATION_RELEASE_ARTIFACT_EXECUTION.md" "dotnet publish PosBuilder\PosBuilder.csproj"
Assert-FileContains "docs\POS_INSTALLER_GENERATION_RELEASE_ARTIFACT_EXECUTION.md" "dotnet publish PosServer\PosServer.csproj"
Assert-FileContains "docs\PHASE_9A_INSTALLER_GENERATION_RELEASE_ARTIFACT_EXECUTION.md" "440 tests passed"
Assert-FileContains "docs\PHASE_9A_INSTALLER_GENERATION_RELEASE_ARTIFACT_EXECUTION.md" "445 tests passed"
Assert-FileContains "docs\PHASE_9A_INSTALLER_GENERATION_RELEASE_ARTIFACT_EXECUTION.md" "0 Advertencia(s)"
Assert-FileContains "docs\PHASE_9A_INSTALLER_GENERATION_RELEASE_ARTIFACT_EXECUTION.md" "0 Errores"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_9A.md" "Release Execution: 0% -> 10%"
Assert-FileContains "scripts\release\Generate-Phase9ReleaseArtifacts.ps1" "dotnet publish"
Assert-FileContains "scripts\release\Generate-Phase9ReleaseArtifacts.ps1" "release-manifest.json"
Assert-FileContains "scripts\release\Generate-Phase9ReleaseArtifacts.ps1" "checksums.sha256"
Assert-FileContains "README.md" "PHASE 9A"
Assert-FileContains "README.md" "Installer Generation and Release Artifact Execution"
Assert-FileContains "README.md" "445 tests passed"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "PHASE 9A"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "Installer Generation and Release Artifact Execution"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "Release Execution: 0% -> 10%"
Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "PosInstallerGenerationReleaseArtifactExecution_Should_Define_Installer_Generation_Execution_Checks"
Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "Phase9A_Release_Script_Should_Document_Publish_Manifest_And_Checksum_Execution"
Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "VerifyPhase9A_Should_Require_Installer_Generation_Release_Artifact_Execution_Markers"

Write-Host "PHASE 9A markers verified."
