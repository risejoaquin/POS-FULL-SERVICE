# Railway Deployment Configuration

## Required configuration

In Railway Settings -> Source:

```text
Root Directory: empty
Branch: main
```

Do not write Dockerfile path inside Root Directory.

## Dockerfile selection

This repository uses `railway.json` at repo root:

```json
{
  "build": {
    "builder": "DOCKERFILE",
    "dockerfilePath": "PosServer/Dockerfile"
  }
}
```

## Why Root Directory must be empty

`PosServer` depends on sibling projects:

```text
PosDomain
PosApplication
PosInfrastructure
```

If Root Directory is `/PosServer`, Docker cannot see those sibling projects and the build fails.

## Diagnostic build markers

The diagnostic Dockerfile prints:

```text
===== RAILWAY CONTEXT AUDIT START =====
Detected .csproj files up to depth 3
RAILWAY CONTEXT AUDIT PASS
===== RAILWAY CONTEXT AUDIT END =====
```

If Railway still fails with `snapshot-target-unpack/Root Directory:`, fix the Root Directory field first. Docker is not running yet in that failure mode.
