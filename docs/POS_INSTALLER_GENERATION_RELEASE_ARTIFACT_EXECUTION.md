# POS Installer Generation and Release Artifact Execution

PHASE 9A documents controlled installer generation and release artifact execution.

Required evidence:

- installer generation and release artifact execution documented
- PHASE 8J go no-go operational readiness prerequisite documented
- 440 tests passed source evidence documented
- 445 tests expected after installer generation execution baseline documented
- dotnet publish PosCore artifact command documented
- dotnet publish PosBuilder artifact command documented
- dotnet publish PosServer artifact command documented
- release artifact output directory documented
- release manifest generation command documented
- SHA-256 checksum generation command documented
- release artifact execution script documented
- installer input artifact checklist documented
- setup package generation readiness documented
- installer output placeholder documented
- artifact verification after publish documented
- operator execution command documented
- release candidate artifact archive documented
- execution failure handling checklist documented

Execution command:

```powershell
.\scripts\release\Generate-Phase9ReleaseArtifacts.ps1 -Configuration Release -RuntimeIdentifier win-x64 -ReleaseVersion 0.9.0-rc.1 -ReleaseChannel release-candidate
```

Publish commands covered by the script:

```powershell
dotnet publish PosCore\PosCore.csproj -c Release -r win-x64 --self-contained false -o artifacts\release\phase9\publish\poscore-win-x64
dotnet publish PosBuilder\PosBuilder.csproj -c Release -r win-x64 --self-contained false -o artifacts\release\phase9\publish\posbuilder-win-x64
dotnet publish PosServer\PosServer.csproj -c Release --self-contained false -o artifacts\release\phase9\publish\posserver
```

Safety boundaries:

- no checkout behavior change
- no inventory mutation
- no production sync enablement
- no deployment execution
- no public API behavior change
- no schema change
- no migrations
