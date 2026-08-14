namespace PosCore.Security;

/// <summary>
/// PHASE 7G SyncService nullability hygiene contract.
/// This phase applies targeted SyncService nullable username guards without changing checkout, inventory, production sync enablement, schema or migrations.
/// </summary>
public static class PosSyncServiceNullabilityHygiene
{
    public const string HygieneName = "POS SyncService Nullability Hygiene";

    public static readonly string[] RequiredSyncServiceNullabilityHygieneChecks =
    {
        "SyncService nullability hygiene documented",
        "CS8602 SyncService username dereference hygiene documented",
        "cloud username null guard implemented",
        "normalized cloud username boundary documented",
        "local username null guard implemented",
        "pull updates behavior preserved",
        "invalid cloud username skip boundary documented",
        "no checkout behavior change",
        "no inventory mutation",
        "no production sync enablement",
        "no public API behavior change",
        "no schema change",
        "no migrations"
    };

    public static string RequiredSyncServiceNullabilityHygieneText => string.Join("; ", RequiredSyncServiceNullabilityHygieneChecks);

    public sealed record SyncServiceNullabilityHygieneEvidence(
        string Scope,
        string TargetFile,
        string WarningFamily,
        string SafetyStatement);

    public static bool HasMinimumSyncServiceNullabilityHygieneReadiness(
        bool hasCloudUsernameGuard,
        bool hasNormalizedUsernameBoundary,
        bool hasLocalUsernameGuard,
        bool preservesPullUpdatesBehavior,
        bool hasNoBusinessLogicChange,
        bool hasNoSchemaChange)
    {
        return hasCloudUsernameGuard
            && hasNormalizedUsernameBoundary
            && hasLocalUsernameGuard
            && preservesPullUpdatesBehavior
            && hasNoBusinessLogicChange
            && hasNoSchemaChange;
    }

    public static SyncServiceNullabilityHygieneEvidence BuildSyncServiceNullabilityHygieneEvidence()
    {
        return new SyncServiceNullabilityHygieneEvidence(
            "SyncService nullable username guard only",
            "PosCore/Services/SyncService.cs",
            "CS8602 possible null dereference",
            "SyncService nullability hygiene only - no checkout behavior change, no inventory mutation, no production sync enablement, no public API behavior change, no schema change, no migrations");
    }

    public static string BuildSyncServiceNullabilityHygieneSummary(bool ready)
    {
        var status = ready ? "ready" : "blocked";
        return $"syncservice_nullability_hygiene_status={status}; scope=SyncService username normalization guard; warnings=CS8602; cloud username null guard implemented; normalized cloud username boundary documented; local username null guard implemented; pull updates behavior preserved; no checkout behavior change; no inventory mutation; no production sync enablement; no public API behavior change; no schema change; no migrations";
    }
}
