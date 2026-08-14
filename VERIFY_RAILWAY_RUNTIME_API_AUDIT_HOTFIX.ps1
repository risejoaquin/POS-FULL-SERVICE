$ErrorActionPreference = "Stop"

$requiredFiles = @(
    "PosServer/Program.cs",
    "PosServer/Middlewares/TenantMiddleware.cs",
    "scripts/railway/start-posserver.sh",
    "Dockerfile",
    "PosServer/Dockerfile",
    "railway.json",
    "docs/RAILWAY_RUNTIME_API_AUDIT.md"
)

foreach ($file in $requiredFiles) {
    if (-not (Test-Path $file)) {
        throw "Missing required file: $file"
    }
}

$program = Get-Content "PosServer/Program.cs" -Raw
$tenant = Get-Content "PosServer/Middlewares/TenantMiddleware.cs" -Raw
$startScript = Get-Content "scripts/railway/start-posserver.sh" -Raw
$rootDockerfile = Get-Content "Dockerfile" -Raw
$serverDockerfile = Get-Content "PosServer/Dockerfile" -Raw
$railway = Get-Content "railway.json" -Raw
$doc = Get-Content "docs/RAILWAY_RUNTIME_API_AUDIT.md" -Raw

$programMarkers = @(
    'app.Run();',
    'POS Server runtime audit: startup completed; entering app.Run().',
    'app.MapGet("/health"',
    'app.MapGet("/api/health"',
    'UseForwardedHeaders',
    'Railway runtime detected; HTTPS redirection is skipped'
)
foreach ($marker in $programMarkers) {
    if (-not $program.Contains($marker)) {
        throw "Missing Program.cs marker: $marker"
    }
}

$tenantMarkers = @(
    'path == "/health"',
    'path == "/api/health"',
    'path.StartsWith("/health/")',
    'path == "/metrics"'
)
foreach ($marker in $tenantMarkers) {
    if (-not $tenant.Contains($marker)) {
        throw "Missing TenantMiddleware marker: $marker"
    }
}

$scriptMarkers = @(
    'RAILWAY RUNTIME PORT BINDING START',
    'ASPNETCORE_URLS="http://0.0.0.0:${PORT}"',
    'exec dotnet PosServer.dll'
)
foreach ($marker in $scriptMarkers) {
    if (-not $startScript.Contains($marker)) {
        throw "Missing start script marker: $marker"
    }
}

if ($rootDockerfile.Contains('ENV ASPNETCORE_URLS=http://+:${PORT}')) {
    throw "Root Dockerfile still contains build-time PORT expansion."
}
if ($serverDockerfile.Contains('ENV ASPNETCORE_URLS=http://+:${PORT}')) {
    throw "PosServer Dockerfile still contains build-time PORT expansion."
}
if (-not $rootDockerfile.Contains('ENTRYPOINT ["/app/start-posserver.sh"]')) {
    throw "Root Dockerfile does not use runtime start script."
}
if (-not $serverDockerfile.Contains('ENTRYPOINT ["/app/start-posserver.sh"]')) {
    throw "PosServer Dockerfile does not use runtime start script."
}
if (-not $railway.Contains('"startCommand": "/app/start-posserver.sh"')) {
    throw "railway.json does not use runtime start script."
}
if (-not $doc.Contains('faltaba `app.Run()`')) {
    throw "Runtime audit document is missing app.Run diagnosis."
}

Write-Host "RAILWAY runtime API audit hotfix verified."
Write-Host "Expected public endpoints after redeploy: /, /health, /api/health, /health/live, /health/ready."
Write-Host "Expected deploy log marker: POS Server runtime audit: startup completed; entering app.Run()."
