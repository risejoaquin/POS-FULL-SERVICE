$ErrorActionPreference = "Stop"

function Assert-FileContains {
    param(
        [string]$Path,
        [string]$Text
    )

    if (!(Test-Path $Path)) {
        throw "Missing required file: $Path"
    }

    $content = Get-Content $Path -Raw
    if (-not $content.Contains($Text)) {
        throw "Missing marker '$Text' in $Path"
    }
}

function Assert-FileNotContains {
    param(
        [string]$Path,
        [string]$Text
    )

    if (!(Test-Path $Path)) {
        throw "Missing required file: $Path"
    }

    $content = Get-Content $Path -Raw
    if ($content.Contains($Text)) {
        throw "Unexpected marker '$Text' found in $Path"
    }
}

Assert-FileContains "PosCore\Security\PosAspNetHeaderAnalyzerHygiene.cs" "PosAspNetHeaderAnalyzerHygiene"
Assert-FileContains "PosCore\Security\PosAspNetHeaderAnalyzerHygiene.cs" "POS ASP.NET Header Analyzer Hygiene"
Assert-FileContains "PosCore\Security\PosAspNetHeaderAnalyzerHygiene.cs" "ASP0019 analyzer hygiene documented"
Assert-FileContains "PosCore\Security\PosAspNetHeaderAnalyzerHygiene.cs" "CorrelationIdMiddleware header Add usage removed"
Assert-FileContains "PosCore\Security\PosAspNetHeaderAnalyzerHygiene.cs" "request correlation header indexer assignment implemented"
Assert-FileContains "PosCore\Security\PosAspNetHeaderAnalyzerHygiene.cs" "response correlation header indexer assignment implemented"
Assert-FileContains "PosCore\Security\PosAspNetHeaderAnalyzerHygiene.cs" "correlation id behavior preserved"
Assert-FileContains "PosCore\Security\PosAspNetHeaderAnalyzerHygiene.cs" "no public API behavior change"
Assert-FileContains "PosCore\Security\PosAspNetHeaderAnalyzerHygiene.cs" "no checkout behavior change"
Assert-FileContains "PosCore\Security\PosAspNetHeaderAnalyzerHygiene.cs" "no inventory mutation"
Assert-FileContains "PosCore\Security\PosAspNetHeaderAnalyzerHygiene.cs" "no production sync enablement"
Assert-FileContains "PosCore\Security\PosAspNetHeaderAnalyzerHygiene.cs" "no schema change"
Assert-FileContains "PosCore\Security\PosAspNetHeaderAnalyzerHygiene.cs" "no migrations"

Assert-FileContains "PosServer\Middlewares\CorrelationIdMiddleware.cs" "context.Request.Headers[CorrelationIdHeader] = correlationId"
Assert-FileContains "PosServer\Middlewares\CorrelationIdMiddleware.cs" "context.Response.Headers[CorrelationIdHeader] = correlationId"
Assert-FileContains "PosServer\Middlewares\CorrelationIdMiddleware.cs" "PHASE 7E ASP.NET header analyzer hygiene applied"
Assert-FileNotContains "PosServer\Middlewares\CorrelationIdMiddleware.cs" "Headers.Add(CorrelationIdHeader, correlationId)"

Assert-FileContains "docs\POS_ASPNET_HEADER_ANALYZER_HYGIENE.md" "ASP0019 analyzer hygiene documented"
Assert-FileContains "docs\PHASE_7E_ASPNET_HEADER_ANALYZER_HYGIENE.md" "365 tests passed"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_7E.md" "40% -> 50%"
Assert-FileContains "README.md" "PHASE 7E"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "Security & Dependency Hardening: 40% -> 50%"

Write-Host "PHASE 7E markers verified."
