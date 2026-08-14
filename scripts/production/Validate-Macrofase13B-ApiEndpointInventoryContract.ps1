param(
    [Parameter(Mandatory = $true)]
    [string]$BaseUrl,

    [switch]$IncludeProtectedReadProbes,
    [switch]$IncludeMetricsExposureProbe
)

$ErrorActionPreference = "Stop"

function Normalize-BaseUrl([string]$url) {
    return $url.TrimEnd('/')
}

function Invoke-GetStatus([string]$url) {
    try {
        $response = Invoke-WebRequest -Uri $url -Method GET -UseBasicParsing -TimeoutSec 30
        return [pscustomobject]@{ StatusCode = [int]$response.StatusCode; Body = $response.Content }
    }
    catch {
        if ($_.Exception.Response -ne $null) {
            $statusCode = [int]$_.Exception.Response.StatusCode
            return [pscustomobject]@{ StatusCode = $statusCode; Body = "" }
        }
        throw
    }
}

$base = Normalize-BaseUrl $BaseUrl
Write-Host "MACROFASE 13B API endpoint inventory contract validation started."
Write-Host "BaseUrl: $base"
Write-Host "Mode: GET-only. No POST/PUT/PATCH/DELETE will be executed."

$publicEndpoints = @(
    @{ Path = "/"; Expected = 200 },
    @{ Path = "/health"; Expected = 200 },
    @{ Path = "/api/health"; Expected = 200 },
    @{ Path = "/health/live"; Expected = 200 },
    @{ Path = "/health/ready"; Expected = 200 }
)

foreach ($endpoint in $publicEndpoints) {
    $target = "$base$($endpoint.Path)"
    Write-Host "Checking public endpoint $($endpoint.Path) ..."
    $result = Invoke-GetStatus $target
    Write-Host "$($endpoint.Path) -> $($result.StatusCode)"
    if ($result.StatusCode -ne $endpoint.Expected) {
        throw "Expected $($endpoint.Expected) from $($endpoint.Path), got $($result.StatusCode)."
    }
    if ($endpoint.Path -eq "/health/ready" -and $result.Body -notmatch "Connected") {
        throw "/health/ready did not report database Connected."
    }
}

if ($IncludeProtectedReadProbes) {
    $protectedReadEndpoints = @(
        "/api/v1/products",
        "/api/v1/products/changes",
        "/api/v1/orders",
        "/api/v1/orders/1",
        "/api/v1/sync/changes"
    )

    foreach ($path in $protectedReadEndpoints) {
        $target = "$base$path"
        Write-Host "Checking protected read endpoint without JWT $path ..."
        $result = Invoke-GetStatus $target
        Write-Host "$path -> $($result.StatusCode)"
        if ($result.StatusCode -eq 200) {
            throw "Protected read endpoint $path returned 200 without JWT."
        }
    }

    Write-Host "Protected read probes passed: no protected GET route returned 200 without JWT."
}
else {
    Write-Host "Protected read probes skipped. Re-run with -IncludeProtectedReadProbes for unauthenticated GET checks."
}

if ($IncludeMetricsExposureProbe) {
    $metricsRoutes = @("/health/metrics", "/metrics")
    foreach ($path in $metricsRoutes) {
        $target = "$base$path"
        Write-Host "Checking metrics exposure route $path ..."
        $result = Invoke-GetStatus $target
        Write-Host "$path -> $($result.StatusCode)"
        if ($result.StatusCode -eq 200) {
            Write-Warning "Metrics route $path is publicly reachable. This is a MACROFASE 13B risk-register item, not an automatic failure."
        }
    }
}
else {
    Write-Host "Metrics exposure probe skipped. Re-run with -IncludeMetricsExposureProbe for read-only exposure check."
}

Write-Host "MACROFASE 13B API endpoint inventory contract validation passed."
