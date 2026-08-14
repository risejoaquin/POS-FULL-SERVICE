namespace PosCore.Security;

/// <summary>
/// PHASE 9B - Installer Package Generation Execution.
/// Defines controlled installer package generation evidence from PHASE 9A published artifacts without deployment, checkout, inventory, production sync, schema, or migration changes.
/// </summary>
public static class PosInstallerPackageGenerationExecution
{
    public const string ExecutionName = "POS Installer Package Generation Execution";

    public static readonly string[] RequiredInstallerPackageGenerationChecks =
    {
        "installer package generation execution documented",
        "PHASE 9A release artifact execution prerequisite documented",
        "445 tests passed source evidence documented",
        "450 tests expected after installer package generation execution documented",
        "published artifact source directory documented",
        "installer package staging directory documented",
        "PosCore published artifact input documented",
        "PosBuilder published artifact input documented",
        "PosServer published artifact input documented",
        "release manifest input documented",
        "checksums input documented",
        "installer package manifest generation documented",
        "installer package checksum generation documented",
        "installer package zip archive generation documented",
        "installer package output naming convention documented",
        "installer package verification command documented",
        "operator package generation command documented",
        "package failure handling checklist documented",
        "no checkout behavior change",
        "no inventory mutation",
        "no production sync enablement",
        "no deployment execution",
        "no public API behavior change",
        "no schema change",
        "no migrations"
    };

    public static string RequiredInstallerPackageGenerationText => string.Join("; ", RequiredInstallerPackageGenerationChecks);

    public sealed record InstallerPackageGenerationEvidence(
        string Scope,
        string Phase9APrerequisiteEvidence,
        string PackageGenerationEvidence,
        string PackageVerificationEvidence,
        string SafetyStatement);

    public static bool HasMinimumInstallerPackageGenerationExecutionReadiness(
        bool hasPhase9AReleaseArtifacts,
        bool hasPublishedArtifactSourceDirectory,
        bool hasInstallerPackageStagingDirectory,
        bool hasPosCoreArtifactInput,
        bool hasPosBuilderArtifactInput,
        bool hasPosServerArtifactInput,
        bool hasReleaseManifestInput,
        bool hasChecksumInput,
        bool hasInstallerPackageManifestGeneration,
        bool hasInstallerPackageChecksumGeneration,
        bool hasInstallerPackageZipArchiveGeneration,
        bool hasInstallerPackageOutputNamingConvention,
        bool hasInstallerPackageVerificationCommand,
        bool hasOperatorPackageGenerationCommand,
        bool hasPackageFailureHandling,
        bool hasNoDeploymentExecution,
        bool hasNoBusinessLogicChange,
        bool hasNoSchemaChange)
    {
        return hasPhase9AReleaseArtifacts
            && hasPublishedArtifactSourceDirectory
            && hasInstallerPackageStagingDirectory
            && hasPosCoreArtifactInput
            && hasPosBuilderArtifactInput
            && hasPosServerArtifactInput
            && hasReleaseManifestInput
            && hasChecksumInput
            && hasInstallerPackageManifestGeneration
            && hasInstallerPackageChecksumGeneration
            && hasInstallerPackageZipArchiveGeneration
            && hasInstallerPackageOutputNamingConvention
            && hasInstallerPackageVerificationCommand
            && hasOperatorPackageGenerationCommand
            && hasPackageFailureHandling
            && hasNoDeploymentExecution
            && hasNoBusinessLogicChange
            && hasNoSchemaChange;
    }

    public static InstallerPackageGenerationEvidence BuildInstallerPackageGenerationExecutionEvidence()
    {
        return new InstallerPackageGenerationEvidence(
            "PHASE 9B installer package generation execution only",
            "PHASE 9A closed with 445 tests passed, zero warnings, zero errors, release-manifest.json generated, and checksums.sha256 generated",
            "Generate-Phase9InstallerPackage.ps1 packages published artifacts from artifacts/release/phase9/publish into artifacts/release/phase9/installer",
            "installer-package-manifest.json, installer-checksums.sha256, and a zip archive are generated for operator review",
            "No checkout behavior change; no inventory mutation; no production sync enablement; no deployment execution; no public API behavior change; no schema change; no migrations");
    }

    public static string BuildInstallerPackageGenerationExecutionSummary(bool ready)
    {
        var status = ready ? "READY" : "BLOCKED";
        return $"PHASE 9B installer package generation execution readiness: {status}. {RequiredInstallerPackageGenerationText}";
    }
}
