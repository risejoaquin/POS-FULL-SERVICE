# Railway Option A — Config as Code Deployment

This hotfix configures Railway from the repository itself using `railway.json`.

## Required Railway UI setting

In Railway → Settings → Source:

```text
Root Directory: empty
```

Do not write any of the following in Root Directory:

```text
/PosServer
Dockerfile Path: PosServer/Dockerfile
Root Directory:
/ Dockerfile Path: PosServer/Dockerfile
```

## How Railway should resolve the build

`railway.json` must be in the repository root and contains:

```json
{
  "build": {
    "builder": "DOCKERFILE",
    "dockerfilePath": "PosServer/Dockerfile"
  }
}
```

This allows the Dockerfile to live in `PosServer/Dockerfile` while the build context remains the repository root.

## Why Root Directory must stay empty

`PosServer` depends on sibling projects:

```text
PosDomain/
PosApplication/
PosInfrastructure/
```

If Root Directory is `/PosServer`, Docker cannot access those sibling folders and Railway will fail with missing project-file errors.

## Expected successful build progression

```text
load build definition from PosServer/Dockerfile
COPY PosDomain/PosDomain.csproj PosDomain/
COPY PosApplication/PosApplication.csproj PosApplication/
COPY PosInfrastructure/PosInfrastructure.csproj PosInfrastructure/
COPY PosServer/PosServer.csproj PosServer/
RUN dotnet restore PosServer/PosServer.csproj
RUN dotnet publish PosServer/PosServer.csproj
```
