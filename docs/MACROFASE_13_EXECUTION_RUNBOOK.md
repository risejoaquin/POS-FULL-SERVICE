# MACROFASE 13 Execution Runbook

## Step 1 - Apply patch

Apply this patch over the current repository root.

## Step 2 - Verify markers

```powershell
.\VERIFY_MACROFASE_13_API_PRODUCTION_VALIDATION.ps1
```

## Step 3 - Run local test suite

```powershell
dotnet test
```

Expected total from latest validated baseline:

```text
643 passed
0 failed
```

## Step 4 - Run Release build

```powershell
dotnet build -c Release Pos.sln
```

Expected:

```text
0 warnings
0 errors
```

## Step 5 - Validate production API

```powershell
.\scripts\production\Validate-Macrofase13-ApiProductionValidation.ps1 -BaseUrl "https://pos-full-service-production.up.railway.app"
```

## Step 6 - Commit closure artifacts

```powershell
git status
git add .
git commit -m "Add API production validation baseline"
git push
```

## Step 7 - Report results

Paste:

- Verifier output.
- dotnet test summary.
- dotnet build summary.
- Production validation script output.
