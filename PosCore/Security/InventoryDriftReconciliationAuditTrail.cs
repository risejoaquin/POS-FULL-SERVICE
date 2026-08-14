using System;
using System.Linq;
using System.Text;

namespace PosCore.Security;

/// <summary>
/// Audit vocabulary and preparation helpers for future controlled inventory drift reconciliation.
/// This file defines an audit trail contract only; it does not persist audit records and does not apply stock adjustments.
/// </summary>
public static class InventoryDriftReconciliationAuditTrail
{
    public const string AuditBaselineName = "inventory.drift.reconciliation.audit-trail.baseline";
    public const string AuditPreparationEvent = "inventory.drift.reconciliation.audit.prepare";
    public const string AuditFutureExecutionEvent = "inventory.drift.reconciliation.execute.future";

    public static readonly string[] RequiredAuditFields =
    {
        "tenant_id",
        "user_id",
        "username",
        "role",
        "required_permission",
        "diagnostic_status",
        "manual_review_status",
        "design_status",
        "exported_evidence_path",
        "physical_count_confirmation",
        "reason",
        "sync_safety_decision",
        "before_operational_quantity",
        "before_ledger_quantity",
        "proposed_target_quantity",
        "result_status"
    };

    public static string RequiredFieldsText => string.Join(", ", RequiredAuditFields);

    public static bool HasMinimumPreparationEvidence(
        bool hasDrift,
        bool manualReviewRequired,
        bool designReady,
        bool permissionGranted,
        string? exportedEvidencePath)
    {
        return hasDrift
            && manualReviewRequired
            && designReady
            && permissionGranted
            && !string.IsNullOrWhiteSpace(exportedEvidencePath);
    }

    public static string BuildPreparationSummary(
        string diagnosticStatus,
        string manualReviewStatus,
        string designStatus,
        string requiredPermission,
        string role,
        string userId,
        string username,
        string tenantId,
        string exportedEvidencePath,
        DateTime preparedAt)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Inventory Drift Reconciliation Audit Trail Baseline");
        builder.AppendLine($"PreparedAt: {preparedAt:yyyy-MM-dd HH:mm:ss}");
        builder.AppendLine($"AuditEvent: {AuditPreparationEvent}");
        builder.AppendLine($"RequiredPermission: {requiredPermission}");
        builder.AppendLine($"Role: {role}");
        builder.AppendLine($"UserId: {userId}");
        builder.AppendLine($"Username: {username}");
        builder.AppendLine($"TenantId: {tenantId}");
        builder.AppendLine($"DiagnosticStatus: {diagnosticStatus}");
        builder.AppendLine($"ManualReviewStatus: {manualReviewStatus}");
        builder.AppendLine($"DesignStatus: {designStatus}");
        builder.AppendLine($"ExportedEvidencePath: {exportedEvidencePath}");
        builder.AppendLine($"RequiredFields: {RequiredFieldsText}");
        builder.AppendLine("Mode: audit trail baseline only; diagnostic only; manual review only; report-only.");
        builder.AppendLine("Safety: does not auto-correct; no inventory mutation; no schema change; no checkout changes; no sync changes.");
        builder.AppendLine("Storage: persistence contract only; no audit storage is written in this phase.");
        return builder.ToString();
    }

    public static string BuildRequiredFieldsChecklist()
    {
        return string.Join(Environment.NewLine, RequiredAuditFields.Select(field => $"- {field}"));
    }
}
