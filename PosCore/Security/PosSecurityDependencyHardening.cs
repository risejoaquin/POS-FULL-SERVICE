using System;

namespace PosCore.Security;

/// <summary>
/// PHASE 7A security dependency hardening contract.
/// This phase remediates the known System.Text.Json 8.0.0 vulnerability warning in PosBuilder by pinning the package to a patched .NET 8 compatible version.
/// It does not change checkout behavior, does not change inventory behavior, does not enable production sync, and does not add migrations.
/// </summary>
public static class PosSecurityDependencyHardening
{
    public const string HardeningName = "POS Security Dependency Hardening";

    public const string HardenedPackage = "System.Text.Json";

    public const string PreviousVersion = "8.0.0";

    public const string PatchedVersion = "8.0.5";

    public static readonly string[] RequiredSecurityDependencyHardeningChecks =
    {
        "security dependency hardening documented",
        "System.Text.Json vulnerability remediation documented",
        "System.Text.Json 8.0.0 removed from PosBuilder",
        "System.Text.Json 8.0.5 pinned in PosBuilder",
        "GHSA-8g4q-xg66-9fp4 remediation tracked",
        "GHSA-hh2w-p6rv-4g7w remediation tracked",
        "dependency update scope limited to PosBuilder",
        "no checkout behavior change",
        "no inventory mutation",
        "no production sync enablement",
        "no schema change",
        "no migrations",
        "operator-safe dependency hardening message documented"
    };

    public static string RequiredSecurityDependencyHardeningText => string.Join("; ", RequiredSecurityDependencyHardeningChecks);

    public sealed record DependencyHardeningEvidence(
        string PackageName,
        string PreviousVersion,
        string PatchedVersion,
        string Scope,
        DateTime ReviewedAt,
        string SafetyStatement);

    public static bool HasMinimumSecurityDependencyHardeningReadiness(
        bool hasPatchedVersion,
        bool hasNoVulnerableVersionInPosBuilder,
        bool hasLimitedScope,
        bool hasAdvisoryTracking,
        bool hasNoBusinessLogicChange)
    {
        return hasPatchedVersion
            && hasNoVulnerableVersionInPosBuilder
            && hasLimitedScope
            && hasAdvisoryTracking
            && hasNoBusinessLogicChange;
    }

    public static DependencyHardeningEvidence BuildDependencyHardeningEvidence(DateTime reviewedAt)
    {
        return new DependencyHardeningEvidence(
            HardenedPackage,
            PreviousVersion,
            PatchedVersion,
            "PosBuilder dependency reference only",
            reviewedAt,
            "dependency hardening only - no checkout behavior change, no inventory mutation, no production sync enablement, no schema change, no migrations");
    }

    public static string BuildSecurityDependencyHardeningSummary(bool ready, DateTime reviewedAt)
    {
        var status = ready ? "ready" : "blocked";
        return $"security_dependency_hardening_status={status}; package={HardenedPackage}; previous_version={PreviousVersion}; patched_version={PatchedVersion}; reviewed_at={reviewedAt:O}; no checkout behavior change; no inventory mutation; no production sync enablement; no schema change; no migrations";
    }
}
