# Railway Option A - Config as Code with Diagnostic Build Logs

This option uses `railway.json` at repository root to select Dockerfile deployment:

```text
builder: DOCKERFILE
dockerfilePath: PosServer/Dockerfile
```

Railway Source configuration must keep Root Directory empty.

This hotfix also improves Docker build logs by printing the visible build context and all detected `.csproj` files before `dotnet restore`.

Current known blocker:

```text
snapshot-target-unpack/Root Directory:
```

This is not a .NET build failure. It means the Railway Root Directory field contains invalid literal text. Clear the Root Directory field completely.
