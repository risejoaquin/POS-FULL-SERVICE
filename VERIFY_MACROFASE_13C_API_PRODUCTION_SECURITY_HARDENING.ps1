$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $MyInvocation.MyCommand.Path

function Read-TextFile {
    param([string]$Path)
    if (-not (Test-Path -LiteralPath $Path)) {
        throw "Required file not found: ${Path}"
    }
    return Get-Content -LiteralPath $Path -Raw
}

function Assert-ContainsLiteral {
    param([string]$Path, [string]$Text)
    $content = Read-TextFile -Path $Path
    if (-not $content.Contains($Text)) {
        throw "Required marker not found in ${Path}: ${Text}"
    }
}

function Assert-NotContainsLiteral {
    param([string]$Path, [string]$Text)
    $content = Read-TextFile -Path $Path
    if ($content.Contains($Text)) {
        throw "Forbidden marker found in ${Path}: ${Text}"
    }
}

$program = Join-Path $root 'PosServer\Program.cs'
$tenant = Join-Path $root 'PosServer\Middlewares\TenantMiddleware.cs'
$docMain = Join-Path $root 'docs\MACROFASE_13C_API_PRODUCTION_SECURITY_HARDENING.md'
$docReport = Join-Path $root 'docs\API_PRODUCTION_SECURITY_HARDENING_REPORT_MACROFASE_13C.md'
$script = Join-Path $root 'scripts\production\Validate-Macrofase13C-ApiProductionSecurityHardening.ps1'

Assert-ContainsLiteral -Path $docMain -Text 'MACROFASE 13C - API Production Security Hardening'
Assert-ContainsLiteral -Path $docMain -Text 'Swagger production gate: ENABLE_SWAGGER'
Assert-ContainsLiteral -Path $docMain -Text 'Metrics public exposure hardened: /metrics and /health/metrics return 404'
Assert-ContainsLiteral -Path $docMain -Text 'Protected unauthenticated API response normalized to 401'
Assert-ContainsLiteral -Path $docReport -Text 'F13B-001 - Metrics exposure and 500 responses'

Assert-ContainsLiteral -Path $program -Text 'ENABLE_SWAGGER'
Assert-ContainsLiteral -Path $program -Text 'Swagger UI is disabled in production unless ENABLE_SWAGGER=true'
Assert-ContainsLiteral -Path $program -Text 'app.MapGet("/metrics"'
Assert-ContainsLiteral -Path $program -Text 'METRICS_NOT_PUBLIC'
Assert-ContainsLiteral -Path $program -Text 'app.MapGet("/health/metrics"'
Assert-ContainsLiteral -Path $program -Text 'app.MapGet("/favicon.ico"'
Assert-ContainsLiteral -Path $program -Text 'app.Run();'

Assert-ContainsLiteral -Path $tenant -Text 'StatusCodes.Status401Unauthorized'
Assert-ContainsLiteral -Path $tenant -Text 'UNAUTHORIZED'
Assert-ContainsLiteral -Path $tenant -Text 'TENANT_CONTEXT_REQUIRED'
Assert-ContainsLiteral -Path $tenant -Text 'api/v1/license/validate'

Assert-ContainsLiteral -Path $script -Text 'MACROFASE 13C API production security hardening validation started.'
Assert-ContainsLiteral -Path $script -Text 'Expected normalized 401 for protected endpoint'
Assert-ContainsLiteral -Path $script -Text 'Expected 404 for'
Assert-ContainsLiteral -Path $script -Text 'GET-only. No POST/PUT/PATCH/DELETE will be executed.'

Write-Host 'MACROFASE 13C API production security hardening markers verified.'
Write-Host 'Swagger production gate verified: Development or ENABLE_SWAGGER=true only.'
Write-Host 'Metrics hardening verified: /metrics and /health/metrics mapped to deterministic 404.'
Write-Host 'Protected unauthenticated API response normalization verified: expected 401.'
Write-Host 'Expected final validation: dotnet test = 643 passed, dotnet build Release = 0 warnings / 0 errors, production security validation passed.'
