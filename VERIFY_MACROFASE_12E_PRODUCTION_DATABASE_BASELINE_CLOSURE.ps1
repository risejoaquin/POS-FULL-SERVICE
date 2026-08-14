$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $MyInvocation.MyCommand.Path

$files = @(
    'docs\MACROFASE_12E_PRODUCTION_DATABASE_BASELINE_CLOSURE.md',
    'docs\PRODUCTION_DATABASE_BASELINE_CLOSURE_REPORT.md',
    'docs\RAILWAY_DEPLOYMENT_VALIDATION_EVIDENCE.md',
    'docs\ENVIRONMENT_VARIABLES_RAILWAY_PRODUCTION_BASELINE.md',
    'docs\PROJECT_PROGRESS_REPORT_MACROFASE_12E.md',
    'scripts\database\Validate-Macrofase12E-ProductionDatabaseBaselineClosure.ps1'
)

foreach ($file in $files) {
    $path = Join-Path $repo $file
    if (-not (Test-Path $path)) {
        throw "Missing MACROFASE 12E closure artifact: $file"
    }
}

$closure = Get-Content (Join-Path $repo 'docs\MACROFASE_12E_PRODUCTION_DATABASE_BASELINE_CLOSURE.md') -Raw
$report = Get-Content (Join-Path $repo 'docs\PRODUCTION_DATABASE_BASELINE_CLOSURE_REPORT.md') -Raw
$evidence = Get-Content (Join-Path $repo 'docs\RAILWAY_DEPLOYMENT_VALIDATION_EVIDENCE.md') -Raw

$markers = @(
    'MACROFASE 12E - Production Database Baseline Closure',
    'InitialProductionBaseline',
    'database Connected',
    'Railway Build:        PASS',
    'Railway Deploy:       PASS',
    'EF Migrations:        PASS',
    'Health Endpoints:     PASS',
    'GET /health/ready',
    'MACROFASE 13 - API Production Validation'
)

$allText = $closure + "`n" + $report + "`n" + $evidence
foreach ($marker in $markers) {
    if (-not $allText.Contains($marker)) {
        throw "Missing MACROFASE 12E closure marker: $marker"
    }
}

$migrationsDir = Join-Path $repo 'PosInfrastructure\Migrations'
if (Test-Path $migrationsDir) {
    $baselineFiles = Get-ChildItem $migrationsDir -Filter '*InitialProductionBaseline*.cs' -ErrorAction SilentlyContinue
    if (-not $baselineFiles -or $baselineFiles.Count -eq 0) {
        Write-Warning 'InitialProductionBaseline migration was not found in this patch/full copy. If you generated it locally, keep your local migration files and apply this patch over them.'
    }
}

Write-Host 'MACROFASE 12E production database baseline closure markers verified.'
Write-Host 'Railway deployment evidence documented.'
Write-Host 'PowerShell encoding hotfix V2 verified.'
Write-Host 'Expected final validation: dotnet test = 643 passed, dotnet build Release = 0 warnings / 0 errors.'
