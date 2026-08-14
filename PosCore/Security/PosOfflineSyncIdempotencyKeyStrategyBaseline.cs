namespace PosCore.Security;

/// <summary>
/// PHASE 4C - POS Offline Sync Idempotency Key Strategy Baseline.
/// offline sync idempotency key strategy baseline only: defines deterministic idempotency key requirements for future offline sync events.
/// This helper does not execute production sync, does not write queue entries, does not mutate inventory, does not change checkout, and does not change schema.
/// </summary>
public static class PosOfflineSyncIdempotencyKeyStrategyBaseline
{
    public const string BaselineName = "POS Offline Sync Idempotency Key Strategy Baseline";

    public static readonly string[] RequiredIdempotencyKeyStrategyChecks =
    {
        "deterministic event identity documented",
        "tenant id included in key scope",
        "device id included in key scope",
        "local event id included in key scope",
        "entity type included in key scope",
        "entity id included in key scope",
        "operation type included in key scope",
        "created at timestamp reviewed",
        "idempotency key immutability documented",
        "duplicate submission handling documented",
        "retry reuse of same key documented",
        "conflict-safe server behavior documented",
        "operator-safe duplicate message documented",
        "no production sync execution",
        "no queue writes",
        "no checkout changes",
        "no inventory mutation",
        "no schema change",
        "no migrations"
    };

    public static string RequiredIdempotencyKeyStrategyText => string.Join("; ", RequiredIdempotencyKeyStrategyChecks);

    public static bool HasMinimumIdempotencyKeyStrategyDesign(
        bool hasDeterministicEventIdentity,
        bool hasTenantScope,
        bool hasDeviceScope,
        bool hasLocalEventId,
        bool hasOperationType,
        bool hasRetryReuseDecision,
        bool hasDuplicateHandlingDecision)
    {
        return hasDeterministicEventIdentity
            && hasTenantScope
            && hasDeviceScope
            && hasLocalEventId
            && hasOperationType
            && hasRetryReuseDecision
            && hasDuplicateHandlingDecision;
    }

    public static string BuildStrategySummary(
        bool hasDeterministicEventIdentity,
        bool hasTenantScope,
        bool hasDeviceScope,
        bool hasLocalEventId,
        bool hasOperationType,
        bool hasRetryReuseDecision,
        bool hasDuplicateHandlingDecision,
        DateTime reviewedAt)
    {
        var status = HasMinimumIdempotencyKeyStrategyDesign(
            hasDeterministicEventIdentity,
            hasTenantScope,
            hasDeviceScope,
            hasLocalEventId,
            hasOperationType,
            hasRetryReuseDecision,
            hasDuplicateHandlingDecision)
            ? "ready"
            : "blocked";

        return $"{BaselineName}: {status}. ReviewedAt={reviewedAt:O}. "
            + $"deterministic_event_identity={hasDeterministicEventIdentity}; "
            + $"tenant_scope={hasTenantScope}; "
            + $"device_scope={hasDeviceScope}; "
            + $"local_event_id={hasLocalEventId}; "
            + $"operation_type={hasOperationType}; "
            + $"retry_reuse_same_key={hasRetryReuseDecision}; "
            + $"duplicate_handling={hasDuplicateHandlingDecision}. "
            + "Diagnostic/design only: no production sync execution, no queue writes, no checkout changes, no inventory mutation, no schema change, no migrations.";
    }
}
