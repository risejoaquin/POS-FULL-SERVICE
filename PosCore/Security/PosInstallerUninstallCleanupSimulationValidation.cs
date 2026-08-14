namespace PosCore.Security;

/// <summary>
/// PHASE 9F - Installer Uninstall and Cleanup Simulation Validation.
/// Defines a dry-run uninstall and cleanup simulation evidence workflow only; no real deletion, no Windows registry changes, no Desktop mutation, no Program Files mutation, no installer execution, deployment, checkout, inventory, production sync, schema, or migration changes.
/// </summary>
public static class PosInstallerUninstallCleanupSimulationValidation
{
    public const string ExecutionName = "POS Installer Uninstall and Cleanup Simulation Validation";

    public static readonly string[] RequiredUninstallCleanupSimulationChecks =
    {
        "installer uninstall cleanup simulation validation documented",
        "PHASE 9E launcher package prerequisite documented",
        "465 tests passed source evidence documented",
        "470 tests expected after installer uninstall cleanup simulation validation documented",
        "uninstall cleanup plan generation documented",
        "uninstall cleanup evidence generation documented",
        "simulated install directory cleanup candidate documented",
        "launcher package directory cleanup candidate documented",
        "desktop shortcut candidate cleanup documented",
        "temporary verification directory cleanup candidate documented",
        "generated installer artifacts preservation documented",
        "release manifests preservation documented",
        "checksums preservation documented",
        "audit evidence preservation documented",
        "dry run only cleanup documented",
        "no real file deletion",
        "no real shortcut deletion",
        "no Program Files mutation",
        "no Desktop mutation",
        "no Windows registry mutation",
        "no real installer execution",
        "no checkout behavior change",
        "no inventory mutation",
        "no production sync enablement",
        "no deployment execution",
        "no public API behavior change",
        "no schema change",
        "no migrations"
    };

    public static string RequiredUninstallCleanupSimulationText => string.Join("; ", RequiredUninstallCleanupSimulationChecks);

    public sealed record InstallerUninstallCleanupSimulationEvidence(
        string Scope,
        string Phase9EPrerequisiteEvidence,
        string CleanupPlanEvidence,
        string PreservationEvidence,
        string SafetyStatement);

    public static bool HasMinimumInstallerUninstallCleanupSimulationReadiness(
        bool hasPhase9ELauncherPackage,
        bool hasSmokeInstallEvidence,
        bool hasLauncherManifest,
        bool hasDesktopShortcutSpec,
        bool hasCleanupPlan,
        bool hasCleanupEvidence,
        bool hasSimulatedInstallCleanupCandidates,
        bool hasLauncherCleanupCandidates,
        bool hasDesktopShortcutCandidates,
        bool hasTemporaryVerificationCandidates,
        bool hasPreservedManifests,
        bool hasPreservedChecksums,
        bool hasPreservedAuditEvidence,
        bool hasDryRunOnly,
        bool hasNoRealFileDeletion,
        bool hasNoDesktopMutation,
        bool hasNoProgramFilesMutation,
        bool hasNoRegistryMutation,
        bool hasNoInstallerExecution,
        bool hasNoDeploymentExecution,
        bool hasNoBusinessLogicChange,
        bool hasNoSchemaChange)
    {
        return hasPhase9ELauncherPackage
            && hasSmokeInstallEvidence
            && hasLauncherManifest
            && hasDesktopShortcutSpec
            && hasCleanupPlan
            && hasCleanupEvidence
            && hasSimulatedInstallCleanupCandidates
            && hasLauncherCleanupCandidates
            && hasDesktopShortcutCandidates
            && hasTemporaryVerificationCandidates
            && hasPreservedManifests
            && hasPreservedChecksums
            && hasPreservedAuditEvidence
            && hasDryRunOnly
            && hasNoRealFileDeletion
            && hasNoDesktopMutation
            && hasNoProgramFilesMutation
            && hasNoRegistryMutation
            && hasNoInstallerExecution
            && hasNoDeploymentExecution
            && hasNoBusinessLogicChange
            && hasNoSchemaChange;
    }

    public static InstallerUninstallCleanupSimulationEvidence BuildInstallerUninstallCleanupSimulationEvidence()
    {
        return new InstallerUninstallCleanupSimulationEvidence(
            "PHASE 9F installer uninstall and cleanup simulation validation only",
            "PHASE 9E closed with 465 tests passed, zero warnings, zero errors, launcher package generated, desktop shortcut specification packaged, and four launch scripts packaged",
            "Simulate-Phase9InstallerUninstallCleanup.ps1 generates uninstall-cleanup-plan.json and uninstall-cleanup-evidence.json under artifacts/release/phase9/uninstall-simulation",
            "Release manifests, checksums, installer packages, launcher package evidence, smoke install evidence, and audit evidence are preserved as immutable release evidence",
            "Dry run only; no real file deletion; no real shortcut deletion; no Program Files mutation; no Desktop mutation; no Windows registry mutation; no real installer execution; no deployment execution; no checkout behavior change; no inventory mutation; no production sync enablement; no public API behavior change; no schema change; no migrations");
    }

    public static string BuildInstallerUninstallCleanupSimulationSummary(bool ready)
    {
        var status = ready ? "READY" : "BLOCKED";
        return $"PHASE 9F installer uninstall cleanup simulation validation readiness: {status}. {RequiredUninstallCleanupSimulationText}";
    }
}
