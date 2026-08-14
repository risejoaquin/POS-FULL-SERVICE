# MACROFASE 12 — Production Database Baseline generator
# Run from repository root. This script does not reset Supabase. It only manages local EF migration files when explicitly requested.

param(
    [switch]$ApplyLocalMigrationReset,
    [string]$MigrationName = "InitialProductionBaseline"
)

$ErrorActionPreference = "Stop"

Write-Host "MACROFASE 12 production database baseline generator" -ForegroundColor Cyan
Write-Host "MigrationName: $MigrationName" -ForegroundColor Cyan

$required = @(
    "Pos.sln",
    "PosServer/PosServer.csproj",
    "PosInfrastructure/PosInfrastructure.csproj",
    "PosInfrastructure/Data/Server/CentralDbContext.cs",
    "scripts/database/Reset-Supabase-PublicSchema.sql"
)

foreach ($path in $required) {
    if (-not (Test-Path $path)) {
        throw "Missing required path: $path"
    }
}

$contextSource = Get-Content "PosInfrastructure/Data/Server/CentralDbContext.cs" -Raw
$markers = @(
    "ApplyProductionDatabaseBaselineHardening",
    "ConfigureTenantScopedEntity",
    "HasPrecision(18, 2)",
    "HasPrecision(18, 3)",
    "InventoryMovement can reference either ProductId or SupplyId"
)

foreach ($marker in $markers) {
    if (-not $contextSource.Contains($marker)) {
        throw "Missing model hardening marker: $marker"
    }
}

Write-Host "Model hardening markers verified." -ForegroundColor Green

if (-not $ApplyLocalMigrationReset) {
    Write-Host "Dry run only. Re-run with -ApplyLocalMigrationReset to backup and remove old migration files, then generate $MigrationName." -ForegroundColor Yellow
    Write-Host "No files were deleted." -ForegroundColor Yellow
    exit 0
}

$migrationsDir = "PosInfrastructure/Migrations"
if (-not (Test-Path $migrationsDir)) {
    New-Item -ItemType Directory -Path $migrationsDir | Out-Null
}

$timestamp = Get-Date -Format "yyyyMMddHHmmss"
$backupDir = "PosInfrastructure/Migrations_Backup_PreMacro12_$timestamp"

if (Test-Path $migrationsDir) {
    Copy-Item $migrationsDir $backupDir -Recurse -Force
    Write-Host "Migration backup created: $backupDir" -ForegroundColor Green
}

Get-ChildItem $migrationsDir -Filter "*.cs" -ErrorAction SilentlyContinue | Remove-Item -Force
Write-Host "Old migration .cs files removed from $migrationsDir" -ForegroundColor Yellow

Write-Host "Generating EF migration: $MigrationName" -ForegroundColor Cyan

dotnet ef migrations add $MigrationName --project PosInfrastructure --startup-project PosServer --context CentralDbContext --output-dir Migrations

Write-Host "Validating Release build after baseline generation..." -ForegroundColor Cyan
dotnet build -c Release Pos.sln

Write-Host "MACROFASE 12 InitialProductionBaseline generated and build verified." -ForegroundColor Green
Write-Host "Next: run scripts/database/Reset-Supabase-PublicSchema.sql intentionally in Supabase SQL Editor, then push and redeploy Railway." -ForegroundColor Yellow
