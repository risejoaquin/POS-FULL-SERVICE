using System;

namespace PosCore.Security;

/// <summary>
/// PHASE 7B nullability warning hardening baseline.
/// This phase inventories and classifies nullable reference warnings before code-level remediation.
/// It does not change checkout behavior, does not mutate inventory, does not enable production sync, and does not add schema changes or migrations.
/// </summary>
public static class PosNullabilityWarningHardeningBaseline
{
    public const string BaselineName = "POS Nullability Warning Hardening Baseline";

    public const string Scope = "nullable reference warning classification and remediation planning only";

    public static readonly string[] RequiredNullabilityWarningHardeningChecks =
    {
        "nullability warning hardening baseline documented",
        "CS8602 possible null dereference classified",
        "CS8601 possible null assignment classified",
        "CS8618 non-nullable initialization classified",
        "CS8622 delegate nullability mismatch classified",
        "CS8600 possible null conversion classified",
        "CS8603 possible null return classified",
        "server service nullability hotspots documented",
        "builder nullability hotspots documented",
        "sync service nullability hotspots documented",
        "central db context nullability hotspots documented",
        "remediation order documented",
        "fail-safe null handling requirement documented",
        "operator-safe nullability hardening message documented",
        "no checkout behavior change",
        "no inventory mutation",
        "no production sync enablement",
        "no schema change",
        "no migrations"
    };

    public static string RequiredNullabilityWarningHardeningText => string.Join("; ", RequiredNullabilityWarningHardeningChecks);

    public sealed record NullabilityWarningHardeningEvidence(
        string Baseline,
        string WarningCodes,
        string Hotspots,
        string RemediationOrder,
        DateTime ReviewedAt,
        string SafetyStatement);

    public static bool HasMinimumNullabilityWarningHardeningReadiness(
        bool hasWarningCodeClassification,
        bool hasHotspotInventory,
        bool hasRemediationOrder,
        bool hasFailSafeNullHandlingRequirement,
        bool hasNoBusinessLogicChange)
    {
        return hasWarningCodeClassification
            && hasHotspotInventory
            && hasRemediationOrder
            && hasFailSafeNullHandlingRequirement
            && hasNoBusinessLogicChange;
    }

    public static NullabilityWarningHardeningEvidence BuildNullabilityWarningHardeningEvidence(DateTime reviewedAt)
    {
        return new NullabilityWarningHardeningEvidence(
            BaselineName,
            "CS8602, CS8601, CS8618, CS8622, CS8600, CS8603",
            "PosInfrastructure server services, CentralDbContext, PosCore SyncService, PosBuilder view models and controls",
            "classify first, apply smallest safe nullable annotations/guards next, preserve behavior, keep tests green",
            reviewedAt,
            "nullability hardening baseline only - no checkout behavior change, no inventory mutation, no production sync enablement, no schema change, no migrations");
    }

    public static string BuildNullabilityWarningHardeningSummary(bool ready, DateTime reviewedAt)
    {
        var status = ready ? "ready" : "blocked";
        return $"nullability_warning_hardening_status={status}; warning_codes=CS8602|CS8601|CS8618|CS8622|CS8600|CS8603; reviewed_at={reviewedAt:O}; no checkout behavior change; no inventory mutation; no production sync enablement; no schema change; no migrations";
    }
}
