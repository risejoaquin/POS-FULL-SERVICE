param(
    [Parameter(Mandatory = $true)]
    [string]$BaseUrl
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$base = $BaseUrl.TrimEnd('/')

$endpoints = @(
    @{ Path = "/"; ExpectedStatus = 200; Name = "root" },
    @{ Path = "/health"; ExpectedStatus = 200; Name = "health" },
    @{ Path = "/api/health"; ExpectedStatus = 200; Name = "api-health" },
    @{ Path = "/health/live"; ExpectedStatus = 200; Name = "health-live" },
    @{ Path = "/health/ready"; ExpectedStatus = 200; Name = "health-ready" }
)

Write-Host "MACROFASE 13 API production validation started."
Write-Host "BaseUrl: $base"

foreach ($endpoint in $endpoints) {
    $url = "$base$($endpoint.Path)"
    Write-Host "Checking $($endpoint.Path) ..."

    try {
        $response = Invoke-WebRequest -Uri $url -Method GET -TimeoutSec 30 -UseBasicParsing
    }
    catch {
        throw "Request failed for $url. $($_.Exception.Message)"
    }

    if ($response.StatusCode -ne $endpoint.ExpectedStatus) {
        throw "Unexpected status for $($endpoint.Path). Expected $($endpoint.ExpectedStatus), got $($response.StatusCode)."
    }

    Write-Host "$($endpoint.Path) -> $($response.StatusCode)"

    if ([string]::IsNullOrWhiteSpace($response.Content)) {
        throw "Response body is empty for $($endpoint.Path)."
    }

    try {
        $json = $response.Content | ConvertFrom-Json
    }
    catch {
        throw "Response is not valid JSON for $($endpoint.Path). Body: $($response.Content)"
    }

    if ($endpoint.Path -eq "/health/ready") {
        if ($null -eq $json.database) {
            throw "/health/ready did not return database field."
        }

        if ($json.database -ne "Connected") {
            throw "/health/ready database was not Connected. Actual: $($json.database)"
        }

        Write-Host "/health/ready database Connected"
    }
}

Write-Host "MACROFASE 13 API production validation passed."
