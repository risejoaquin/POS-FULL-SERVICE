using System;
using System.Collections.Generic;

namespace PosCore.Security;

/// <summary>
/// PHASE 5I baseline contract for POS production sync operational runbook and support handoff.
/// This class is documentation/guardrail only: it does not execute production sync, does not write queue entries,
/// does not trigger support handoff, does not change runtime operations, does not commit checkpoints,
/// does not mutate inventory, does not change checkout, and does not change schema or migrations.
/// </summary>
public static class PosProductionSyncOperationalRunbookSupportHandoffBaseline
{
    public const string BaselineName = "POS Production Sync Operational Runbook & Support Handoff Baseline";
    public const string Scope = "production sync operational runbook and support handoff baseline only";

    public const string OperationalRunbookRequirement = "operational runbook documented";
    public const string SupportHandoffRequirement = "support handoff workflow documented";
    public const string IncidentSeverityRequirement = "incident severity classification documented";
    public const string FirstResponseRequirement = "first response checklist documented";
    public const string EscalationMatrixRequirement = "escalation matrix documented";
    public const string EvidencePackageRequirement = "support evidence package documented";
    public const string QueueSnapshotRequirement = "queue snapshot evidence documented";
    public const string RuntimeMetricsRequirement = "runtime metrics evidence documented";
    public const string CorrelationIdRequirement = "correlation id evidence documented";
    public const string TenantDeviceRequirement = "tenant/device evidence documented";
    public const string IdempotencyKeyRequirement = "idempotency key evidence documented";
    public const string CheckpointStateRequirement = "checkpoint state evidence documented";
    public const string FeatureFlagStateRequirement = "feature flag state evidence documented";
    public const string KillSwitchStateRequirement = "kill switch state evidence documented";
    public const string DeadLetterStateRequirement = "dead-letter state evidence documented";
    public const string OperatorCommunicationRequirement = "operator communication template documented";
    public const string ClosureCriteriaRequirement = "support closure criteria documented";
    public const string OperatorSafeRunbookMessageRequirement = "operator-safe runbook message documented";

    public const string NoProductionSyncExecution = "no production sync execution";
    public const string NoQueueWrites = "no queue writes";
    public const string NoSupportHandoffExecution = "no support handoff execution";
    public const string NoRuntimeOperationChange = "no runtime operation change";
    public const string NoCheckpointCommit = "no checkpoint commit";
    public const string NoCheckoutChanges = "no checkout changes";
    public const string NoInventoryMutation = "no inventory mutation";
    public const string NoSchemaChange = "no schema change";
    public const string NoMigrations = "no migrations";

    public static readonly IReadOnlyList<string> RequiredOperationalRunbookSupportHandoffChecks = new[]
    {
        OperationalRunbookRequirement,
        SupportHandoffRequirement,
        IncidentSeverityRequirement,
        FirstResponseRequirement,
        EscalationMatrixRequirement,
        EvidencePackageRequirement,
        QueueSnapshotRequirement,
        RuntimeMetricsRequirement,
        CorrelationIdRequirement,
        TenantDeviceRequirement,
        IdempotencyKeyRequirement,
        CheckpointStateRequirement,
        FeatureFlagStateRequirement,
        KillSwitchStateRequirement,
        DeadLetterStateRequirement,
        OperatorCommunicationRequirement,
        ClosureCriteriaRequirement,
        OperatorSafeRunbookMessageRequirement,
        NoProductionSyncExecution,
        NoQueueWrites,
        NoSupportHandoffExecution,
        NoRuntimeOperationChange,
        NoCheckpointCommit,
        NoCheckoutChanges,
        NoInventoryMutation,
        NoSchemaChange,
        NoMigrations
    };

    public static string RequiredOperationalRunbookSupportHandoffText =>
        string.Join("; ", RequiredOperationalRunbookSupportHandoffChecks);

    public static bool HasMinimumOperationalRunbookSupportHandoffDesign(
        bool hasOperationalRunbook,
        bool hasSupportHandoffWorkflow,
        bool hasIncidentSeverityClassification,
        bool hasFirstResponseChecklist,
        bool hasEscalationMatrix,
        bool hasEvidencePackage,
        bool hasOperatorCommunicationTemplate,
        bool hasClosureCriteria)
    {
        return hasOperationalRunbook
            && hasSupportHandoffWorkflow
            && hasIncidentSeverityClassification
            && hasFirstResponseChecklist
            && hasEscalationMatrix
            && hasEvidencePackage
            && hasOperatorCommunicationTemplate
            && hasClosureCriteria;
    }

    public static string BuildOperationalRunbookSupportHandoffSummary(
        bool hasOperationalRunbook,
        bool hasSupportHandoffWorkflow,
        bool hasIncidentSeverityClassification,
        bool hasFirstResponseChecklist,
        bool hasEscalationMatrix,
        bool hasEvidencePackage,
        bool hasOperatorCommunicationTemplate,
        bool hasClosureCriteria,
        DateTime reviewedAt)
    {
        var readiness = HasMinimumOperationalRunbookSupportHandoffDesign(
            hasOperationalRunbook,
            hasSupportHandoffWorkflow,
            hasIncidentSeverityClassification,
            hasFirstResponseChecklist,
            hasEscalationMatrix,
            hasEvidencePackage,
            hasOperatorCommunicationTemplate,
            hasClosureCriteria)
                ? "READY"
                : "BLOCKED";

        return $"{BaselineName}: {readiness}. ReviewedAt={reviewedAt:O}. " +
               "Operational runbook, support handoff workflow, incident severity, first response, escalation matrix, evidence package, queue snapshot, runtime metrics, correlation id, tenant/device, idempotency key, checkpoint state, feature flag state, kill switch state, dead-letter state, operator communication template and closure criteria reviewed. " +
               "No production sync execution, no queue writes, no support handoff execution, no runtime operation change, no checkpoint commit, no checkout changes, no inventory mutation, no schema change, no migrations.";
    }
}
