using System;
using System.Linq;

namespace PosCore.Security;

/// <summary>
/// Design-only helper for a future controlled inventory drift reconciliation execution.
/// This helper defines prerequisites and plan text only. It does not mutate inventory.
/// </summary>
public static class InventoryDriftControlledReconciliationExecutionDesign
{
    public const string ExecutionDesignBaselineName = "inventory.drift.controlled.reconciliation.execution.design.baseline";
    public const string ExecutionDesignEvent = "inventory.drift.controlled.reconciliation.execution.design.prepared";

    public static readonly string[] RequiredExecutionPreconditions =
    {
        "drift confirmed",
        "manual review required",
        "controlled reconciliation design ready",
        "RBAC permission guard passed",
        "audit trail prepared",
        "sync-safe guard prepared",
        "exported evidence linked",
        "physical count confirmation required",
        "reason required",
        "operator final confirmation required",
        "dry-run calculation required"
    };

    public static string RequiredExecutionPreconditionsText => string.Join("; ", RequiredExecutionPreconditions);

    public static bool HasRequiredPreparationState(
        bool hasInventoryDrift,
        bool manualReviewRequired,
        bool controlledDesignReady,
        bool permissionGuardReady,
        bool auditTrailReady,
        bool syncSafetyReady)
    {
        return hasInventoryDrift
            && manualReviewRequired
            && controlledDesignReady
            && permissionGuardReady
            && auditTrailReady
            && syncSafetyReady;
    }

    public static string BuildExecutionDesignChecklist()
    {
        return string.Join(Environment.NewLine, RequiredExecutionPreconditions.Select(item => $"- {item}"));
    }

    public static string BuildExecutionPlanSummary(
        string status,
        string requiredPermission,
        string role,
        string exportPath,
        DateTime reviewedAt)
    {
        return $"Execution design status: {status}{Environment.NewLine}"
            + $"Required permission: {requiredPermission}{Environment.NewLine}"
            + $"Current role: {role}{Environment.NewLine}"
            + $"Exported evidence: {(string.IsNullOrWhiteSpace(exportPath) ? "pending" : exportPath)}{Environment.NewLine}"
            + $"Reviewed at: {reviewedAt:O}{Environment.NewLine}"
            + "Execution mode: design only; no inventory mutation; no schema change; no checkout changes; no sync changes.";
    }

    public static string BuildBlockedReason(
        bool hasInventoryDrift,
        bool manualReviewRequired,
        bool controlledDesignReady,
        bool permissionGuardReady,
        bool auditTrailReady,
        bool syncSafetyReady)
    {
        return $"Blocked prerequisites -> drift={hasInventoryDrift}, manualReview={manualReviewRequired}, controlledDesign={controlledDesignReady}, permissionGuard={permissionGuardReady}, auditTrail={auditTrailReady}, syncSafety={syncSafetyReady}.";
    }
}
