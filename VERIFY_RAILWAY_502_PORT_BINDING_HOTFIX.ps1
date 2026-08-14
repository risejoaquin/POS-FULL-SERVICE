$ErrorActionPreference = "Stop"

$requiredFiles = @(
    "Dockerfile",
    "PosServer/Dockerfile",
    "railway.json",
    "scripts/railway/start-posserver.sh",
    "docs/RAILWAY_502_PORT_BINDING_HOTFIX.md"
)

foreach ($file in $requiredFiles) {
    if (-not (Test-Path $file)) {
        throw "Missing required file: $file"
    }
}

$rootDockerfile = Get-Content "Dockerfile" -Raw
$serverDockerfile = Get-Content "PosServer/Dockerfile" -Raw
$startScript = Get-Content "scripts/railway/start-posserver.sh" -Raw
$railwayJson = Get-Content "railway.json" -Raw
$doc = Get-Content "docs/RAILWAY_502_PORT_BINDING_HOTFIX.md" -Raw

$forbidden = 'ENV ASPNETCORE_URLS=http://+:${PORT}'
if ($rootDockerfile.Contains($forbidden)) {
    throw "Root Dockerfile still contains build-time PORT expansion."
}
if ($serverDockerfile.Contains($forbidden)) {
    throw "PosServer/Dockerfile still contains build-time PORT expansion."
}

$markers = @(
    'COPY scripts/railway/start-posserver.sh /app/start-posserver.sh',
    'ENTRYPOINT ["/app/start-posserver.sh"]',
    'ASPNETCORE_URLS="http://0.0.0.0:${PORT}"',
    'RAILWAY RUNTIME PORT BINDING START',
    '"startCommand": "/app/start-posserver.sh"',
    'Railway 502 Port Binding Hotfix',
    'RAILWAY 502 verifier syntax hotfix V2',
    'RAILWAY 502 verifier forbidden-string hotfix V3'
)

$combined = $rootDockerfile + "`n" + $serverDockerfile + "`n" + $startScript + "`n" + $railwayJson + "`n" + $doc
foreach ($marker in $markers) {
    if (-not $combined.Contains($marker)) {
        throw "Missing Railway 502 port binding hotfix marker: $marker"
    }
}

Write-Host "RAILWAY 502 port binding hotfix verified."
Write-Host "Dockerfiles no longer expand PORT at build time."
Write-Host "Runtime startup script will bind ASPNETCORE_URLS to Railway PORT."
