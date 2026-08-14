$ErrorActionPreference = "Stop"

$requiredFiles = @(
    "docs/MACROFASE_12_DATABASE_AUDIT.md",
    "docs/MACROFASE_12_PRODUCTION_DATABASE_BASELINE_PLAN.md",
    "docs/ENVIRONMENT_VARIABLES_PRODUCTION.md",
    "scripts/database/Reset-Supabase-PublicSchema.sql",
    "scripts/database/Run-Macrofase12-LocalBaselineCommands.ps1",
    "railway.json",
    "PosServer/Dockerfile",
    "PosInfrastructure/Data/Server/CentralDbContext.cs",
    "PosInfrastructure/Migrations/20260810230421_InitialServer.cs"
)

foreach ($file in $requiredFiles) {
    if (-not (Test-Path $file)) {
        throw "Missing MACROFASE 12 audit artifact: $file"
    }
}

$audit = Get-Content "docs/MACROFASE_12_DATABASE_AUDIT.md" -Raw
$requiredMarkers = @(
    '42P07: relation "CashRegisterShifts" already exists',
    'InitialProductionBaseline',
    'expone 17 conjuntos persistentes',
    'RESET CONTROLADO DEL ESQUEMA',
    'JWT_KEY',
    'JWT_ISSUER',
    'JWT_AUDIENCE',
    'numeric(18,2)',
    'InventoryMovements: TenantId + ProductId + MovementDate'
)
foreach ($marker in $requiredMarkers) {
    if ($audit -notlike "*$marker*") {
        throw "Missing audit marker: $marker"
    }
}

Write-Host "MACROFASE 12 database audit markers verified."
Write-Host "Audit deliverables are present."
Write-Host "Next step: harden CentralDbContext model, reset migrations, create InitialProductionBaseline."
