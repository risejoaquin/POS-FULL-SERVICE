# API Production Validation Checklist

Status: PENDING

## Pre-checks

- MACROFASE 12A closed.
- MACROFASE 12B closed.
- MACROFASE 12C closed.
- MACROFASE 12D closed.
- MACROFASE 12E closed.
- Railway production URL is known.
- Supabase schema baseline is aligned with InitialProductionBaseline.

## Local validation

Run from repository root:

```powershell
.\VERIFY_MACROFASE_13_API_PRODUCTION_VALIDATION.ps1

dotnet test

dotnet build -c Release Pos.sln
```

Expected local result:

```text
MACROFASE 13 API production validation markers verified.
Expected final validation: dotnet test = 643 passed, dotnet build Release = 0 warnings / 0 errors.
```

## Production validation

Run:

```powershell
.\scripts\production\Validate-Macrofase13-ApiProductionValidation.ps1 -BaseUrl "https://pos-full-service-production.up.railway.app"
```

Expected production result:

```text
/ -> 200
/health -> 200
/api/health -> 200
/health/live -> 200
/health/ready -> 200
/health/ready database Connected
MACROFASE 13 API production validation passed.
```

## Failure handling

If an endpoint returns 502:

- Check Railway deploy logs.
- Confirm runtime port binding log appears.
- Confirm app.Run startup marker appears.

If an endpoint returns 404:

- Confirm route mapping in PosServer/Program.cs.
- Confirm controller routes for HealthController.

If /health/ready fails:

- Check Supabase connection string.
- Check EF migration state.
- Check database availability.

If /favicon.ico returns 400:

- Ignore for this macrofase. Browser favicon requests are not part of API health validation.
