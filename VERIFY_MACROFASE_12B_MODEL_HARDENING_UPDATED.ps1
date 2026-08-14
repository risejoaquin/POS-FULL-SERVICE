$ErrorActionPreference = "Stop"

$requiredPaths = @(
    "PosInfrastructure/Data/Server/CentralDbContext.cs",
    "docs/MACROFASE_12B_MODEL_HARDENING.md",
    "docs/DATABASE_BASELINE_MODEL_HARDENING_REPORT.md",
    "docs/MIGRATION_RESET_RUNBOOK.md",
    "docs/PROJECT_PROGRESS_REPORT_MACROFASE_12B.md",
    "scripts/database/Invoke-Macrofase12-ProductionDatabaseBaseline.ps1",
    "scripts/database/Reset-Supabase-PublicSchema.sql"
)

foreach ($path in $requiredPaths) {
    if (-not (Test-Path $path)) {
        throw "Missing required MACROFASE 12B path: $path"
    }
}

$context = Get-Content "PosInfrastructure/Data/Server/CentralDbContext.cs" -Raw
$doc = Get-Content "docs/MACROFASE_12B_MODEL_HARDENING.md" -Raw
$report = Get-Content "docs/DATABASE_BASELINE_MODEL_HARDENING_REPORT.md" -Raw
$runbook = Get-Content "docs/MIGRATION_RESET_RUNBOOK.md" -Raw
$script = Get-Content "scripts/database/Invoke-Macrofase12-ProductionDatabaseBaseline.ps1" -Raw

$markers = @(
    "ApplyProductionDatabaseBaselineHardening",
    "ConfigureTenantScopedEntity",
    "HasPrecision(18, 2)",
    "HasPrecision(18, 3)",
    "HasPrecision(18, 4)",
    "InventoryMovement can reference either ProductId or SupplyId",
    "InitialProductionBaseline",
    "MACROFASE 12B production database baseline hardening documented",
    "DATABASE BASELINE MODEL HARDENING REPORT",
    "Migration Reset Runbook",
    "ApplyLocalMigrationReset",
    "Reset-Supabase-PublicSchema.sql"
)

$combined = $context + "`n" + $doc + "`n" + $report + "`n" + $runbook + "`n" + $script

foreach ($marker in $markers) {
    if (-not $combined.Contains($marker)) {
        throw "Missing MACROFASE 12B marker: $marker"
    }
}

Write-Host "MACROFASE 12B model hardening markers verified."
Write-Host "Next step: run dotnet test, dotnet build, then generate InitialProductionBaseline in MACROFASE 12C."
