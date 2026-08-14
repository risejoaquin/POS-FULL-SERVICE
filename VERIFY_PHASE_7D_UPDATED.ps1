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
    if ($content -notlike "*$Text*") {
        throw "Missing marker '$Text' in $Path"
    }
}

function Assert-UsingOccursOnce {
    param(
        [string]$Path,
        [string]$Text
    )

    if (!(Test-Path $Path)) {
        throw "Missing required file: $Path"
    }

    $content = Get-Content $Path -Raw
    $count = ([regex]::Matches($content, [regex]::Escape($Text))).Count
    if ($count -ne 1) {
        throw "Expected exactly one occurrence of '$Text' in $Path but found $count"
    }
}

Assert-FileContains "PosCore\Security\PosDuplicateUsingCleanupAnalyzerHygiene.cs" "PosDuplicateUsingCleanupAnalyzerHygiene"
Assert-FileContains "PosCore\Security\PosDuplicateUsingCleanupAnalyzerHygiene.cs" "POS Duplicate Using Cleanup & Analyzer Hygiene"
Assert-FileContains "PosCore\Security\PosDuplicateUsingCleanupAnalyzerHygiene.cs" "CS0105 analyzer hygiene documented"
Assert-FileContains "PosCore\Security\PosDuplicateUsingCleanupAnalyzerHygiene.cs" "exact duplicate using directives removed"
Assert-FileContains "PosCore\Security\PosDuplicateUsingCleanupAnalyzerHygiene.cs" "no public API behavior change"
Assert-FileContains "PosCore\Security\PosDuplicateUsingCleanupAnalyzerHygiene.cs" "no checkout behavior change"
Assert-FileContains "PosCore\Security\PosDuplicateUsingCleanupAnalyzerHygiene.cs" "no inventory mutation"
Assert-FileContains "PosCore\Security\PosDuplicateUsingCleanupAnalyzerHygiene.cs" "no production sync enablement"
Assert-FileContains "PosCore\Security\PosDuplicateUsingCleanupAnalyzerHygiene.cs" "no schema change"
Assert-FileContains "PosCore\Security\PosDuplicateUsingCleanupAnalyzerHygiene.cs" "no migrations"

Assert-UsingOccursOnce "PosInfrastructure\Repositories\Local\OrderRepository.cs" "using PosDomain.Interfaces;"
Assert-UsingOccursOnce "PosInfrastructure\Repositories\Local\ProductRepository.cs" "using PosDomain.Interfaces;"
Assert-UsingOccursOnce "PosInfrastructure\Repositories\Local\Repository.cs" "using PosDomain.Interfaces;"
Assert-UsingOccursOnce "PosServer\Controllers\AuthController.cs" "using PosApplication.Interfaces.Server;"
Assert-UsingOccursOnce "PosServer\Controllers\InventoryMovementsController.cs" "using PosApplication.Interfaces.Server;"
Assert-UsingOccursOnce "PosServer\Controllers\LicenseController.cs" "using PosApplication.Interfaces.Server;"
Assert-UsingOccursOnce "PosServer\Controllers\OrdersController.cs" "using PosApplication.Interfaces.Server;"
Assert-UsingOccursOnce "PosServer\Controllers\ProductsController.cs" "using PosApplication.Interfaces.Server;"
Assert-UsingOccursOnce "PosServer\Controllers\ShiftsController.cs" "using PosApplication.Interfaces.Server;"
Assert-UsingOccursOnce "PosServer\Controllers\SyncController.cs" "using PosApplication.Interfaces.Server;"
Assert-UsingOccursOnce "PosServer\Controllers\UsersController.cs" "using PosApplication.Interfaces.Server;"
Assert-UsingOccursOnce "PosCore\Services\DatabaseBackupService.cs" "using Serilog;"
Assert-UsingOccursOnce "PosCore\Services\LicenseService.cs" "using Serilog;"
Assert-UsingOccursOnce "PosCore\Services\TicketPrinterService.cs" "using Serilog;"

Assert-FileContains "docs\POS_DUPLICATE_USING_CLEANUP_ANALYZER_HYGIENE.md" "CS0105 analyzer hygiene documented"
Assert-FileContains "docs\PHASE_7D_DUPLICATE_USING_CLEANUP_ANALYZER_HYGIENE.md" "360 tests passed"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_7D.md" "30% -> 40%"
Assert-FileContains "README.md" "PHASE 7D"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "Security & Dependency Hardening: 30% -> 40%"

Write-Host "PHASE 7D markers verified."
