namespace PosCore.Security;

/// <summary>
/// PHASE 4B baseline contract for POS offline sync queue inventory and diagnostics.
/// This helper is diagnostics only; it does not execute sync, does not write queues,
/// does not change checkout, and does not mutate inventory.
/// </summary>
public static class PosOfflineSyncQueueDiagnosticsBaseline
{
    public const string BaselineName = "POS Offline Sync Queue Inventory & Diagnostics Baseline";
    public const string BaselineScope = "offline sync queue diagnostics baseline only";

    public static readonly string[] RequiredQueueDiagnostics =
    {
        "offline queue location documented",
        "pending items count reviewed",
        "failed items count reviewed",
        "retry attempts reviewed",
        "last error summary reviewed",
        "oldest pending item reviewed",
        "idempotency key presence reviewed",
        "tenant id presence reviewed",
        "correlation id presence reviewed",
        "operator-safe diagnostic message documented",
        "no production sync execution",
        "no queue writes",
        "no checkout changes",
        "no inventory mutation",
        "no schema change",
        "no migrations"
    };

    public static string RequiredQueueDiagnosticsText => string.Join("; ", RequiredQueueDiagnostics);

    public static bool HasMinimumQueueDiagnosticDesign(
        bool hasQueueLocation,
        bool hasPendingCount,
        bool hasFailedCount,
        bool hasRetryAttempts,
        bool hasLastErrorSummary,
        bool hasIdempotencyDecision,
        bool hasTenantBoundaryDecision)
    {
        return hasQueueLocation
            && hasPendingCount
            && hasFailedCount
            && hasRetryAttempts
            && hasLastErrorSummary
            && hasIdempotencyDecision
            && hasTenantBoundaryDecision;
    }

    public static string BuildDiagnosticsSummary(
        bool hasQueueLocation,
        bool hasPendingCount,
        bool hasFailedCount,
        bool hasRetryAttempts,
        bool hasLastErrorSummary,
        bool hasIdempotencyDecision,
        bool hasTenantBoundaryDecision,
        DateTime reviewedAt)
    {
        return $"{BaselineName}: queueLocation={hasQueueLocation}; pendingCount={hasPendingCount}; failedCount={hasFailedCount}; retryAttempts={hasRetryAttempts}; lastErrorSummary={hasLastErrorSummary}; idempotency={hasIdempotencyDecision}; tenantBoundary={hasTenantBoundaryDecision}; reviewedAt={reviewedAt:O}; diagnostics only; no production sync execution; no queue writes; no inventory mutation; no checkout changes; no schema change.";
    }
}
