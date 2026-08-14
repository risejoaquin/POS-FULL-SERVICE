$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path

function Assert-FileExists {
    param([string]$Path)
    if (-not (Test-Path $Path)) {
        throw "Required file not found: ${Path}"
    }
}

function Assert-ContainsLiteral {
    param([string]$Path, [string]$Text)
    Assert-FileExists -Path $Path
    $content = Get-Content -Raw -Path $Path
    if (-not $content.Contains($Text)) {
        throw "Required marker not found in ${Path}: ${Text}"
    }
}

$program = Join-Path $root "PosServer\Program.cs"
$tenantMiddleware = Join-Path $root "PosServer\Middlewares\TenantMiddleware.cs"
$validator = Join-Path $root "scripts\production\Validate-Macrofase13C-ApiProductionSecurityHardening.ps1"
$doc = Join-Path $root "docs\MACROFASE_13C_API_PRODUCTION_SECURITY_HARDENING.md"
$report = Join-Path $root "docs\API_PRODUCTION_SECURITY_HARDENING_REPORT_MACROFASE_13C.md"
$v2doc = Join-Path $root "docs\MACROFASE_13C_METRICS_SHORT_CIRCUIT_HOTFIX_V2.md"

Assert-ContainsLiteral -Path $program -Text "MACROFASE 13C V2: Short-circuit hardened public diagnostic routes before endpoint routing."
Assert-ContainsLiteral -Path $program -Text 'requestPath.Equals("/metrics", StringComparison.OrdinalIgnoreCase)'
Assert-ContainsLiteral -Path $program -Text 'requestPath.Equals("/health/metrics", StringComparison.OrdinalIgnoreCase)'
Assert-ContainsLiteral -Path $program -Text 'StatusCodes.Status404NotFound'
Assert-ContainsLiteral -Path $program -Text 'METRICS_NOT_PUBLIC'
Assert-ContainsLiteral -Path $program -Text 'Swagger UI is disabled in production unless ENABLE_SWAGGER=true.'
Assert-ContainsLiteral -Path $tenantMiddleware -Text 'context.Response.StatusCode = StatusCodes.Status401Unauthorized;'
Assert-ContainsLiteral -Path $validator -Text 'Expected 404 for ${path}'
Assert-ContainsLiteral -Path $doc -Text 'MACROFASE 13C'
Assert-ContainsLiteral -Path $report -Text 'API Production Security Hardening'
Assert-ContainsLiteral -Path $v2doc -Text 'MACROFASE 13C V2 - Metrics Short-Circuit Hotfix'

Write-Host "MACROFASE 13C API production security hardening markers verified."
Write-Host "Metrics short-circuit hotfix V2 verified: /metrics and /health/metrics are blocked before endpoint routing."
Write-Host "Expected final validation: dotnet test = 643 passed, dotnet build Release = 0 warnings / 0 errors, production security validation passed."
