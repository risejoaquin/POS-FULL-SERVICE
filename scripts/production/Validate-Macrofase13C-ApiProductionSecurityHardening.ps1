param(
    [Parameter(Mandatory = $true)]
    [string]$BaseUrl,

    [switch]$AllowSwaggerInProduction
)

$ErrorActionPreference = 'Stop'

function Join-UrlPath {
    param([string]$Base, [string]$Path)
    return $Base.TrimEnd('/') + $Path
}

function Invoke-GetStatus {
    param([string]$Path)

    $url = Join-UrlPath -Base $BaseUrl -Path $Path
    try {
        $response = Invoke-WebRequest -Uri $url -Method GET -UseBasicParsing -TimeoutSec 30
        return @{ StatusCode = [int]$response.StatusCode; Body = $response.Content }
    }
    catch {
        if ($_.Exception.Response -ne $null) {
            $statusCode = [int]$_.Exception.Response.StatusCode
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $body = $reader.ReadToEnd()
            return @{ StatusCode = $statusCode; Body = $body }
        }
        throw
    }
}

Write-Host 'MACROFASE 13C API production security hardening validation started.'
Write-Host "BaseUrl: $BaseUrl"
Write-Host 'Mode: GET-only. No POST/PUT/PATCH/DELETE will be executed.'

$publicExpected200 = @('/', '/health', '/api/health', '/health/live', '/health/ready')
foreach ($path in $publicExpected200) {
    Write-Host "Checking public endpoint $path ..."
    $result = Invoke-GetStatus -Path $path
    Write-Host "$path -> $($result.StatusCode)"
    if ($result.StatusCode -ne 200) {
        throw "Expected 200 for ${path}, got $($result.StatusCode)."
    }
    if ($path -eq '/health/ready' -and ($result.Body -notmatch 'Connected')) {
        throw '/health/ready did not report database Connected.'
    }
}

$metricsExpected404 = @('/metrics', '/health/metrics')
foreach ($path in $metricsExpected404) {
    Write-Host "Checking hardened metrics endpoint $path ..."
    $result = Invoke-GetStatus -Path $path
    Write-Host "$path -> $($result.StatusCode)"
    if ($result.StatusCode -ne 404) {
        throw "Expected 404 for ${path}, got $($result.StatusCode)."
    }
}

$swaggerResult = Invoke-GetStatus -Path '/swagger'
Write-Host "/swagger -> $($swaggerResult.StatusCode)"
if (-not $AllowSwaggerInProduction) {
    if ($swaggerResult.StatusCode -eq 200) {
        throw 'Swagger is publicly accessible in production. Set -AllowSwaggerInProduction only if ENABLE_SWAGGER=true is intentionally enabled.'
    }
}

$protectedReadPaths = @('/api/v1/products', '/api/v1/products/changes', '/api/v1/orders', '/api/v1/orders/1', '/api/v1/sync/changes')
foreach ($path in $protectedReadPaths) {
    Write-Host "Checking protected endpoint without JWT $path ..."
    $result = Invoke-GetStatus -Path $path
    Write-Host "$path -> $($result.StatusCode)"
    if ($result.StatusCode -eq 200) {
        throw "Protected endpoint ${path} returned 200 without JWT."
    }
    if ($result.StatusCode -ne 401) {
        throw "Expected normalized 401 for protected endpoint ${path} without JWT, got $($result.StatusCode)."
    }
}

Write-Host 'MACROFASE 13C API production security hardening validation passed.'
