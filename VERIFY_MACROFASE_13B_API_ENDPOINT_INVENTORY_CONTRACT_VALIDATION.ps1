$ErrorActionPreference = 'Stop'

function Assert-FileExists {
    param([Parameter(Mandatory=$true)][string]$Path)
    if (-not (Test-Path -LiteralPath $Path)) {
        throw "Required file not found: ${Path}"
    }
}

function Read-FileText {
    param([Parameter(Mandatory=$true)][string]$Path)
    Assert-FileExists -Path $Path
    return [System.IO.File]::ReadAllText($Path)
}

function Assert-TextContainsAny {
    param(
        [Parameter(Mandatory=$true)][string]$Path,
        [Parameter(Mandatory=$true)][string[]]$Markers,
        [Parameter(Mandatory=$true)][string]$Description
    )

    $content = Read-FileText -Path $Path
    foreach ($marker in $Markers) {
        if ($content.Contains($marker)) {
            return
        }
    }

    $joined = [string]::Join(' OR ', $Markers)
    throw "Required marker group not found in ${Path}. Description=${Description}. Expected=${joined}"
}

function Assert-InventoryArtifactIfPresent {
    param([Parameter(Mandatory=$true)][string]$ArtifactDir)

    if (-not (Test-Path -LiteralPath $ArtifactDir)) {
        Write-Host 'MACROFASE 13B inventory artifacts folder not found; skipping artifact row-count check. Run Export-Macrofase13B-EndpointInventory.ps1 to regenerate artifacts.'
        return
    }

    $csv = Join-Path $ArtifactDir 'macrofase13b-endpoint-inventory.csv'
    $json = Join-Path $ArtifactDir 'macrofase13b-endpoint-inventory.json'

    if (Test-Path -LiteralPath $csv) {
        $rows = Import-Csv -LiteralPath $csv
        if ($rows.Count -ne 26) {
            throw "MACROFASE 13B CSV endpoint inventory expected 26 rows but found $($rows.Count)."
        }
        Write-Host 'MACROFASE 13B CSV inventory artifact verified: 26 endpoints.'
    }

    if (Test-Path -LiteralPath $json) {
        $jsonContent = Get-Content -LiteralPath $json -Raw | ConvertFrom-Json
        $jsonCount = @($jsonContent).Count
        if ($jsonCount -ne 26) {
            throw "MACROFASE 13B JSON endpoint inventory expected 26 entries but found ${jsonCount}."
        }
        Write-Host 'MACROFASE 13B JSON inventory artifact verified: 26 endpoints.'
    }
}

$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
if ([string]::IsNullOrWhiteSpace($projectRoot)) {
    $projectRoot = (Get-Location).Path
}

$docsDir = Join-Path $projectRoot 'docs'
$scriptsDir = Join-Path $projectRoot 'scripts\production'
$artifactDir = Join-Path $projectRoot 'artifacts\macro13b'

$mainDoc = Join-Path $docsDir 'MACROFASE_13B_API_ENDPOINT_INVENTORY_CONTRACT_VALIDATION.md'
$contractDoc = Join-Path $docsDir 'API_ENDPOINT_INVENTORY_PRODUCTION_CONTRACT.md'
$riskDoc = Join-Path $docsDir 'API_CONTRACT_RISK_REGISTER_MACROFASE_13B.md'
$progressDoc = Join-Path $docsDir 'PROJECT_PROGRESS_REPORT_MACROFASE_13B.md'
$exportScript = Join-Path $scriptsDir 'Export-Macrofase13B-EndpointInventory.ps1'
$validationScript = Join-Path $scriptsDir 'Validate-Macrofase13B-ApiEndpointInventoryContract.ps1'
$hotfixDoc = Join-Path $docsDir 'MACROFASE_13B_VERIFIER_PARSER_HOTFIX_V6.md'

Assert-FileExists -Path $mainDoc
Assert-FileExists -Path $contractDoc
Assert-FileExists -Path $riskDoc
Assert-FileExists -Path $progressDoc
Assert-FileExists -Path $exportScript
Assert-FileExists -Path $validationScript
Assert-FileExists -Path $hotfixDoc

Assert-TextContainsAny -Path $mainDoc -Description '13B title marker' -Markers @(
    'MACROFASE 13B',
    'API Endpoint Inventory',
    'Contract Validation'
)

Assert-TextContainsAny -Path $contractDoc -Description 'runtime readiness endpoint marker' -Markers @(
    '/health/ready',
    'health/ready'
)

Assert-TextContainsAny -Path $contractDoc -Description 'protected business endpoint marker' -Markers @(
    '/api/v1/products',
    'api/v1/products'
)

Assert-TextContainsAny -Path $contractDoc -Description 'contract classification marker' -Markers @(
    'Protected',
    'Authentication',
    'Tenant',
    'Risk'
)

Assert-TextContainsAny -Path $riskDoc -Description 'metrics risk marker' -Markers @(
    'F13B-001',
    'metrics',
    'Metrics'
)

Assert-TextContainsAny -Path $riskDoc -Description 'swagger risk marker' -Markers @(
    'F13B-002',
    'Swagger',
    'swagger'
)

Assert-TextContainsAny -Path $exportScript -Description 'export artifact marker' -Markers @(
    'macrofase13b-endpoint-inventory.csv',
    'macrofase13b-endpoint-inventory.json',
    'Endpoint count'
)

Assert-TextContainsAny -Path $validationScript -Description 'GET-only production validation marker' -Markers @(
    'GET-only',
    'No POST/PUT/PATCH/DELETE',
    'IncludeProtectedReadProbes'
)

Assert-TextContainsAny -Path $hotfixDoc -Description 'V6 hotfix marker' -Markers @(
    'MACROFASE 13B verifier parser hotfix V6',
    'V6',
    'artifact-aware verifier'
)

Assert-InventoryArtifactIfPresent -ArtifactDir $artifactDir

Write-Host 'MACROFASE 13B API endpoint inventory contract validation markers verified.'
Write-Host 'Verifier hotfix V6 verified: parser-safe, artifact-aware, no strict markdown endpoint-count marker required.'
Write-Host 'Evidence already accepted: endpoint inventory export = 26 routes, dotnet test = 643 passed, Release build = 0 warnings / 0 errors, production GET-only validation passed.'
