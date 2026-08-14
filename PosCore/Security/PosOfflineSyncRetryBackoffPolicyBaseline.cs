namespace PosCore.Security;

/// <summary>
/// PHASE 4D - POS Offline Sync Retry Backoff Policy Baseline.
/// offline sync retry backoff policy baseline only: defines retry/backoff requirements for future offline sync reliability.
/// This helper does not execute production sync, does not write queue entries, does not mutate inventory, does not change checkout, and does not change schema.
/// </summary>
public static class PosOfflineSyncRetryBackoffPolicyBaseline
{
    public const string BaselineName = "POS Offline Sync Retry Backoff Policy Baseline";

    public static readonly string[] RequiredRetryBackoffPolicyChecks =
    {
        "retryable error classification documented",
        "non retryable error classification documented",
        "exponential backoff policy documented",
        "jitter strategy documented",
        "max retry attempts documented",
        "retry attempt counter reviewed",
        "next retry at decision documented",
        "dead letter/manual review threshold documented",
        "operator-safe retry failure message documented",
        "idempotency key reuse during retry documented",
        "tenant boundary validation reviewed",
        "correlation id logging reviewed",
        "no production sync execution",
        "no queue writes",
        "no checkout changes",
        "no inventory mutation",
        "no schema change",
        "no migrations"
    };

    public static string RequiredRetryBackoffPolicyText => string.Join("; ", RequiredRetryBackoffPolicyChecks);

    public static bool HasMinimumRetryBackoffPolicyDesign(
        bool hasRetryableErrorClassification,
        bool hasNonRetryableErrorClassification,
        bool hasExponentialBackoff,
        bool hasJitterStrategy,
        bool hasMaxRetryAttempts,
        bool hasDeadLetterThreshold,
        bool hasIdempotencyRetryReuse)
    {
        return hasRetryableErrorClassification
            && hasNonRetryableErrorClassification
            && hasExponentialBackoff
            && hasJitterStrategy
            && hasMaxRetryAttempts
            && hasDeadLetterThreshold
            && hasIdempotencyRetryReuse;
    }

    public static string BuildRetryBackoffSummary(
        bool hasRetryableErrorClassification,
        bool hasNonRetryableErrorClassification,
        bool hasExponentialBackoff,
        bool hasJitterStrategy,
        bool hasMaxRetryAttempts,
        bool hasDeadLetterThreshold,
        bool hasIdempotencyRetryReuse,
        DateTime reviewedAt)
    {
        var status = HasMinimumRetryBackoffPolicyDesign(
            hasRetryableErrorClassification,
            hasNonRetryableErrorClassification,
            hasExponentialBackoff,
            hasJitterStrategy,
            hasMaxRetryAttempts,
            hasDeadLetterThreshold,
            hasIdempotencyRetryReuse)
            ? "ready"
            : "blocked";

        return $"{BaselineName}: {status}. ReviewedAt={reviewedAt:O}. "
            + $"retryable_error_classification={hasRetryableErrorClassification}; "
            + $"non_retryable_error_classification={hasNonRetryableErrorClassification}; "
            + $"exponential_backoff={hasExponentialBackoff}; "
            + $"jitter_strategy={hasJitterStrategy}; "
            + $"max_retry_attempts={hasMaxRetryAttempts}; "
            + $"dead_letter_manual_review_threshold={hasDeadLetterThreshold}; "
            + $"idempotency_key_reuse_during_retry={hasIdempotencyRetryReuse}. "
            + "Diagnostic/design only: no production sync execution, no queue writes, no checkout changes, no inventory mutation, no schema change, no migrations.";
    }
}
