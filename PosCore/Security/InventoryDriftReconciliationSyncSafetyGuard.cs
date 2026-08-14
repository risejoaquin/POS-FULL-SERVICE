using System;
using System.Linq;
using System.Text;

namespace PosCore.Security;

/// <summary>
/// Sync-safe guard vocabulary for future controlled inventory drift reconciliation.
/// This helper is a baseline only; it does not write sync records, does not change queues, and does not apply inventory corrections.
/// </summary>
public static class InventoryDriftReconciliationSyncSafetyGuard
{
    public const string SyncSafetyBaselineName = "inventory.drift.reconciliation.sync-safe.guard.baseline";
    public const string SyncSafetyPreparationEvent = "inventory.drift.reconciliation.sync-safe.prepare";

    public static readonly string[] RequiredSyncSafetyChecks =
    {
        "tenant scoped reconciliation",
        "pending sync queue reviewed",
        "last successful sync reviewed",
        "offline mode decision documented",
        "idempotency key strategy defined",
        "conflict resolution strategy defined",
        "audit trail evidence linked",
        "no checkout changes",
        "no sync changes",
        "no schema change",
        "no inventory mutation"
    };

    public static string RequiredChecksText => string.Join(", ", RequiredSyncSafetyChecks);

    public static bool HasRequiredPreparationState(
        bool hasDrift,
        bool manualReviewRequired,
        bool designReady,
        bool permissionGranted,
        bool auditTrailReady)
    {
        return hasDrift
            && manualReviewRequired
            && designReady
            && permissionGranted
            && auditTrailReady;
    }

    public static string BuildSafetyChecklist()
    {
        return string.Join(Environment.NewLine, RequiredSyncSafetyChecks.Select(check => $"- {check}"));
    }

    public static string BuildSafetyDecision(string status, string reason, DateTime reviewedAt)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Inventory Drift Reconciliation Sync-Safe Guard Baseline");
        builder.AppendLine($"ReviewedAt: {reviewedAt:yyyy-MM-dd HH:mm:ss}");
        builder.AppendLine($"Event: {SyncSafetyPreparationEvent}");
        builder.AppendLine($"Status: {status}");
        builder.AppendLine($"Reason: {reason}");
        builder.AppendLine($"RequiredChecks: {RequiredChecksText}");
        builder.AppendLine("Mode: sync-safe guard baseline only; diagnostic only; manual review only; report-only.");
        builder.AppendLine("Safety: does not auto-correct; no inventory mutation; no schema change; no checkout changes; no sync changes.");
        builder.AppendLine("Execution: future reconciliation remains blocked until an authorized execution phase.");
        return builder.ToString();
    }
}
