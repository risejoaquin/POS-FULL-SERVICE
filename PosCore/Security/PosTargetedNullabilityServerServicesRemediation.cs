using System;

namespace PosCore.Security;

/// <summary>
/// PHASE 7C targeted nullability remediation contract for server services.
/// This phase remediates selected nullable warning hotspots in AuthService, UserService and CentralDbContext without changing business behavior.
/// It does not change checkout behavior, does not mutate inventory, does not enable production sync, does not change schema and does not add migrations.
/// </summary>
public static class PosTargetedNullabilityServerServicesRemediation
{
    public const string RemediationName = "POS Targeted Nullability Remediation: Server Services";

    public static readonly string[] RequiredTargetedNullabilityServerServicesChecks =
    {
        "targeted nullability remediation documented",
        "AuthService nullable password hash guard implemented",
        "AuthService token claim null guard implemented",
        "AuthService provision request null guard implemented",
        "AuthService admin credential null guard implemented",
        "AuthService employee credential null guard implemented",
        "UserService nullable payload contract implemented",
        "UserService username comparison null guard implemented",
        "UserService delete username null guard implemented",
        "CentralDbContext DbSet null-forgiving initialization implemented",
        "CentralDbContext audit entity id string conversion guard implemented",
        "CentralDbContext outbox tenant id string conversion guard implemented",
        "server services only remediation scope documented",
        "no checkout behavior change",
        "no inventory mutation",
        "no production sync enablement",
        "no schema change",
        "no migrations",
        "operator-safe targeted nullability remediation message documented"
    };

    public static string RequiredTargetedNullabilityServerServicesText => string.Join("; ", RequiredTargetedNullabilityServerServicesChecks);

    public sealed record TargetedNullabilityRemediationEvidence(
        string Scope,
        string RemediatedServices,
        string RemediatedWarnings,
        DateTime ReviewedAt,
        string SafetyStatement);

    public static bool HasMinimumTargetedNullabilityServerServicesReadiness(
        bool hasAuthServiceGuards,
        bool hasUserServiceGuards,
        bool hasCentralDbContextGuards,
        bool hasServerServicesOnlyScope,
        bool hasNoBusinessLogicChange)
    {
        return hasAuthServiceGuards
            && hasUserServiceGuards
            && hasCentralDbContextGuards
            && hasServerServicesOnlyScope
            && hasNoBusinessLogicChange;
    }

    public static TargetedNullabilityRemediationEvidence BuildTargetedNullabilityRemediationEvidence(DateTime reviewedAt)
    {
        return new TargetedNullabilityRemediationEvidence(
            "server services targeted nullability remediation only",
            "AuthService; UserService; CentralDbContext",
            "CS8602; CS8601; CS8619; DbSet null initialization",
            reviewedAt,
            "server services nullability remediation only - no checkout behavior change, no inventory mutation, no production sync enablement, no schema change, no migrations");
    }

    public static string BuildTargetedNullabilityRemediationSummary(bool ready, DateTime reviewedAt)
    {
        var status = ready ? "ready" : "blocked";
        return $"targeted_nullability_server_services_status={status}; services=AuthService|UserService|CentralDbContext; reviewed_at={reviewedAt:O}; no checkout behavior change; no inventory mutation; no production sync enablement; no schema change; no migrations";
    }
}
