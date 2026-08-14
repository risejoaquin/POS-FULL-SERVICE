# Railway Deployment — PosServer Dockerfile

## Purpose

This repository now includes a root-level `Dockerfile` for Railway deployment of `PosServer`.

Railway previously failed with:

```text
couldn't locate the dockerfile at path Dockerfile in code archive
  - not found at PosServer/Dockerfile
  - not found at Dockerfile
```

The fix is to keep `Dockerfile` at the repository root, next to `Pos.sln`, because `PosServer` depends on sibling projects:

```text
PosDomain
PosApplication
PosInfrastructure
PosServer
```

## Railway settings

Recommended Railway configuration:

```text
Root Directory: /
Dockerfile Path: Dockerfile
```

If Railway auto-detects the Dockerfile at root, no custom Dockerfile path is required.

## Local validation

From the repository root:

```powershell
docker build -t posserver-railway-test .
docker run --rm -p 8080:8080 -e PORT=8080 posserver-railway-test
```

Then validate the API health endpoint, depending on the route implemented by `PosServer`:

```powershell
curl http://localhost:8080/health
```

or:

```powershell
curl http://localhost:8080/api/health
```

## Guardrails

This hotfix only adds deployment packaging files. It does not change:

```text
business logic
checkout behavior
inventory mutation behavior
public API contracts
database schema
migrations
Railway variables
Supabase data
production secrets
```

## Update: Dockerfile inside PosServer

If Railway is configured to look for the Dockerfile inside the API project folder, use:

```text
Root Directory: /
Dockerfile Path: PosServer/Dockerfile
```

The build context must remain the repository root because the API depends on sibling projects:

```text
PosDomain/
PosApplication/
PosInfrastructure/
PosServer/
```

Local Docker validation, if Docker is installed:

```powershell
docker build -f PosServer/Dockerfile -t posserver-railway-test .
docker run --rm -p 8080:8080 -e PORT=8080 posserver-railway-test
```
