$ErrorActionPreference = 'Stop'

$requiredFiles = @(
    'PosServer/Dockerfile',
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

$dockerfile = Get-Content 'PosServer/Dockerfile' -Raw
$requiredDockerMarkers = @(
    'Location: PosServer/Dockerfile',
    'Dockerfile Path: PosServer/Dockerfile',
    'mcr.microsoft.com/dotnet/sdk:8.0',
    'mcr.microsoft.com/dotnet/aspnet:8.0',
    'dotnet restore PosServer/PosServer.csproj',
    'dotnet publish PosServer/PosServer.csproj',
    'PosServer.dll',
    '${PORT:-8080}'
)

foreach ($marker in $requiredDockerMarkers) {
    if ($dockerfile -notlike "*$marker*") {
        throw "PosServer/Dockerfile marker missing: $marker"
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

Write-Host 'RAILWAY PosServer Dockerfile hotfix markers verified.'
Write-Host 'Expected Railway deployment: Dockerfile found at PosServer/Dockerfile.'
Write-Host 'Railway Root Directory: /'
Write-Host 'Railway Dockerfile Path: PosServer/Dockerfile'
Write-Host 'Expected local Docker command if Docker is installed: docker build -f PosServer/Dockerfile -t posserver-railway-test .'
