namespace PosCore.Security;

/// <summary>
/// PHASE 7D duplicate using cleanup and analyzer hygiene contract.
/// This phase removes exact duplicate using directives reported by analyzers while preserving business behavior.
/// It does not change checkout behavior, does not mutate inventory, does not enable production sync, does not change schema and does not add migrations.
/// </summary>
public static class PosDuplicateUsingCleanupAnalyzerHygiene
{
    public const string HygieneName = "POS Duplicate Using Cleanup & Analyzer Hygiene";

    public static readonly string[] RequiredDuplicateUsingCleanupChecks =
    {
        "duplicate using cleanup documented",
        "PosInfrastructure local repository duplicate using cleanup implemented",
        "PosServer controller duplicate using cleanup implemented",
        "PosCore service duplicate using cleanup implemented",
        "CS0105 analyzer hygiene documented",
        "exact duplicate using directives removed",
        "using order preserved where possible",
        "no namespace movement documented",
        "no public API behavior change",
        "no checkout behavior change",
        "no inventory mutation",
        "no production sync enablement",
        "no schema change",
        "no migrations",
        "operator-safe analyzer hygiene message documented"
    };

    public static string RequiredDuplicateUsingCleanupText => string.Join("; ", RequiredDuplicateUsingCleanupChecks);

    public sealed record DuplicateUsingCleanupEvidence(
        string Scope,
        string AnalyzerWarning,
        string CleanedAreas,
        string SafetyStatement);

    public static bool HasMinimumDuplicateUsingCleanupReadiness(
        bool hasInfrastructureRepositoryCleanup,
        bool hasServerControllerCleanup,
        bool hasCoreServiceCleanup,
        bool hasNoBusinessLogicChange,
        bool hasNoSchemaChange)
    {
        return hasInfrastructureRepositoryCleanup
            && hasServerControllerCleanup
            && hasCoreServiceCleanup
            && hasNoBusinessLogicChange
            && hasNoSchemaChange;
    }

    public static DuplicateUsingCleanupEvidence BuildDuplicateUsingCleanupEvidence()
    {
        return new DuplicateUsingCleanupEvidence(
            "duplicate using cleanup and analyzer hygiene only",
            "CS0105 duplicate using directive",
            "PosInfrastructure local repositories; PosServer controllers; PosCore services",
            "analyzer hygiene only - no checkout behavior change, no inventory mutation, no production sync enablement, no schema change, no migrations");
    }

    public static string BuildDuplicateUsingCleanupSummary(bool ready)
    {
        var status = ready ? "ready" : "blocked";
        return $"duplicate_using_cleanup_status={status}; analyzer=CS0105; areas=PosInfrastructure.LocalRepositories|PosServer.Controllers|PosCore.Services; no public API behavior change; no checkout behavior change; no inventory mutation; no production sync enablement; no schema change; no migrations";
    }
}
