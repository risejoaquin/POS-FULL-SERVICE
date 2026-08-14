namespace PosCore.Security;

/// <summary>
/// PHASE 4G - POS Offline Sync Tenant/Device Boundary & Sync Ownership Baseline.
/// offline sync tenant device boundary and sync ownership baseline only: defines tenant, device, user/session, queue ownership, and sync ownership requirements for future offline sync reliability.
/// This helper does not execute production sync, does not write queue entries, does not claim sync ownership, does not mutate inventory, does not change checkout, and does not change schema.
/// </summary>
public static class PosOfflineSyncTenantDeviceBoundarySyncOwnershipBaseline
{
    public const string BaselineName = "POS Offline Sync Tenant/Device Boundary & Sync Ownership Baseline";

    public static readonly string[] RequiredTenantDeviceBoundarySyncOwnershipChecks =
    {
        "tenant id boundary documented",
        "device id boundary documented",
        "user session boundary documented",
        "local queue owner documented",
        "sync ownership boundary documented",
        "single writer ownership rule documented",
        "device registration requirement reviewed",
        "tenant mismatch rejection documented",
        "device mismatch rejection documented",
        "queue item ownership validation documented",
        "checkpoint ownership validation documented",
        "idempotency key tenant device scope documented",
        "retry/backoff tenant device scope documented",
        "conflict detection tenant device scope documented",
        "operator-safe ownership mismatch message documented",
        "correlation id logging reviewed",
        "no production sync execution",
        "no queue writes",
        "no sync ownership claim",
        "no checkpoint advancement",
        "no checkout changes",
        "no inventory mutation",
        "no schema change",
        "no migrations"
    };

    public static string RequiredTenantDeviceBoundarySyncOwnershipText => string.Join("; ", RequiredTenantDeviceBoundarySyncOwnershipChecks);

    public static bool HasMinimumTenantDeviceBoundarySyncOwnershipDesign(
        bool hasTenantBoundary,
        bool hasDeviceBoundary,
        bool hasUserSessionBoundary,
        bool hasLocalQueueOwner,
        bool hasSyncOwnershipBoundary,
        bool hasSingleWriterOwnershipRule,
        bool hasOwnershipMismatchRejection,
        bool hasCheckpointOwnershipValidation)
    {
        return hasTenantBoundary
            && hasDeviceBoundary
            && hasUserSessionBoundary
            && hasLocalQueueOwner
            && hasSyncOwnershipBoundary
            && hasSingleWriterOwnershipRule
            && hasOwnershipMismatchRejection
            && hasCheckpointOwnershipValidation;
    }

    public static string BuildTenantDeviceBoundarySyncOwnershipSummary(
        bool hasTenantBoundary,
        bool hasDeviceBoundary,
        bool hasUserSessionBoundary,
        bool hasLocalQueueOwner,
        bool hasSyncOwnershipBoundary,
        bool hasSingleWriterOwnershipRule,
        bool hasOwnershipMismatchRejection,
        bool hasCheckpointOwnershipValidation,
        DateTime reviewedAt)
    {
        var status = HasMinimumTenantDeviceBoundarySyncOwnershipDesign(
            hasTenantBoundary,
            hasDeviceBoundary,
            hasUserSessionBoundary,
            hasLocalQueueOwner,
            hasSyncOwnershipBoundary,
            hasSingleWriterOwnershipRule,
            hasOwnershipMismatchRejection,
            hasCheckpointOwnershipValidation)
            ? "ready"
            : "blocked";

        return $"{BaselineName}: {status}. ReviewedAt={reviewedAt:O}. "
            + $"tenant_boundary={hasTenantBoundary}; "
            + $"device_boundary={hasDeviceBoundary}; "
            + $"user_session_boundary={hasUserSessionBoundary}; "
            + $"local_queue_owner={hasLocalQueueOwner}; "
            + $"sync_ownership_boundary={hasSyncOwnershipBoundary}; "
            + $"single_writer_ownership_rule={hasSingleWriterOwnershipRule}; "
            + $"ownership_mismatch_rejection={hasOwnershipMismatchRejection}; "
            + $"checkpoint_ownership_validation={hasCheckpointOwnershipValidation}. "
            + "Diagnostic/design only: no production sync execution, no queue writes, no sync ownership claim, no checkpoint advancement, no checkout changes, no inventory mutation, no schema change, no migrations.";
    }
}
