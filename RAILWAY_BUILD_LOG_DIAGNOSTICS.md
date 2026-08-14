# Railway Build Log Diagnostics Hotfix

## Current failure

The current Railway log fails before .NET compilation:

```text
fsutil.NewFS(.../snapshot-target-unpack/Root Directory:):
lstat .../snapshot-target-unpack/Root Directory:: no such file or directory
```

This means Railway is using the text `Root Directory:` as the Source Root Directory value. That is a UI/configuration error. Dockerfile diagnostics cannot execute until Root Directory is cleaned.

## Correct Railway Source settings

Use:

```text
Root Directory: empty
Branch: main
```

Do not put any of these in Root Directory:

```text
Root Directory:
/PosServer
Dockerfile Path: PosServer/Dockerfile
/Dockerfile Path: PosServer/Dockerfile
```

## Dockerfile path

The Dockerfile path is handled by `railway.json` at repository root:

```json
{
  "build": {
    "builder": "DOCKERFILE",
    "dockerfilePath": "PosServer/Dockerfile"
  }
}
```

## Better build logs added

`PosServer/Dockerfile` now includes a diagnostic context audit before `dotnet restore`:

```text
===== RAILWAY CONTEXT AUDIT START =====
Top-level files/folders visible to Docker:
Detected .csproj files up to depth 3:
Required files check:
RAILWAY CONTEXT AUDIT PASS: repo root context is visible.
===== RAILWAY CONTEXT AUDIT END =====
```

If Railway is still pointed to `/PosServer`, the Dockerfile will fail with a clearer message explaining that sibling projects are missing.

## Expected good build progression

```text
load build definition from PosServer/Dockerfile
COPY . .
===== RAILWAY CONTEXT AUDIT START =====
RAILWAY CONTEXT AUDIT PASS: repo root context is visible.
dotnet restore PosServer/PosServer.csproj
dotnet publish PosServer/PosServer.csproj
```

## Guardrails

This hotfix does not change POS business behavior:

```text
no checkout behavior change
no inventory mutation
no public API behavior change
no schema change
no migrations
no Supabase mutation
no Railway variables mutation
no secret disclosure
```
