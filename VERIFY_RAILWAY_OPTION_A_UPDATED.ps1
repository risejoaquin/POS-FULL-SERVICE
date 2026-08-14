$ErrorActionPreference = "Stop"

$requiredFiles = @(
  "railway.json",
  "PosServer/Dockerfile",
  ".dockerignore",
  "Pos.sln",
  "PosServer/PosServer.csproj",
  "PosDomain/PosDomain.csproj",
  "PosApplication/PosApplication.csproj",
  "PosInfrastructure/PosInfrastructure.csproj",
  "RAILWAY_OPTION_A_CONFIG_AS_CODE.md"
)

foreach ($file in $requiredFiles) {
  if (-not (Test-Path $file)) {
    throw "Missing required Railway Option A file: $file. Run this verifier from repository root."
  }
}

$railwayJson = Get-Content "railway.json" -Raw
$requiredRailwayMarkers = @(
  '"builder": "DOCKERFILE"',
  '"dockerfilePath": "PosServer/Dockerfile"',
  '"startCommand": "dotnet PosServer.dll"',
  '"restartPolicyType": "ON_FAILURE"'
)
foreach ($marker in $requiredRailwayMarkers) {
  if ($railwayJson -notlike "*$marker*") {
    throw "Missing railway.json marker: $marker"
  }
}

$dockerfile = Get-Content "PosServer/Dockerfile" -Raw
$requiredDockerMarkers = @(
  "COPY PosDomain/PosDomain.csproj PosDomain/",
  "COPY PosApplication/PosApplication.csproj PosApplication/",
  "COPY PosInfrastructure/PosInfrastructure.csproj PosInfrastructure/",
  "COPY PosServer/PosServer.csproj PosServer/",
  "dotnet restore PosServer/PosServer.csproj",
  "dotnet publish PosServer/PosServer.csproj",
  "ASPNETCORE_URLS=http://0.0.0.0"
)
foreach ($marker in $requiredDockerMarkers) {
  if ($dockerfile -notlike "*$marker*") {
    throw "Missing PosServer/Dockerfile marker: $marker"
  }
}

$dockerignore = Get-Content ".dockerignore" -Raw
$blockedBadMarkers = @(
  "PosServer/",
  "PosDomain/",
  "PosApplication/",
  "PosInfrastructure/"
)
foreach ($marker in $blockedBadMarkers) {
  if ($dockerignore -match "(?m)^$([regex]::Escape($marker))\s*$") {
    throw ".dockerignore incorrectly excludes required project folder: $marker"
  }
}

Write-Host "RAILWAY Option A config-as-code markers verified."
Write-Host "railway.json forces DOCKERFILE builder and PosServer/Dockerfile."
Write-Host "Expected Railway Root Directory: empty."
Write-Host "Do not put Dockerfile Path text inside Root Directory."
