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
    if ($content -notlike "*$Text*") {
        throw "Missing marker '$Text' in $Path"
    }
}

Assert-FileContains "PosCore\Security\PosSecurityDependencyHardening.cs" "PosSecurityDependencyHardening"
Assert-FileContains "PosCore\Security\PosSecurityDependencyHardening.cs" "POS Security Dependency Hardening"
Assert-FileContains "PosCore\Security\PosSecurityDependencyHardening.cs" "System.Text.Json"
Assert-FileContains "PosCore\Security\PosSecurityDependencyHardening.cs" "8.0.5"
Assert-FileContains "PosCore\Security\PosSecurityDependencyHardening.cs" "GHSA-8g4q-xg66-9fp4"
Assert-FileContains "PosCore\Security\PosSecurityDependencyHardening.cs" "GHSA-hh2w-p6rv-4g7w"
Assert-FileContains "PosCore\Security\PosSecurityDependencyHardening.cs" "no checkout behavior change"
Assert-FileContains "PosCore\Security\PosSecurityDependencyHardening.cs" "no inventory mutation"
Assert-FileContains "PosCore\Security\PosSecurityDependencyHardening.cs" "no production sync enablement"
Assert-FileContains "PosCore\Security\PosSecurityDependencyHardening.cs" "no schema change"
Assert-FileContains "PosCore\Security\PosSecurityDependencyHardening.cs" "no migrations"

Assert-FileContains "PosBuilder\PosBuilder.csproj" '<PackageReference Include="System.Text.Json" Version="8.0.5" />'

Assert-FileContains "docs\POS_SECURITY_DEPENDENCY_HARDENING.md" "System.Text.Json 8.0.5 pinned in PosBuilder"
Assert-FileContains "docs\PHASE_7A_SECURITY_DEPENDENCY_HARDENING.md" "PHASE 7A"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_7A.md" "0% -> 10%"
Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "PosBuilder_Should_Pin_SystemTextJson_To_Patched_805_Version"
Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "PHASE 7A markers verified."

Write-Host "PHASE 7A markers verified."
