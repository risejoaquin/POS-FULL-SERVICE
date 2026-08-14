# MACROFASE 12 — Local baseline command helper
# Run from repository root.
# This script does not delete Supabase data. The schema reset SQL is separate and must be executed intentionally in Supabase.

$ErrorActionPreference = "Stop"

Write-Host "MACROFASE 12 local baseline command helper" -ForegroundColor Cyan
Write-Host "Expected repository root contains Pos.sln, PosServer, PosInfrastructure." -ForegroundColor Cyan

$required = @(
    "Pos.sln",
    "PosServer/PosServer.csproj",
    "PosInfrastructure/PosInfrastructure.csproj",
    "PosInfrastructure/Data/Server/CentralDbContext.cs"
)

foreach ($path in $required) {
    if (-not (Test-Path $path)) {
        throw "Missing required path: $path"
    }
}

Write-Host "Required repository structure verified." -ForegroundColor Green
Write-Host "Recommended EF migration reset commands:" -ForegroundColor Yellow
Write-Host ""
Write-Host "# 1. Backup current migrations" -ForegroundColor Gray
Write-Host "Copy-Item PosInfrastructure/Migrations PosInfrastructure/Migrations_Backup_PreMacro12 -Recurse" -ForegroundColor White
Write-Host ""
Write-Host "# 2. Remove old migration files after model hardening" -ForegroundColor Gray
Write-Host "Remove-Item PosInfrastructure/Migrations/*.cs" -ForegroundColor White
Write-Host ""
Write-Host "# 3. Generate production baseline" -ForegroundColor Gray
Write-Host "dotnet ef migrations add InitialProductionBaseline --project PosInfrastructure --startup-project PosServer --context CentralDbContext --output-dir Migrations" -ForegroundColor White
Write-Host ""
Write-Host "# 4. Validate build" -ForegroundColor Gray
Write-Host "dotnet build -c Release Pos.sln" -ForegroundColor White
Write-Host ""
Write-Host "Do not run schema reset from this script. Use scripts/database/Reset-Supabase-PublicSchema.sql intentionally in Supabase SQL Editor." -ForegroundColor Yellow
