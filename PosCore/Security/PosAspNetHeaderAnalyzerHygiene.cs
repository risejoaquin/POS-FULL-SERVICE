namespace PosCore.Security;

/// <summary>
/// PHASE 7E ASP.NET header analyzer hygiene contract.
/// This phase replaces HeaderDictionary.Add usage in CorrelationIdMiddleware with safe header indexer assignment to satisfy ASP0019 while preserving correlation behavior.
/// It does not change checkout behavior, does not mutate inventory, does not enable production sync, does not change schema and does not add migrations.
/// </summary>
public static class PosAspNetHeaderAnalyzerHygiene
{
    public const string HygieneName = "POS ASP.NET Header Analyzer Hygiene";

    public static readonly string[] RequiredAspNetHeaderAnalyzerHygieneChecks =
    {
        "ASP.NET header analyzer hygiene documented",
        "ASP0019 analyzer hygiene documented",
        "CorrelationIdMiddleware header Add usage removed",
        "request correlation header indexer assignment implemented",
        "response correlation header indexer assignment implemented",
        "duplicate response header exception risk reduced",
        "correlation id behavior preserved",
        "no public API behavior change",
        "no checkout behavior change",
        "no inventory mutation",
        "no production sync enablement",
        "no schema change",
        "no migrations",
        "operator-safe ASP.NET header hygiene message documented"
    };

    public static string RequiredAspNetHeaderAnalyzerHygieneText => string.Join("; ", RequiredAspNetHeaderAnalyzerHygieneChecks);

    public sealed record AspNetHeaderAnalyzerHygieneEvidence(
        string Scope,
        string AnalyzerWarning,
        string RemediatedComponent,
        string SafetyStatement);

    public static bool HasMinimumAspNetHeaderAnalyzerHygieneReadiness(
        bool hasAsp0019Remediation,
        bool hasRequestHeaderIndexerAssignment,
        bool hasResponseHeaderIndexerAssignment,
        bool hasNoBusinessLogicChange,
        bool hasNoSchemaChange)
    {
        return hasAsp0019Remediation
            && hasRequestHeaderIndexerAssignment
            && hasResponseHeaderIndexerAssignment
            && hasNoBusinessLogicChange
            && hasNoSchemaChange;
    }

    public static AspNetHeaderAnalyzerHygieneEvidence BuildAspNetHeaderAnalyzerHygieneEvidence()
    {
        return new AspNetHeaderAnalyzerHygieneEvidence(
            "ASP.NET header analyzer hygiene only",
            "ASP0019 HeaderDictionary.Add should be replaced with Append or indexer assignment",
            "PosServer.Middlewares.CorrelationIdMiddleware",
            "analyzer hygiene only - no public API behavior change, no checkout behavior change, no inventory mutation, no production sync enablement, no schema change, no migrations");
    }

    public static string BuildAspNetHeaderAnalyzerHygieneSummary(bool ready)
    {
        var status = ready ? "ready" : "blocked";
        return $"aspnet_header_analyzer_hygiene_status={status}; analyzer=ASP0019; component=CorrelationIdMiddleware; indexer assignment applied; correlation id behavior preserved; no public API behavior change; no checkout behavior change; no inventory mutation; no production sync enablement; no schema change; no migrations";
    }
}
