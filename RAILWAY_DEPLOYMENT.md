# Railway Deployment — PosServer

## Current deployment mode

This repository uses Railway config-as-code.

```text
railway.json at repo root
Dockerfile: PosServer/Dockerfile
Build context: repository root
Runtime entrypoint: dotnet PosServer.dll
```

## Railway UI settings

In Railway → Settings → Source:

```text
Root Directory: empty
Branch: main
```

Do not set Root Directory to `/PosServer`.
Do not type `Dockerfile Path: PosServer/Dockerfile` into Root Directory.

## Why

The Dockerfile is inside `PosServer`, but the build context must be the repository root because `PosServer` depends on:

```text
PosDomain
PosApplication
PosInfrastructure
```

## Expected Railway build

```text
load build definition from PosServer/Dockerfile
COPY PosDomain/PosDomain.csproj PosDomain/
COPY PosApplication/PosApplication.csproj PosApplication/
COPY PosInfrastructure/PosInfrastructure.csproj PosInfrastructure/
COPY PosServer/PosServer.csproj PosServer/
RUN dotnet restore PosServer/PosServer.csproj
RUN dotnet publish PosServer/PosServer.csproj
```

## Local verification

```powershell
.\VERIFY_RAILWAY_OPTION_A_UPDATED.ps1
```

Optional local Docker verification if Docker is installed:

```powershell
docker build -f PosServer/Dockerfile -t posserver-railway-test .
docker run --rm -p 8080:8080 -e PORT=8080 posserver-railway-test
```
