param(
    [switch]$ApplyMigrationReset,
    [switch]$GenerateBaseline,
    [switch]$EmitSupabaseResetSql,
    [switch]$InstallLocalDotnetEf,
    [string]$MigrationName = "InitialProductionBaseline",
    [string]$Context = "CentralDbContext",
    [string]$Project = "PosInfrastructure",
    [string]$StartupProject = "PosServer",
    [string]$OutputDir = "Migrations"
)

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

function Invoke-DotnetCommand {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments,
        [string]$FailureMessage
    )

    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        if ([string]::IsNullOrWhiteSpace($FailureMessage)) {
            throw "dotnet command failed: dotnet $($Arguments -join ' ')"
        }
        throw $FailureMessage
    }
}

function Test-DotnetEfAvailable {
    try {
        $versionOutput = & dotnet ef --version 2>&1
        if ($LASTEXITCODE -eq 0) {
            return [PSCustomObject]@{
                Available = $true
                Mode = "GlobalOrSdk"
                Version = ($versionOutput | Out-String).Trim()
            }
        }
    }
    catch {
        # Intentionally fall through to local tool check.
    }

    try {
        $localVersionOutput = & dotnet tool run dotnet-ef -- --version 2>&1
        if ($LASTEXITCODE -eq 0) {
            return [PSCustomObject]@{
                Available = $true
                Mode = "LocalTool"
                Version = ($localVersionOutput | Out-String).Trim()
            }
        }
    }
    catch {
        # Intentionally return unavailable below.
    }

    return [PSCustomObject]@{
        Available = $false
        Mode = "Unavailable"
        Version = ""
    }
}

function Ensure-LocalDotnetEf {
    param([string]$RepoRoot)

    $toolManifest = Join-Path $RepoRoot ".config\dotnet-tools.json"

    if (-not (Test-Path $toolManifest)) {
        Write-Host "Creating local .NET tool manifest for dotnet-ef."
        Invoke-DotnetCommand -Arguments @("new", "tool-manifest") -FailureMessage "Failed to create local .NET tool manifest."
    }

    $toolList = & dotnet tool list --local 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to list local .NET tools."
    }

    if (($toolList | Out-String) -notmatch "dotnet-ef") {
        Write-Host "Installing dotnet-ef as a local repository tool."
        Invoke-DotnetCommand -Arguments @("tool", "install", "dotnet-ef", "--version", "8.*") -FailureMessage "Failed to install local dotnet-ef tool."
    }
    else {
        Write-Host "dotnet-ef local tool already listed in .config/dotnet-tools.json."
    }

    Write-Host "Restoring local .NET tools."
    Invoke-DotnetCommand -Arguments @("tool", "restore") -FailureMessage "Failed to restore local .NET tools."
}

$repoRoot = Resolve-RepoRoot
Set-Location $repoRoot

$migrationsDir = Join-Path $repoRoot "PosInfrastructure\Migrations"
$backupRoot = Join-Path $repoRoot "artifacts\database\migration-backups"
$timestamp = Get-Date -Format "yyyyMMddHHmmss"
$backupDir = Join-Path $backupRoot "Migrations_Backup_PreMacro12_$timestamp"
$sqlPath = Join-Path $repoRoot "scripts\database\Reset-Supabase-PublicSchema-Macrofase12C.sql"

Write-Host "MACROFASE 12C migration reset and InitialProductionBaseline script."
Write-Host "RepositoryRoot: $repoRoot"
Write-Host "MigrationsDir: $migrationsDir"
Write-Host "MigrationName: $MigrationName"
Write-Host "Context: $Context"

if (-not $ApplyMigrationReset -and -not $GenerateBaseline -and -not $EmitSupabaseResetSql) {
    Write-Host "MACROFASE 12C dry run only."
    Write-Host "No migration files were deleted."
    Write-Host "No Supabase schema was dropped."
    Write-Host "No InitialProductionBaseline migration was generated."
    Write-Host "Run with -ApplyMigrationReset to back up and remove old CentralDbContext migrations."
    Write-Host "Run with -GenerateBaseline to create InitialProductionBaseline after reset."
    Write-Host "Run with -GenerateBaseline -InstallLocalDotnetEf if dotnet ef is not available."
    Write-Host "Run with -EmitSupabaseResetSql to print the destructive reset SQL path."
    exit 0
}

if ($EmitSupabaseResetSql) {
    if (-not (Test-Path $sqlPath)) {
        throw "Supabase reset SQL not found: $sqlPath"
    }
    Write-Host "Supabase reset SQL path: $sqlPath"
    Write-Host "This SQL is destructive and must be executed intentionally in Supabase SQL Editor."
}

if ($ApplyMigrationReset) {
    if (-not (Test-Path $migrationsDir)) {
        throw "Migrations directory not found: $migrationsDir"
    }

    New-Item -ItemType Directory -Force -Path $backupDir | Out-Null
    $existingFiles = Get-ChildItem -Path $migrationsDir -File -Include "*.cs" -Recurse
    if ($existingFiles.Count -gt 0) {
        Copy-Item -Path (Join-Path $migrationsDir "*") -Destination $backupDir -Recurse -Force
        Write-Host "Existing CentralDbContext migrations backed up to: $backupDir"
        $existingFiles | Remove-Item -Force
        Write-Host "Old CentralDbContext migration files removed from: $migrationsDir"
    }
    else {
        Write-Host "No CentralDbContext migration files found to remove. Migration reset already appears applied."
    }

    Write-Host "MACROFASE 12C migration reset applied locally."
}

if ($GenerateBaseline) {
    $dotnetEf = Test-DotnetEfAvailable

    $toolManifest = Join-Path $repoRoot ".config\dotnet-tools.json"
    if (-not $dotnetEf.Available -and (Test-Path $toolManifest)) {
        Write-Host "Local dotnet tools manifest detected. Restoring local tools before retrying dotnet-ef."
        Invoke-DotnetCommand -Arguments @("tool", "restore") -FailureMessage "Failed to restore local .NET tools."
        $dotnetEf = Test-DotnetEfAvailable
    }

    if (-not $dotnetEf.Available -and $InstallLocalDotnetEf) {
        Ensure-LocalDotnetEf -RepoRoot $repoRoot
        $dotnetEf = Test-DotnetEfAvailable
    }

    if (-not $dotnetEf.Available) {
        Write-Host "dotnet ef is not available in this shell. Installing local dotnet-ef automatically for MACROFASE 12C baseline generation."
        Ensure-LocalDotnetEf -RepoRoot $repoRoot
        $dotnetEf = Test-DotnetEfAvailable
    }

    if (-not $dotnetEf.Available) {
        throw @"
dotnet ef is not available even after local tool restore/install attempt.
Manual fix:
  dotnet tool restore
  dotnet tool install dotnet-ef --version 8.*
  .\scripts\database\Invoke-Macrofase12C-MigrationResetAndBaseline.ps1 -GenerateBaseline
"@
    }

    $existingBaselineFiles = @(Get-ChildItem -Path $migrationsDir -File -Filter "*$MigrationName*.cs" -ErrorAction SilentlyContinue)
    if ($existingBaselineFiles.Count -gt 0) {
        Write-Host "InitialProductionBaseline migration already exists. Skipping generation."
        Write-Host "Existing baseline files:"
        $existingBaselineFiles | ForEach-Object { Write-Host " - $($_.FullName)" }
        & dotnet build -c Release Pos.sln
        if ($LASTEXITCODE -ne 0) {
            throw "Release build failed while verifying existing InitialProductionBaseline migration."
        }
        Write-Host "MACROFASE 12C InitialProductionBaseline already present and Release build verified."
        exit 0
    }

    Write-Host "dotnet ef mode: $($dotnetEf.Mode)"
    Write-Host "dotnet ef version: $($dotnetEf.Version)"
    Write-Host "Generating InitialProductionBaseline migration..."
    Write-Host "Expected design-time factory: PosInfrastructure/Data/Server/CentralDbContextDesignTimeFactory.cs"
    Write-Host "EF tooling should not require JWT_KEY/JWT_ISSUER/JWT_AUDIENCE during migration generation."

    if ($dotnetEf.Mode -eq "LocalTool") {
        & dotnet tool run dotnet-ef -- migrations add $MigrationName --context $Context --project $Project --startup-project $StartupProject --output-dir $OutputDir
    }
    else {
        & dotnet ef migrations add $MigrationName --context $Context --project $Project --startup-project $StartupProject --output-dir $OutputDir
    }

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet ef migrations add failed."
    }

    & dotnet build -c Release Pos.sln
    if ($LASTEXITCODE -ne 0) {
        throw "Release build failed after InitialProductionBaseline generation."
    }

    Write-Host "MACROFASE 12C InitialProductionBaseline generated and Release build verified."
}
