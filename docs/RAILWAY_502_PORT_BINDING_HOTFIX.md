# Railway 502 Port Binding Hotfix

## Problem

Railway build and deploy completed, EF Core migrations were up to date, but the public Railway URL returned `502 Bad Gateway`.

Build logs also showed:

```text
UndefinedVar: Usage of undefined variable '$PORT'
```

## Cause

The Dockerfile used a build-time ENV expression similar to:

```dockerfile
ENV ASPNETCORE_URLS=http://+:${PORT}
```

Railway injects `PORT` at runtime, not during Docker build. Docker therefore expands `${PORT}` too early, creating an invalid or empty binding. The container can start, but the application may not listen on the port Railway expects.

## Fix

Both Dockerfiles now delegate runtime binding to:

```text
scripts/railway/start-posserver.sh
```

The startup script reads Railway's runtime `PORT`, defaults to `8080` for local Docker runs, exports `ASPNETCORE_URLS`, and starts PosServer:

```sh
export ASPNETCORE_URLS="http://0.0.0.0:${PORT}"
exec dotnet PosServer.dll
```

## Expected deploy logs

```text
RAILWAY RUNTIME PORT BINDING START
PORT=<railway-port>
ASPNETCORE_URLS=http://0.0.0.0:<railway-port>
Starting PosServer...
```

Then the app should be reachable through the Railway public domain.

## Guardrails

- No business logic change.
- No database schema change.
- No migration change.
- No Supabase mutation.
- No checkout behavior change.
- No inventory behavior change.
- No public API contract change.


## Verifier Hotfix V2

RAILWAY 502 verifier syntax hotfix V2. PowerShell markers use single-quoted strings so paths like `/app/start-posserver.sh` and `${PORT}` are treated as literal text.


## RAILWAY 502 verifier forbidden-string hotfix V3

The Dockerfiles no longer contain the exact forbidden build-time PORT expansion string, even in comments. Runtime binding remains delegated to `scripts/railway/start-posserver.sh`.
