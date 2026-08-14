# Railway Deployment — PosServer Dockerfile Context Hotfix

## Current failure fixed by this package

Railway is finding the Dockerfile, but the build context is wrong:

```text
load build definition from PosServer/Dockerfile
failed to calculate checksum: "/PosServer/PosServer.csproj": not found
```

This means the Dockerfile path is now correct, but Railway is building from `/PosServer` instead of the repository root.

## Required Railway settings

Use this exact configuration:

```text
Root Directory: empty or /
Dockerfile Path: PosServer/Dockerfile
```

Do not use:

```text
Root Directory: /PosServer
```

`PosServer` is not standalone. It has project references to sibling projects:

```text
../PosApplication/PosApplication.csproj
../PosInfrastructure/PosInfrastructure.csproj
```

Those projects require repository-root build context.

## Files included

```text
railway.json
Dockerfile
PosServer/Dockerfile
.dockerignore
VERIFY_RAILWAY_BUILD_CONTEXT_UPDATED.ps1
```

The root `Dockerfile` and `PosServer/Dockerfile` both publish only `PosServer` and avoid publishing Windows desktop projects.

## Expected build progression after fixing Root Directory

The Railway build should progress past:

```text
COPY PosServer/PosServer.csproj PosServer/
```

and continue to:

```text
dotnet restore PosServer/PosServer.csproj
dotnet publish PosServer/PosServer.csproj
```

## Local Docker validation

If Docker is installed locally, run from repository root:

```powershell
docker build -f PosServer/Dockerfile -t posserver-railway-test .
docker run --rm -p 8080:8080 -e PORT=8080 posserver-railway-test
```

## Guardrails

This hotfix only changes deployment packaging and Railway configuration files.

It does not change:

```text
business logic
checkout behavior
inventory mutation behavior
public API contracts
database schema
migrations
Railway secrets
Supabase data
production data
```
