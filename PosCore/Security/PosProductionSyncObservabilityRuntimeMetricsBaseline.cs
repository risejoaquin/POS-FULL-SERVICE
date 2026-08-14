using System;
using System.Collections.Generic;

namespace PosCore.Security;

/// <summary>
/// PHASE 5H baseline contract for POS production sync observability runtime metrics.
/// This class is documentation/guardrail only: it does not execute production sync, does not write queue entries,
/// does not emit runtime metrics, does not change alerting configuration, does not commit checkpoints,
/// does not mutate inventory, does not change checkout, and does not change schema or migrations.
/// </summary>
public static class PosProductionSyncObservabilityRuntimeMetricsBaseline
{
    public const string BaselineName = "POS Production Sync Observability Runtime Metrics Baseline";
    public const string Scope = "production sync observability runtime metrics baseline only";

    public const string RuntimeMetricsContractRequirement = "runtime metrics contract documented";
    public const string QueueDepthMetricRequirement = "queue depth metric documented";
    public const string ProcessingLatencyMetricRequirement = "processing latency metric documented";
    public const string AcknowledgementLatencyMetricRequirement = "acknowledgement latency metric documented";
    public const string CheckpointLagMetricRequirement = "checkpoint lag metric documented";
    public const string RetryRateMetricRequirement = "retry rate metric documented";
    public const string DeadLetterRateMetricRequirement = "dead-letter rate metric documented";
    public const string ConflictRateMetricRequirement = "conflict rate metric documented";
    public const string ErrorRateMetricRequirement = "error rate metric documented";
    public const string ThroughputMetricRequirement = "sync throughput metric documented";
    public const string TenantDeviceDimensionsRequirement = "tenant/device metric dimensions documented";
    public const string CorrelationIdTraceRequirement = "correlation id trace metric documented";
    public const string RedactionRequirement = "sensitive data redaction documented";
    public const string AlertThresholdRequirement = "alert threshold requirement documented";
    public const string DashboardRequirement = "operator dashboard requirement documented";
    public const string OperatorSafeMetricsMessageRequirement = "operator-safe metrics message documented";

    public const string NoProductionSyncExecution = "no production sync execution";
    public const string NoQueueWrites = "no queue writes";
    public const string NoRuntimeMetricsEmission = "no runtime metrics emission";
    public const string NoAlertingConfigurationChange = "no alerting configuration change";
    public const string NoCheckpointCommit = "no checkpoint commit";
    public const string NoCheckoutChanges = "no checkout changes";
    public const string NoInventoryMutation = "no inventory mutation";
    public const string NoSchemaChange = "no schema change";
    public const string NoMigrations = "no migrations";

    public static readonly IReadOnlyList<string> RequiredObservabilityRuntimeMetricsChecks = new[]
    {
        RuntimeMetricsContractRequirement,
        QueueDepthMetricRequirement,
        ProcessingLatencyMetricRequirement,
        AcknowledgementLatencyMetricRequirement,
        CheckpointLagMetricRequirement,
        RetryRateMetricRequirement,
        DeadLetterRateMetricRequirement,
        ConflictRateMetricRequirement,
        ErrorRateMetricRequirement,
        ThroughputMetricRequirement,
        TenantDeviceDimensionsRequirement,
        CorrelationIdTraceRequirement,
        RedactionRequirement,
        AlertThresholdRequirement,
        DashboardRequirement,
        OperatorSafeMetricsMessageRequirement,
        NoProductionSyncExecution,
        NoQueueWrites,
        NoRuntimeMetricsEmission,
        NoAlertingConfigurationChange,
        NoCheckpointCommit,
        NoCheckoutChanges,
        NoInventoryMutation,
        NoSchemaChange,
        NoMigrations
    };

    public static string RequiredObservabilityRuntimeMetricsText =>
        string.Join("; ", RequiredObservabilityRuntimeMetricsChecks);

    public static bool HasMinimumObservabilityRuntimeMetricsDesign(
        bool hasRuntimeMetricsContract,
        bool hasQueueDepthMetric,
        bool hasLatencyMetrics,
        bool hasCheckpointLagMetric,
        bool hasFailureRateMetrics,
        bool hasTenantDeviceDimensions,
        bool hasRedactionRequirement,
        bool hasAlertThresholdRequirement)
    {
        return hasRuntimeMetricsContract
            && hasQueueDepthMetric
            && hasLatencyMetrics
            && hasCheckpointLagMetric
            && hasFailureRateMetrics
            && hasTenantDeviceDimensions
            && hasRedactionRequirement
            && hasAlertThresholdRequirement;
    }

    public static string BuildObservabilityRuntimeMetricsSummary(
        bool hasRuntimeMetricsContract,
        bool hasQueueDepthMetric,
        bool hasLatencyMetrics,
        bool hasCheckpointLagMetric,
        bool hasFailureRateMetrics,
        bool hasTenantDeviceDimensions,
        bool hasRedactionRequirement,
        bool hasAlertThresholdRequirement,
        DateTime reviewedAt)
    {
        var readiness = HasMinimumObservabilityRuntimeMetricsDesign(
            hasRuntimeMetricsContract,
            hasQueueDepthMetric,
            hasLatencyMetrics,
            hasCheckpointLagMetric,
            hasFailureRateMetrics,
            hasTenantDeviceDimensions,
            hasRedactionRequirement,
            hasAlertThresholdRequirement)
                ? "READY"
                : "BLOCKED";

        return $"{BaselineName}: {readiness}. ReviewedAt={reviewedAt:O}. " +
               "Runtime metrics contract, queue depth, processing latency, acknowledgement latency, checkpoint lag, retry/dead-letter/conflict/error rates, throughput, tenant/device dimensions, correlation trace, redaction, alert thresholds and operator dashboard reviewed. " +
               "No production sync execution, no queue writes, no runtime metrics emission, no alerting configuration change, no checkpoint commit, no checkout changes, no inventory mutation, no schema change, no migrations.";
    }
}
