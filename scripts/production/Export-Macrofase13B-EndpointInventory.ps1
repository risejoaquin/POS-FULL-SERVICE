param(
    [string]$ProjectRoot = (Get-Location).Path,
    [string]$OutputDirectory = "artifacts/macro13b"
)

$ErrorActionPreference = "Stop"

function Join-RootPath([string]$relativePath) {
    return Join-Path $ProjectRoot $relativePath
}

Write-Host "MACROFASE 13B endpoint inventory export started."
Write-Host "ProjectRoot: $ProjectRoot"

$controllersPath = Join-RootPath "PosServer/Controllers"
$programPath = Join-RootPath "PosServer/Program.cs"

if (-not (Test-Path $controllersPath)) {
    throw "Missing PosServer/Controllers directory."
}
if (-not (Test-Path $programPath)) {
    throw "Missing PosServer/Program.cs."
}

$requiredFiles = @(
    "AuthController.cs",
    "HealthController.cs",
    "InventoryMovementsController.cs",
    "LicenseController.cs",
    "OrdersController.cs",
    "ProductsController.cs",
    "ShiftsController.cs",
    "SyncController.cs",
    "UsersController.cs"
)

foreach ($file in $requiredFiles) {
    $fullPath = Join-Path $controllersPath $file
    if (-not (Test-Path $fullPath)) {
        throw "Missing controller: $file"
    }
}

$inventory = @(
    [pscustomobject]@{ Method="GET"; Route="/"; Source="Program.cs"; Auth="Anonymous"; MutatesData=$false; Risk="P0"; Notes="Service identity endpoint" },
    [pscustomobject]@{ Method="GET"; Route="/health"; Source="Program.cs"; Auth="Anonymous"; MutatesData=$false; Risk="P0"; Notes="Public health alias" },
    [pscustomobject]@{ Method="GET"; Route="/api/health"; Source="Program.cs"; Auth="Anonymous"; MutatesData=$false; Risk="P0"; Notes="Public API health alias" },
    [pscustomobject]@{ Method="GET"; Route="/health/live"; Source="HealthController.cs"; Auth="Anonymous"; MutatesData=$false; Risk="P0"; Notes="Liveness" },
    [pscustomobject]@{ Method="GET"; Route="/health/ready"; Source="HealthController.cs"; Auth="Anonymous"; MutatesData=$false; Risk="P0"; Notes="Readiness with database check" },
    [pscustomobject]@{ Method="GET"; Route="/health/metrics"; Source="HealthController.cs"; Auth="ReviewRequired"; MutatesData=$false; Risk="P2"; Notes="Operational metrics exposure requires decision" },
    [pscustomobject]@{ Method="GET"; Route="/metrics"; Source="HealthController.cs"; Auth="ReviewRequired"; MutatesData=$false; Risk="P2"; Notes="Absolute metrics route exposure requires decision" },

    [pscustomobject]@{ Method="POST"; Route="/api/v1/auth/login"; Source="AuthController.cs"; Auth="Anonymous"; MutatesData=$false; Risk="P1"; Notes="LoginPolicy rate limit" },
    [pscustomobject]@{ Method="POST"; Route="/api/v1/auth/refresh"; Source="AuthController.cs"; Auth="Anonymous"; MutatesData=$false; Risk="P1"; Notes="LoginPolicy rate limit" },
    [pscustomobject]@{ Method="POST"; Route="/api/v1/auth/provision"; Source="AuthController.cs"; Auth="ServiceGuardRequired"; MutatesData=$true; Risk="P1"; Notes="Must be protected by provision guard" },

    [pscustomobject]@{ Method="POST"; Route="/api/v1/license/validate"; Source="LicenseController.cs"; Auth="Anonymous"; MutatesData=$false; Risk="P1"; Notes="License validation" },
    [pscustomobject]@{ Method="POST"; Route="/api/v1/license/generate"; Source="LicenseController.cs"; Auth="Admin"; MutatesData=$true; Risk="P1"; Notes="License generation" },

    [pscustomobject]@{ Method="GET"; Route="/api/v1/products"; Source="ProductsController.cs"; Auth="Authenticated"; MutatesData=$false; Risk="P1"; Notes="Paged products" },
    [pscustomobject]@{ Method="GET"; Route="/api/v1/products/changes"; Source="ProductsController.cs"; Auth="Authenticated"; MutatesData=$false; Risk="P1"; Notes="Product sync changes" },
    [pscustomobject]@{ Method="POST"; Route="/api/v1/products"; Source="ProductsController.cs"; Auth="Admin"; MutatesData=$true; Risk="P1"; Notes="Create/update product" },
    [pscustomobject]@{ Method="DELETE"; Route="/api/v1/products/{barcode}"; Source="ProductsController.cs"; Auth="Admin"; MutatesData=$true; Risk="P1"; Notes="Delete product" },

    [pscustomobject]@{ Method="POST"; Route="/api/v1/orders"; Source="OrdersController.cs"; Auth="Authenticated"; MutatesData=$true; Risk="P1"; Notes="Create/update order" },
    [pscustomobject]@{ Method="GET"; Route="/api/v1/orders"; Source="OrdersController.cs"; Auth="Authenticated"; MutatesData=$false; Risk="P1"; Notes="Paged orders" },
    [pscustomobject]@{ Method="GET"; Route="/api/v1/orders/{id}"; Source="OrdersController.cs"; Auth="Authenticated"; MutatesData=$false; Risk="P1"; Notes="Order by id" },

    [pscustomobject]@{ Method="POST"; Route="/api/v1/inventorymovements"; Source="InventoryMovementsController.cs"; Auth="Authenticated"; MutatesData=$true; Risk="P1"; Notes="Sync inventory movement" },
    [pscustomobject]@{ Method="POST"; Route="/api/v1/shifts"; Source="ShiftsController.cs"; Auth="Authenticated"; MutatesData=$true; Risk="P1"; Notes="Sync shift" },

    [pscustomobject]@{ Method="GET"; Route="/api/v1/sync/changes"; Source="SyncController.cs"; Auth="Authenticated"; MutatesData=$false; Risk="P1"; Notes="Sync changes" },
    [pscustomobject]@{ Method="POST"; Route="/api/v1/sync/apply"; Source="SyncController.cs"; Auth="Authenticated"; MutatesData=$true; Risk="P1"; Notes="Apply sync payload" },
    [pscustomobject]@{ Method="POST"; Route="/api/v1/sync/ping"; Source="SyncController.cs"; Auth="Authenticated"; MutatesData=$false; Risk="P2"; Notes="Heartbeat logging" },

    [pscustomobject]@{ Method="POST"; Route="/api/v1/users"; Source="UsersController.cs"; Auth="Admin"; MutatesData=$true; Risk="P1"; Notes="Create/update user" },
    [pscustomobject]@{ Method="DELETE"; Route="/api/v1/users/{username}"; Source="UsersController.cs"; Auth="Admin"; MutatesData=$true; Risk="P1"; Notes="Delete user" }
)

$outDir = Join-RootPath $OutputDirectory
New-Item -ItemType Directory -Force -Path $outDir | Out-Null
$csvPath = Join-Path $outDir "macrofase13b-endpoint-inventory.csv"
$jsonPath = Join-Path $outDir "macrofase13b-endpoint-inventory.json"

$inventory | Export-Csv -Path $csvPath -NoTypeInformation -Encoding UTF8
$inventory | ConvertTo-Json -Depth 5 | Set-Content -Path $jsonPath -Encoding UTF8

Write-Host "MACROFASE 13B endpoint inventory exported."
Write-Host "CSV: $csvPath"
Write-Host "JSON: $jsonPath"
Write-Host "Endpoint count: $($inventory.Count)"
