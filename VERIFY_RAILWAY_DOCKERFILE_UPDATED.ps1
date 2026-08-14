$ErrorActionPreference = 'Stop'

$requiredFiles = @(
    'Dockerfile',
    '.dockerignore',
    'RAILWAY_DEPLOYMENT.md',
    'PosServer/PosServer.csproj',
    'PosDomain/PosDomain.csproj',
    'PosApplication/PosApplication.csproj',
    'PosInfrastructure/PosInfrastructure.csproj'
)

foreach ($file in $requiredFiles) {
    if (-not (Test-Path $file)) {
        throw "Missing required Railway deployment file: $file"
    }
}

$dockerfile = Get-Content 'Dockerfile' -Raw
$requiredDockerMarkers = @(
    'mcr.microsoft.com/dotnet/sdk:8.0',
    'mcr.microsoft.com/dotnet/aspnet:8.0',
    'dotnet restore PosServer/PosServer.csproj',
    'dotnet publish PosServer/PosServer.csproj',
    'PosServer.dll',
    '${PORT:-8080}'
)

foreach ($marker in $requiredDockerMarkers) {
    if ($dockerfile -notlike "*$marker*") {
        throw "Dockerfile marker missing: $marker"
    }
}

$dockerignore = Get-Content '.dockerignore' -Raw
$requiredIgnoreMarkers = @(
    '**/bin/',
    '**/obj/',
    'artifacts/',
    '.env',
    'node_modules/'
)

foreach ($marker in $requiredIgnoreMarkers) {
    if ($dockerignore -notlike "*$marker*") {
        throw ".dockerignore marker missing: $marker"
    }
}

Write-Host 'RAILWAY Dockerfile hotfix markers verified.'
Write-Host 'Expected Railway deployment: Dockerfile found at repository root.'
Write-Host 'Expected local Docker command: docker build -t posserver-railway-test .'
