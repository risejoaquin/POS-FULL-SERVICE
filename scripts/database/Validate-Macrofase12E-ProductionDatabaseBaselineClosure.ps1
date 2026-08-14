param(
    [string]$BaseUrl = ""
)

$ErrorActionPreference = "Stop"
$repo = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path

Write-Host "MACROFASE 12E production database baseline closure validation."
Write-Host "RepositoryRoot: $repo"

$requiredFiles = @(
    "docs/MACROFASE_12E_PRODUCTION_DATABASE_BASELINE_CLOSURE.md",
    "docs/PRODUCTION_DATABASE_BASELINE_CLOSURE_REPORT.md",
    "docs/RAILWAY_DEPLOYMENT_VALIDATION_EVIDENCE.md",
    "docs/ENVIRONMENT_VARIABLES_RAILWAY_PRODUCTION_BASELINE.md",
    "docs/PROJECT_PROGRESS_REPORT_MACROFASE_12E.md"
)

foreach ($file in $requiredFiles) {
    $path = Join-Path $repo $file
    if (-not (Test-Path $path)) {
        throw "Missing required MACROFASE 12E file: $file"
    }
}

$migrationsDir = Join-Path $repo "PosInfrastructure\Migrations"
if (-not (Test-Path $migrationsDir)) {
    throw "Missing migrations directory: PosInfrastructure/Migrations"
}

$baselineFiles = Get-ChildItem $migrationsDir -Filter "*InitialProductionBaseline*.cs" -ErrorAction SilentlyContinue
if (-not $baselineFiles -or $baselineFiles.Count -eq 0) {
    throw "InitialProductionBaseline migration file was not found in PosInfrastructure/Migrations."
}

$programCs = Join-Path $repo "PosServer\Program.cs"
if (Test-Path $programCs) {
    $programText = Get-Content $programCs -Raw
    foreach ($marker in @('app.Run()', 'POS-FULL-SERVICE API', '/health', '/api/health')) {
        if (-not $programText.Contains($marker)) {
            throw "Missing Program.cs runtime endpoint marker: $marker"
        }
    }
}

if ($BaseUrl -and $BaseUrl.Trim().Length -gt 0) {
    $cleanBaseUrl = $BaseUrl.TrimEnd('/')
    $endpoints = @('/', '/health', '/api/health', '/health/live', '/health/ready')
    foreach ($endpoint in $endpoints) {
        $url = "$cleanBaseUrl$endpoint"
        Write-Host "Checking $url"
        $response = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 30
        if ($response.StatusCode -ne 200) {
            throw "Endpoint $endpoint returned HTTP $($response.StatusCode)."
        }
    }
}

Write-Host "MACROFASE 12E production database baseline closure verified."
Write-Host "InitialProductionBaseline: present"
Write-Host "Railway endpoints expected: /, /health, /api/health, /health/live, /health/ready"
Write-Host "Next step: dotnet test, dotnet build, then close MACROFASE 12 FINAL."
