namespace PosCore.Security;

/// <summary>
/// PHASE 4F - POS Offline Sync Checkpoint & Last Success State Baseline.
/// offline sync checkpoint and last success state baseline only: defines checkpoint, last-success, resume-safety, and recovery-state requirements for future offline sync reliability.
/// This helper does not execute production sync, does not write queue entries, does not advance checkpoints, does not mutate inventory, does not change checkout, and does not change schema.
/// </summary>
public static class PosOfflineSyncCheckpointLastSuccessStateBaseline
{
    public const string BaselineName = "POS Offline Sync Checkpoint & Last Success State Baseline";

    public static readonly string[] RequiredCheckpointLastSuccessStateChecks =
    {
        "checkpoint strategy documented",
        "last successful sync timestamp reviewed",
        "last successful queue item id reviewed",
        "last successful server cursor reviewed",
        "resume from checkpoint behavior documented",
        "partial sync failure state documented",
        "atomic checkpoint update documented",
        "checkpoint rollback safety documented",
        "duplicate replay prevention documented",
        "idempotency key interaction documented",
        "retry/backoff interaction documented",
        "conflict detection interaction documented",
        "tenant boundary validation reviewed",
        "device id boundary reviewed",
        "operator-safe resume message documented",
        "correlation id logging reviewed",
        "no production sync execution",
        "no queue writes",
        "no checkpoint advancement",
        "no checkout changes",
        "no inventory mutation",
        "no schema change",
        "no migrations"
    };

    public static string RequiredCheckpointLastSuccessStateText => string.Join("; ", RequiredCheckpointLastSuccessStateChecks);

    public static bool HasMinimumCheckpointLastSuccessStateDesign(
        bool hasCheckpointStrategy,
        bool hasLastSuccessfulSyncTimestamp,
        bool hasLastSuccessfulQueueItemId,
        bool hasServerCursor,
        bool hasResumeBehavior,
        bool hasAtomicCheckpointUpdate,
        bool hasDuplicateReplayPrevention,
        bool hasTenantBoundary)
    {
        return hasCheckpointStrategy
            && hasLastSuccessfulSyncTimestamp
            && hasLastSuccessfulQueueItemId
            && hasServerCursor
            && hasResumeBehavior
            && hasAtomicCheckpointUpdate
            && hasDuplicateReplayPrevention
            && hasTenantBoundary;
    }

    public static string BuildCheckpointLastSuccessStateSummary(
        bool hasCheckpointStrategy,
        bool hasLastSuccessfulSyncTimestamp,
        bool hasLastSuccessfulQueueItemId,
        bool hasServerCursor,
        bool hasResumeBehavior,
        bool hasAtomicCheckpointUpdate,
        bool hasDuplicateReplayPrevention,
        bool hasTenantBoundary,
        DateTime reviewedAt)
    {
        var status = HasMinimumCheckpointLastSuccessStateDesign(
            hasCheckpointStrategy,
            hasLastSuccessfulSyncTimestamp,
            hasLastSuccessfulQueueItemId,
            hasServerCursor,
            hasResumeBehavior,
            hasAtomicCheckpointUpdate,
            hasDuplicateReplayPrevention,
            hasTenantBoundary)
            ? "ready"
            : "blocked";

        return $"{BaselineName}: {status}. ReviewedAt={reviewedAt:O}. "
            + $"checkpoint_strategy={hasCheckpointStrategy}; "
            + $"last_successful_sync_timestamp={hasLastSuccessfulSyncTimestamp}; "
            + $"last_successful_queue_item_id={hasLastSuccessfulQueueItemId}; "
            + $"server_cursor={hasServerCursor}; "
            + $"resume_behavior={hasResumeBehavior}; "
            + $"atomic_checkpoint_update={hasAtomicCheckpointUpdate}; "
            + $"duplicate_replay_prevention={hasDuplicateReplayPrevention}; "
            + $"tenant_boundary={hasTenantBoundary}. "
            + "Diagnostic/design only: no production sync execution, no queue writes, no checkpoint advancement, no checkout changes, no inventory mutation, no schema change, no migrations.";
    }
}
