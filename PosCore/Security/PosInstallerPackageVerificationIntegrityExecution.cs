namespace PosCore.Security;

/// <summary>
/// PHASE 9C - Installer Package Verification and Integrity Execution.
/// Defines controlled installer package verification evidence for PHASE 9B package outputs without deployment, checkout, inventory, production sync, schema, or migration changes.
/// </summary>
public static class PosInstallerPackageVerificationIntegrityExecution
{
    public const string ExecutionName = "POS Installer Package Verification and Integrity Execution";

    public static readonly string[] RequiredInstallerPackageVerificationChecks =
    {
        "installer package verification integrity execution documented",
        "PHASE 9B installer package generation prerequisite documented",
        "450 tests passed source evidence documented",
        "455 tests expected after installer package verification integrity execution documented",
        "installer package archive existence verification documented",
        "installer package manifest existence verification documented",
        "installer package checksum manifest existence verification documented",
        "installer package archive SHA-256 verification documented",
        "installer package manifest SHA-256 cross-check documented",
        "installer package unzip verification documented",
        "PosCore package content verification documented",
        "PosBuilder package content verification documented",
        "PosServer package content verification documented",
        "release manifest packaged content verification documented",
        "checksums packaged content verification documented",
        "installer package tamper detection documented",
        "operator package verification command documented",
        "package verification failure handling checklist documented",
        "no checkout behavior change",
        "no inventory mutation",
        "no production sync enablement",
        "no deployment execution",
        "no public API behavior change",
        "no schema change",
        "no migrations"
    };

    public static string RequiredInstallerPackageVerificationText => string.Join("; ", RequiredInstallerPackageVerificationChecks);

    public sealed record InstallerPackageVerificationEvidence(
        string Scope,
        string Phase9BPrerequisiteEvidence,
        string ArchiveIntegrityEvidence,
        string ContentIntegrityEvidence,
        string SafetyStatement);

    public static bool HasMinimumInstallerPackageVerificationIntegrityReadiness(
        bool hasPhase9BInstallerPackage,
        bool hasPackageArchive,
        bool hasInstallerPackageManifest,
        bool hasInstallerChecksumManifest,
        bool hasArchiveSha256Verification,
        bool hasManifestSha256CrossCheck,
        bool hasUnzipVerification,
        bool hasPosCoreContentVerification,
        bool hasPosBuilderContentVerification,
        bool hasPosServerContentVerification,
        bool hasReleaseManifestContentVerification,
        bool hasChecksumsContentVerification,
        bool hasTamperDetection,
        bool hasOperatorVerificationCommand,
        bool hasVerificationFailureHandling,
        bool hasNoDeploymentExecution,
        bool hasNoBusinessLogicChange,
        bool hasNoSchemaChange)
    {
        return hasPhase9BInstallerPackage
            && hasPackageArchive
            && hasInstallerPackageManifest
            && hasInstallerChecksumManifest
            && hasArchiveSha256Verification
            && hasManifestSha256CrossCheck
            && hasUnzipVerification
            && hasPosCoreContentVerification
            && hasPosBuilderContentVerification
            && hasPosServerContentVerification
            && hasReleaseManifestContentVerification
            && hasChecksumsContentVerification
            && hasTamperDetection
            && hasOperatorVerificationCommand
            && hasVerificationFailureHandling
            && hasNoDeploymentExecution
            && hasNoBusinessLogicChange
            && hasNoSchemaChange;
    }

    public static InstallerPackageVerificationEvidence BuildInstallerPackageVerificationIntegrityEvidence()
    {
        return new InstallerPackageVerificationEvidence(
            "PHASE 9C installer package verification and integrity execution only",
            "PHASE 9B closed with 450 tests passed, zero warnings, zero errors, installer package zip generated, installer manifest generated, and installer checksums generated",
            "Verify-Phase9InstallerPackageIntegrity.ps1 validates package existence, manifest existence, checksum manifest existence, and package archive SHA-256 against installer-package-manifest.json",
            "The verification script expands the installer package and verifies required package contents: poscore-win-x64, posbuilder-win-x64, posserver, release-manifest.json, checksums.sha256, and installer-checksums.sha256",
            "No checkout behavior change; no inventory mutation; no production sync enablement; no deployment execution; no public API behavior change; no schema change; no migrations");
    }

    public static string BuildInstallerPackageVerificationIntegritySummary(bool ready)
    {
        var status = ready ? "READY" : "BLOCKED";
        return $"PHASE 9C installer package verification integrity execution readiness: {status}. {RequiredInstallerPackageVerificationText}";
    }
}
