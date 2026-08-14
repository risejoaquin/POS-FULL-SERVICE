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
    if (!$content.Contains($Text)) {
        throw "Missing marker '$Text' in $Path"
    }
}

Assert-FileContains "PosCore\Security\PosClientOrderServiceAsyncHygiene.cs" "PosClientOrderServiceAsyncHygiene"
Assert-FileContains "PosCore\Security\PosClientOrderServiceAsyncHygiene.cs" "POS ClientOrderService Async Hygiene"
Assert-FileContains "PosCore\Security\PosClientOrderServiceAsyncHygiene.cs" "CS1998 ClientOrderService async without await hygiene documented"
Assert-FileContains "PosCore\Security\PosClientOrderServiceAsyncHygiene.cs" "CreateDraftOrderAsync Task contract preserved"
Assert-FileContains "PosCore\Security\PosClientOrderServiceAsyncHygiene.cs" "Task.FromResult result boundary implemented"
Assert-FileContains "PosCore\Security\PosClientOrderServiceAsyncHygiene.cs" "no checkout behavior change"
Assert-FileContains "PosCore\Security\PosClientOrderServiceAsyncHygiene.cs" "no inventory mutation"
Assert-FileContains "PosCore\Security\PosClientOrderServiceAsyncHygiene.cs" "no production sync enablement"
Assert-FileContains "PosCore\Security\PosClientOrderServiceAsyncHygiene.cs" "no schema change"
Assert-FileContains "PosCore\Security\PosClientOrderServiceAsyncHygiene.cs" "no migrations"

Assert-FileContains "PosApplication\UseCases\Orders\ClientOrderService.cs" "PHASE 7I ClientOrderService async hygiene applied"
Assert-FileContains "PosApplication\UseCases\Orders\ClientOrderService.cs" "public Task<Result<Order>> CreateDraftOrderAsync"
Assert-FileContains "PosApplication\UseCases\Orders\ClientOrderService.cs" "return Task.FromResult(Result<Order>.Success(order));"
Assert-FileContains "PosApplication\UseCases\Orders\ClientOrderService.cs" "public async Task<Result> CheckoutAsync"

Assert-FileContains "docs\POS_CLIENTORDERSERVICE_ASYNC_HYGIENE.md" "ClientOrderService async hygiene documented"
Assert-FileContains "docs\POS_CLIENTORDERSERVICE_ASYNC_HYGIENE.md" "Task.FromResult result boundary implemented"
Assert-FileContains "docs\PHASE_7I_CLIENTORDERSERVICE_ASYNC_HYGIENE.md" "385 tests passed"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_7I.md" "80% -> 90%"

Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "PosClientOrderServiceAsyncHygiene_Should_Define_ClientOrderService_Async_Checks"
Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "Phase7I_ClientOrderService_Should_Remove_Unnecessary_Async_State_Machine"
Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "VerifyPhase7I_Should_Require_ClientOrderService_Async_Hygiene_Markers"

Write-Host "PHASE 7I markers verified."
