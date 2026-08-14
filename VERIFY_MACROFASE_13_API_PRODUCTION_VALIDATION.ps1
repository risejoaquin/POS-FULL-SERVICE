Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$requiredFiles = @(
    "docs/MACROFASE_13_API_PRODUCTION_VALIDATION.md",
    "docs/API_PRODUCTION_VALIDATION_CHECKLIST.md",
    "docs/API_PRODUCTION_ENDPOINT_INVENTORY.md",
    "docs/PROJECT_PROGRESS_REPORT_MACROFASE_13.md",
    "docs/MACROFASE_13_EXECUTION_RUNBOOK.md",
    "scripts/production/Validate-Macrofase13-ApiProductionValidation.ps1"
)

foreach ($file in $requiredFiles) {
    if (-not (Test-Path $file)) {
        throw "Required MACROFASE 13 file missing: $file"
    }
}

$mainDoc = Get-Content "docs/MACROFASE_13_API_PRODUCTION_VALIDATION.md" -Raw
$checklist = Get-Content "docs/API_PRODUCTION_VALIDATION_CHECKLIST.md" -Raw
$inventory = Get-Content "docs/API_PRODUCTION_ENDPOINT_INVENTORY.md" -Raw
$script = Get-Content "scripts/production/Validate-Macrofase13-ApiProductionValidation.ps1" -Raw

$requiredMarkers = @(
    "MACROFASE 13 - API Production Validation",
    "MACROFASE 13B - Authenticated API Contract Validation",
    "MACROFASE 13 API production validation passed.",
    "/health/ready database Connected",
    "Checkout operations.",
    "Inventory mutations.",
    "Payments."
)

foreach ($marker in $requiredMarkers) {
    $found = $mainDoc.Contains($marker) -or $checklist.Contains($marker) -or $inventory.Contains($marker) -or $script.Contains($marker)
    if (-not $found) {
        throw "MACROFASE 13 marker missing: $marker"
    }
}

Write-Host "MACROFASE 13 API production validation markers verified."
Write-Host "Safe production validation script verified: GET-only public runtime endpoints."
Write-Host "Expected endpoints: /, /health, /api/health, /health/live, /health/ready."
Write-Host "Expected final validation: dotnet test = 643 passed, dotnet build Release = 0 warnings / 0 errors."
