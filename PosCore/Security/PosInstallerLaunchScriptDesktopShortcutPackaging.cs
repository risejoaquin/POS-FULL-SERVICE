namespace PosCore.Security;

/// <summary>
/// PHASE 9E - Installer Launch Script and Desktop Shortcut Packaging.
/// Defines controlled launch script and desktop shortcut packaging evidence using generated installer package contents only; no real installer execution, no shortcut creation on the operator machine, deployment, checkout, inventory, production sync, schema, or migration changes.
/// </summary>
public static class PosInstallerLaunchScriptDesktopShortcutPackaging
{
    public const string ExecutionName = "POS Installer Launch Script and Desktop Shortcut Packaging";

    public static readonly string[] RequiredLaunchPackagingChecks =
    {
        "installer launch script desktop shortcut packaging documented",
        "PHASE 9D smoke install simulation prerequisite documented",
        "460 tests passed source evidence documented",
        "465 tests expected after installer launch script desktop shortcut packaging documented",
        "launch script staging directory documented",
        "PosCore launch script packaging documented",
        "PosBuilder launch script packaging documented",
        "PosServer launch script packaging documented",
        "desktop shortcut specification packaging documented",
        "desktop shortcut creation script packaged but not executed",
        "launcher manifest generation documented",
        "launcher checksum generation documented",
        "launch package archive generation documented",
        "launch package content verification documented",
        "operator launch packaging command documented",
        "launch packaging failure handling documented",
        "no real shortcut creation",
        "no real installer execution",
        "no checkout behavior change",
        "no inventory mutation",
        "no production sync enablement",
        "no deployment execution",
        "no public API behavior change",
        "no schema change",
        "no migrations"
    };

    public static string RequiredLaunchPackagingText => string.Join("; ", RequiredLaunchPackagingChecks);

    public sealed record InstallerLaunchPackagingEvidence(
        string Scope,
        string Phase9DPrerequisiteEvidence,
        string LaunchScriptsEvidence,
        string DesktopShortcutEvidence,
        string SafetyStatement);

    public static bool HasMinimumInstallerLaunchPackagingReadiness(
        bool hasPhase9DSmokeInstallSimulation,
        bool hasLaunchStagingDirectory,
        bool hasPosCoreLaunchScript,
        bool hasPosBuilderLaunchScript,
        bool hasPosServerLaunchScript,
        bool hasDesktopShortcutSpec,
        bool hasShortcutCreationScriptPackagedOnly,
        bool hasLauncherManifest,
        bool hasLauncherChecksums,
        bool hasLaunchPackageArchive,
        bool hasLaunchPackageContentVerification,
        bool hasOperatorCommand,
        bool hasFailureHandling,
        bool hasNoRealShortcutCreation,
        bool hasNoRealInstallerExecution,
        bool hasNoDeploymentExecution,
        bool hasNoBusinessLogicChange,
        bool hasNoSchemaChange)
    {
        return hasPhase9DSmokeInstallSimulation
            && hasLaunchStagingDirectory
            && hasPosCoreLaunchScript
            && hasPosBuilderLaunchScript
            && hasPosServerLaunchScript
            && hasDesktopShortcutSpec
            && hasShortcutCreationScriptPackagedOnly
            && hasLauncherManifest
            && hasLauncherChecksums
            && hasLaunchPackageArchive
            && hasLaunchPackageContentVerification
            && hasOperatorCommand
            && hasFailureHandling
            && hasNoRealShortcutCreation
            && hasNoRealInstallerExecution
            && hasNoDeploymentExecution
            && hasNoBusinessLogicChange
            && hasNoSchemaChange;
    }

    public static InstallerLaunchPackagingEvidence BuildInstallerLaunchPackagingEvidence()
    {
        return new InstallerLaunchPackagingEvidence(
            "PHASE 9E installer launch script and desktop shortcut packaging only",
            "PHASE 9D closed with 460 tests passed, zero warnings, zero errors, installer smoke install simulation verified, smoke evidence generated, and extracted files validated",
            "Generate-Phase9LaunchAndShortcutPackage.ps1 stages Start-PosCore.ps1, Start-PosBuilder.ps1, Start-PosServer.ps1, and a launch package archive under artifacts/release/phase9/installer",
            "desktop-shortcut-spec.json and Create-DesktopShortcuts.ps1 are packaged as operator-reviewed inputs only; no real shortcut is created during PHASE 9E",
            "No real shortcut creation; no real installer execution; no checkout behavior change; no inventory mutation; no production sync enablement; no deployment execution; no public API behavior change; no schema change; no migrations");
    }

    public static string BuildInstallerLaunchPackagingSummary(bool ready)
    {
        var status = ready ? "READY" : "BLOCKED";
        return $"PHASE 9E installer launch script desktop shortcut packaging readiness: {status}. {RequiredLaunchPackagingText}";
    }
}
