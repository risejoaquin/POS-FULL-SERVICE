$ErrorActionPreference = "Stop"

$requiredFiles = @(
  "railway.json",
  "Dockerfile",
  "PosServer/Dockerfile",
  ".dockerignore",
  "RAILWAY_BUILD_LOG_DIAGNOSTICS.md"
)

foreach ($file in $requiredFiles) {
  if (-not (Test-Path $file)) {
    throw "Missing required Railway diagnostic file: $file"
  }
}

$railwayJson = Get-Content "railway.json" -Raw
if ($railwayJson -notmatch '"builder"\s*:\s*"DOCKERFILE"') {
  throw "railway.json does not force DOCKERFILE builder."
}
if ($railwayJson -notmatch '"dockerfilePath"\s*:\s*"PosServer/Dockerfile"') {
  throw "railway.json does not point to PosServer/Dockerfile."
}

$posServerDockerfile = Get-Content "PosServer/Dockerfile" -Raw
$markers = @(
  "RAILWAY CONTEXT AUDIT START",
  "Detected .csproj files up to depth 3",
  "Missing PosServer/PosServer.csproj",
  "Missing PosDomain/PosDomain.csproj",
  "Missing PosApplication/PosApplication.csproj",
  "Missing PosInfrastructure/PosInfrastructure.csproj",
  "RAILWAY CONTEXT AUDIT PASS",
  "dotnet restore PosServer/PosServer.csproj",
  "dotnet publish PosServer/PosServer.csproj"
)
foreach ($marker in $markers) {
  if ($posServerDockerfile -notmatch [regex]::Escape($marker)) {
    throw "Missing diagnostic marker in PosServer/Dockerfile: $marker"
  }
}

if (-not (Test-Path "PosServer/PosServer.csproj")) {
  throw "Local verification failed: PosServer/PosServer.csproj is missing."
}
if (-not (Test-Path "PosDomain/PosDomain.csproj")) {
  throw "Local verification failed: PosDomain/PosDomain.csproj is missing."
}
if (-not (Test-Path "PosApplication/PosApplication.csproj")) {
  throw "Local verification failed: PosApplication/PosApplication.csproj is missing."
}
if (-not (Test-Path "PosInfrastructure/PosInfrastructure.csproj")) {
  throw "Local verification failed: PosInfrastructure/PosInfrastructure.csproj is missing."
}

Write-Host "RAILWAY diagnostic build logs hotfix markers verified."
Write-Host "Expected Railway Root Directory: EMPTY."
Write-Host "Expected Dockerfile path comes from railway.json: PosServer/Dockerfile."
Write-Host "Current Railway error means Root Directory still contains literal text such as 'Root Directory:'."
Write-Host "After Root Directory is cleaned, build logs will include RAILWAY CONTEXT AUDIT markers."
