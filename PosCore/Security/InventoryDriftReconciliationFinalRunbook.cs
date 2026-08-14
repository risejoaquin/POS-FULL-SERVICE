namespace PosCore.Security;

/// <summary>
/// Final operational runbook contract for the inventory drift reconciliation block.
/// This is runbook closure only and does not execute reconciliation.
/// </summary>
public static class InventoryDriftReconciliationFinalRunbook
{
    public const string RunbookBaselineName = "inventory.drift.reconciliation.final.runbook.operational.closure";

    public static readonly string[] OperationalClosureChecklist =
    {
        "drift diagnostic executed",
        "manual review completed",
        "controlled reconciliation design ready",
        "RBAC permission guard passed",
        "audit trail prepared",
        "sync-safe guard prepared",
        "controlled execution design ready",
        "exported evidence archived",
        "physical count confirmation captured",
        "operator final confirmation required",
        "rollback decision documented",
        "no inventory mutation",
        "no schema change",
        "no checkout changes",
        "no sync changes"
    };

    public static string OperationalClosureChecklistText => string.Join("; ", OperationalClosureChecklist);

    public static bool HasRequiredClosureState(
        bool hasDrift,
        bool manualReviewRequired,
        bool controlledDesignReady,
        bool permissionReady,
        bool auditReady,
        bool syncSafetyReady,
        bool executionDesignReady)
    {
        return hasDrift
            && manualReviewRequired
            && controlledDesignReady
            && permissionReady
            && auditReady
            && syncSafetyReady
            && executionDesignReady;
    }

    public static string BuildClosureStatus(bool ready)
    {
        return ready
            ? "Runbook operativo final preparado"
            : "Runbook operativo final bloqueado por prerrequisitos";
    }

    public static string BuildClosureSummary(
        string diagnosticsStatus,
        string manualReviewStatus,
        string executionDesignStatus,
        string syncSafetyStatus,
        string auditStatus,
        string evidencePath,
        DateTime reviewedAt)
    {
        var evidence = string.IsNullOrWhiteSpace(evidencePath) ? "evidencia exportada pendiente" : evidencePath;

        return $"Final Runbook & Operational Closure: diagnostics={diagnosticsStatus}; manualReview={manualReviewStatus}; executionDesign={executionDesignStatus}; syncSafety={syncSafetyStatus}; audit={auditStatus}; evidence={evidence}; reviewedAt={reviewedAt:O}; runbook closure only; does not execute real reconciliation; no inventory mutation; no schema change; no checkout changes; no sync changes.";
    }
}
