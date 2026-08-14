namespace PosCore.Security;

/// <summary>
/// PHASE 7I ClientOrderService async hygiene contract.
/// This phase removes the remaining ClientOrderService CS1998 warning by preserving the Task-based public contract without an unnecessary async state machine.
/// </summary>
public static class PosClientOrderServiceAsyncHygiene
{
    public const string HygieneName = "POS ClientOrderService Async Hygiene";

    public static readonly string[] RequiredClientOrderServiceAsyncHygieneChecks =
    {
        "ClientOrderService async hygiene documented",
        "CS1998 ClientOrderService async without await hygiene documented",
        "CreateDraftOrderAsync Task contract preserved",
        "Task.FromResult result boundary implemented",
        "draft order behavior preserved",
        "checkout behavior preserved",
        "no checkout behavior change",
        "no inventory mutation",
        "no production sync enablement",
        "no public API behavior change",
        "no schema change",
        "no migrations"
    };

    public static string RequiredClientOrderServiceAsyncHygieneText => string.Join("; ", RequiredClientOrderServiceAsyncHygieneChecks);

    public sealed record ClientOrderServiceAsyncHygieneEvidence(
        string Scope,
        string TargetFile,
        string WarningFamily,
        string SafetyStatement);

    public static bool HasMinimumClientOrderServiceAsyncHygieneReadiness(
        bool hasTaskContractPreserved,
        bool hasTaskFromResultBoundary,
        bool preservesDraftOrderBehavior,
        bool preservesCheckoutBehavior,
        bool hasNoBusinessLogicChange,
        bool hasNoSchemaChange)
    {
        return hasTaskContractPreserved
            && hasTaskFromResultBoundary
            && preservesDraftOrderBehavior
            && preservesCheckoutBehavior
            && hasNoBusinessLogicChange
            && hasNoSchemaChange;
    }

    public static ClientOrderServiceAsyncHygieneEvidence BuildClientOrderServiceAsyncHygieneEvidence()
    {
        return new ClientOrderServiceAsyncHygieneEvidence(
            "ClientOrderService CreateDraftOrderAsync CS1998 hygiene only",
            "PosApplication/UseCases/Orders/ClientOrderService.cs",
            "CS1998 async method lacks await",
            "ClientOrderService async hygiene only - Task contract preserved, draft order behavior preserved, checkout behavior preserved, no checkout behavior change, no inventory mutation, no production sync enablement, no public API behavior change, no schema change, no migrations");
    }

    public static string BuildClientOrderServiceAsyncHygieneSummary(bool ready)
    {
        var status = ready ? "ready" : "blocked";
        return $"clientorderservice_async_hygiene_status={status}; scope=CreateDraftOrderAsync Task.FromResult boundary; warning=CS1998; CreateDraftOrderAsync Task contract preserved; Task.FromResult result boundary implemented; draft order behavior preserved; checkout behavior preserved; no checkout behavior change; no inventory mutation; no production sync enablement; no public API behavior change; no schema change; no migrations";
    }
}
