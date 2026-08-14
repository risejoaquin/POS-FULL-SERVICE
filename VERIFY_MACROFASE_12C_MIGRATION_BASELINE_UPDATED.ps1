Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Resolve-RepoRoot {
    $current = Get-Location
    while ($null -ne $current) {
        if (Test-Path (Join-Path $current "Pos.sln")) {
            return $current.Path
        }
        $current = $current.Parent
    }
    throw "Could not locate repository root containing Pos.sln."
}

$repoRoot = Resolve-RepoRoot
$requiredFiles = @(
    "docs\MACROFASE_12C_MIGRATION_RESET_INITIAL_BASELINE.md",
    "docs\INITIAL_PRODUCTION_BASELINE_RUNBOOK.md",
    "docs\PROJECT_PROGRESS_REPORT_MACROFASE_12C.md",
    "scripts\database\Invoke-Macrofase12C-MigrationResetAndBaseline.ps1",
    "scripts\database\Reset-Supabase-PublicSchema-Macrofase12C.sql",
    "PosInfrastructure\Data\Server\CentralDbContextDesignTimeFactory.cs",
    "docs\MACROFASE_12C_DESIGN_TIME_FACTORY_HOTFIX.md"
)

foreach ($relativePath in $requiredFiles) {
    $path = Join-Path $repoRoot $relativePath
    if (-not (Test-Path $path)) {
        throw "Missing MACROFASE 12C file: $relativePath"
    }
}

$doc = Get-Content (Join-Path $repoRoot "docs\MACROFASE_12C_MIGRATION_RESET_INITIAL_BASELINE.md") -Raw
$runbook = Get-Content (Join-Path $repoRoot "docs\INITIAL_PRODUCTION_BASELINE_RUNBOOK.md") -Raw
$script = Get-Content (Join-Path $repoRoot "scripts\database\Invoke-Macrofase12C-MigrationResetAndBaseline.ps1") -Raw
$sql = Get-Content (Join-Path $repoRoot "scripts\database\Reset-Supabase-PublicSchema-Macrofase12C.sql") -Raw
$progress = Get-Content (Join-Path $repoRoot "docs\PROJECT_PROGRESS_REPORT_MACROFASE_12C.md") -Raw
$factory = Get-Content (Join-Path $repoRoot "PosInfrastructure\Data\Server\CentralDbContextDesignTimeFactory.cs") -Raw
$hotfix = Get-Content (Join-Path $repoRoot "docs\MACROFASE_12C_DESIGN_TIME_FACTORY_HOTFIX.md") -Raw

$markers = @(
    "MACROFASE 12C migration reset and InitialProductionBaseline documented.",
    "InitialProductionBaseline",
    "42P07: relation",
    "DROP SCHEMA IF EXISTS public CASCADE",
    "Migrations_Backup_PreMacro12",
    "dotnet ef migrations add",
    "MACROFASE 12 overall: 55% complete",
    "CentralDbContextDesignTimeFactory",
    "IDesignTimeDbContextFactory<CentralDbContext>",
    "resolves the CentralDbContext constructor ambiguity"
)

$allContent = $doc + "`n" + $runbook + "`n" + $script + "`n" + $sql + "`n" + $progress + "`n" + $factory + "`n" + $hotfix
foreach ($marker in $markers) {
    if (-not $allContent.Contains($marker)) {
        throw "Missing MACROFASE 12C marker: $marker"
    }
}

Write-Host "MACROFASE 12C migration baseline reset tooling verified."
Write-Host "InitialProductionBaseline generation path documented."
Write-Host "Supabase public schema reset SQL documented."
Write-Host "Design-time factory hotfix verified."
Write-Host "Next step: generate InitialProductionBaseline, reset disposable Supabase schema, then redeploy Railway."

$hotfixV2Markers = @(
    "without executing JWT startup validation",
    "Local dotnet tools manifest detected",
    "MACROFASE 12C - dotnet-ef and design-time factory hotfix V2"
)

$hotfixV2Files = @(
    "PosInfrastructure/Data/Server/CentralDbContextDesignTimeFactory.cs",
    "scripts/database/Invoke-Macrofase12C-MigrationResetAndBaseline.ps1",
    "docs/MACROFASE_12C_DOTNET_EF_FACTORY_HOTFIX_V2.md",
    ".config/dotnet-tools.json"
)

foreach ($file in $hotfixV2Files) {
    if (-not (Test-Path (Join-Path $repoRoot $file))) {
        throw "Missing MACROFASE 12C hotfix V2 file: $file"
    }
}

foreach ($marker in $hotfixV2Markers) {
    $found = $false
    foreach ($file in $hotfixV2Files) {
        $path = Join-Path $repoRoot $file
        if ((Get-Content $path -Raw) -like "*$marker*") {
            $found = $true
            break
        }
    }
    if (-not $found) {
        throw "Missing MACROFASE 12C hotfix V2 marker: $marker"
    }
}

Write-Host "dotnet-ef local tool and design-time factory hotfix V2 verified."

$scriptPath = Join-Path $PSScriptRoot "scripts\database\Invoke-Macrofase12C-MigrationResetAndBaseline.ps1"
$scriptContent = Get-Content -Raw $scriptPath
if ($scriptContent -notlike '*InitialProductionBaseline migration already exists. Skipping generation.*') {
    throw 'Missing idempotent baseline generation guard.'
}

$factoryDocPath = Join-Path $PSScriptRoot "docs\MACROFASE_12C_DESIGN_TIME_FACTORY_HOTFIX.md"
$factoryDocContent = Get-Content -Raw $factoryDocPath
if ($factoryDocContent -notlike '*does not require JWT_KEY*') {
    throw 'Missing design-time JWT requirement marker.'
}

Write-Host "MACROFASE 12C idempotent baseline generation hotfix verified."
