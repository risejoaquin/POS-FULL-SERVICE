namespace PosCore.Security;

/// <summary>
/// PHASE 7H AuthService remaining nullability hygiene contract.
/// This phase removes the remaining AuthService CS8602 nullable username dereference warning without changing checkout, inventory, production sync enablement, schema or migrations.
/// </summary>
public static class PosAuthServiceRemainingNullabilityHygiene
{
    public const string HygieneName = "POS AuthService Remaining Nullability Hygiene";

    public static readonly string[] RequiredAuthServiceRemainingNullabilityHygieneChecks =
    {
        "AuthService remaining nullability hygiene documented",
        "CS8602 AuthService login username dereference hygiene documented",
        "login username local variable boundary implemented",
        "login password local variable boundary implemented",
        "nullable entity username guard implemented",
        "login behavior preserved",
        "credential validation behavior preserved",
        "no checkout behavior change",
        "no inventory mutation",
        "no production sync enablement",
        "no public API behavior change",
        "no schema change",
        "no migrations"
    };

    public static string RequiredAuthServiceRemainingNullabilityHygieneText => string.Join("; ", RequiredAuthServiceRemainingNullabilityHygieneChecks);

    public sealed record AuthServiceRemainingNullabilityHygieneEvidence(
        string Scope,
        string TargetFile,
        string WarningFamily,
        string SafetyStatement);

    public static bool HasMinimumAuthServiceRemainingNullabilityHygieneReadiness(
        bool hasLoginUsernameBoundary,
        bool hasLoginPasswordBoundary,
        bool hasNullableEntityUsernameGuard,
        bool preservesLoginBehavior,
        bool hasNoBusinessLogicChange,
        bool hasNoSchemaChange)
    {
        return hasLoginUsernameBoundary
            && hasLoginPasswordBoundary
            && hasNullableEntityUsernameGuard
            && preservesLoginBehavior
            && hasNoBusinessLogicChange
            && hasNoSchemaChange;
    }

    public static AuthServiceRemainingNullabilityHygieneEvidence BuildAuthServiceRemainingNullabilityHygieneEvidence()
    {
        return new AuthServiceRemainingNullabilityHygieneEvidence(
            "AuthService login nullable username guard only",
            "PosInfrastructure/Services/Server/AuthService.cs",
            "CS8602 possible null dereference",
            "AuthService remaining nullability hygiene only - no checkout behavior change, no inventory mutation, no production sync enablement, no public API behavior change, no schema change, no migrations");
    }

    public static string BuildAuthServiceRemainingNullabilityHygieneSummary(bool ready)
    {
        var status = ready ? "ready" : "blocked";
        return $"authservice_remaining_nullability_hygiene_status={status}; scope=AuthService login username normalization guard; warnings=CS8602; login username local variable boundary implemented; login password local variable boundary implemented; nullable entity username guard implemented; login behavior preserved; credential validation behavior preserved; no checkout behavior change; no inventory mutation; no production sync enablement; no public API behavior change; no schema change; no migrations";
    }
}
