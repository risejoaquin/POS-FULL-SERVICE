$ErrorActionPreference = "Stop"

$requiredFiles = @(
  "railway.json",
  "Dockerfile",
  "PosServer/Dockerfile",
  ".dockerignore",
  "Pos.sln",
  "PosServer/PosServer.csproj",
  "PosApplication/PosApplication.csproj",
  "PosInfrastructure/PosInfrastructure.csproj"
)

foreach ($file in $requiredFiles) {
  if (-not (Test-Path $file)) {
    throw "Missing required Railway deployment file: $file. Run this verifier from repository root, not from PosServer."
  }
}

$dockerfile = Get-Content "PosServer/Dockerfile" -Raw
$requiredDockerMarkers = @(
  "COPY PosApplication/PosApplication.csproj PosApplication/",
  "COPY PosInfrastructure/PosInfrastructure.csproj PosInfrastructure/",
  "COPY PosServer/PosServer.csproj PosServer/",
  "dotnet restore PosServer/PosServer.csproj",
  "dotnet publish PosServer/PosServer.csproj",
  "ASPNETCORE_URLS=http://0.0.0.0"
)

foreach ($marker in $requiredDockerMarkers) {
  if ($dockerfile -notlike "*$marker*") {
    throw "Missing Dockerfile marker: $marker"
  }
}

$railwayJson = Get-Content "railway.json" -Raw
if ($railwayJson -notlike '*"builder": "DOCKERFILE"*') {
  throw "railway.json does not force DOCKERFILE builder."
}
if ($railwayJson -notlike '*"dockerfilePath": "PosServer/Dockerfile"*') {
  throw "railway.json does not point to PosServer/Dockerfile."
}

Write-Host "RAILWAY build context hotfix markers verified."
Write-Host "Expected Railway Root Directory: empty or /."
Write-Host "Expected Railway Dockerfile Path: PosServer/Dockerfile."
Write-Host "Do not use Root Directory: /PosServer because PosServer depends on sibling projects."
