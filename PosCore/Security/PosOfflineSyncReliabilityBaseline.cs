namespace PosCore.Security;

/// <summary>
/// PHASE 4A baseline contract for POS offline sync reliability.
/// This helper is a baseline only; it does not execute sync, does not write queues,
/// does not change checkout, and does not mutate inventory.
/// </summary>
public static class PosOfflineSyncReliabilityBaseline
{
    public const string BaselineName = "POS Offline Sync Reliability Baseline";
    public const string BaselineScope = "offline sync reliability baseline only";

    public static readonly string[] RequiredReliabilityChecks =
    {
        "offline queue inventory reviewed",
        "idempotency key strategy reviewed",
        "retry backoff policy documented",
        "conflict detection strategy documented",
        "sync checkpoint and last-success state reviewed",
        "tenant boundary validation reviewed",
        "observability correlation id reviewed",
        "operator-safe failure message documented",
        "no checkout changes",
        "no inventory mutation",
        "no schema change",
        "no migrations",
        "no production sync execution"
    };

    public static string RequiredReliabilityChecksText => string.Join("; ", RequiredReliabilityChecks);

    public static bool HasMinimumReliabilityDesign(
        bool hasIdempotencyStrategy,
        bool hasRetryPolicy,
        bool hasConflictStrategy,
        bool hasCheckpointDecision,
        bool hasTenantBoundaryDecision)
    {
        return hasIdempotencyStrategy
            && hasRetryPolicy
            && hasConflictStrategy
            && hasCheckpointDecision
            && hasTenantBoundaryDecision;
    }

    public static string BuildBaselineSummary(
        bool hasIdempotencyStrategy,
        bool hasRetryPolicy,
        bool hasConflictStrategy,
        bool hasCheckpointDecision,
        bool hasTenantBoundaryDecision,
        DateTime reviewedAt)
    {
        return $"{BaselineName}: idempotency={hasIdempotencyStrategy}; retry={hasRetryPolicy}; conflict={hasConflictStrategy}; checkpoint={hasCheckpointDecision}; tenantBoundary={hasTenantBoundaryDecision}; reviewedAt={reviewedAt:O}; baseline only; no production sync execution; no inventory mutation; no checkout changes; no schema change.";
    }
}
