namespace PosCore.Security;

/// <summary>
/// PHASE 7F PosBuilder nullability hygiene contract.
/// This phase applies targeted nullability hygiene to PosBuilder UI/bootstrap code without changing checkout, inventory, production sync, schema or migrations.
/// </summary>
public static class PosBuilderNullabilityHygiene
{
    public const string HygieneName = "POS PosBuilder Nullability Hygiene";

    public static readonly string[] RequiredPosBuilderNullabilityHygieneChecks =
    {
        "PosBuilder nullability hygiene documented",
        "CS8618 non-nullable initialization hygiene documented",
        "CS8622 event handler nullability hygiene documented",
        "CS8603 converter return nullability hygiene documented",
        "CS8600 and CS8601 possible null assignment hygiene documented",
        "App.xaml.cs handler sender nullable compatibility applied",
        "ConfigModel LicenseKey initialized",
        "WizardViewModel step title fields initialized",
        "MainWindow StepIndicator fields initialized",
        "MainWindow provisioning response null guards applied",
        "Converters ConvertBack null guard applied",
        "ColorPickerControl event and palette model nullability hygiene applied",
        "FileBrowserControl event nullability hygiene applied",
        "NotificationService brush conversion null guard applied",
        "PosBuilder UI only remediation scope documented",
        "no checkout behavior change",
        "no inventory mutation",
        "no production sync enablement",
        "no public API behavior change",
        "no schema change",
        "no migrations"
    };

    public static string RequiredPosBuilderNullabilityHygieneText => string.Join("; ", RequiredPosBuilderNullabilityHygieneChecks);

    public sealed record PosBuilderNullabilityHygieneEvidence(
        string Scope,
        string TargetProject,
        string WarningFamilies,
        string SafetyStatement);

    public static bool HasMinimumPosBuilderNullabilityHygieneReadiness(
        bool hasNonNullableInitialization,
        bool hasEventHandlerCompatibility,
        bool hasConverterNullGuard,
        bool hasUiOnlyScope,
        bool hasNoBusinessLogicChange,
        bool hasNoSchemaChange)
    {
        return hasNonNullableInitialization
            && hasEventHandlerCompatibility
            && hasConverterNullGuard
            && hasUiOnlyScope
            && hasNoBusinessLogicChange
            && hasNoSchemaChange;
    }

    public static PosBuilderNullabilityHygieneEvidence BuildPosBuilderNullabilityHygieneEvidence()
    {
        return new PosBuilderNullabilityHygieneEvidence(
            "PosBuilder UI/bootstrap nullability hygiene only",
            "PosBuilder",
            "CS8618, CS8622, CS8603, CS8600, CS8601",
            "UI nullability hygiene only - no checkout behavior change, no inventory mutation, no production sync enablement, no public API behavior change, no schema change, no migrations");
    }

    public static string BuildPosBuilderNullabilityHygieneSummary(bool ready)
    {
        var status = ready ? "ready" : "blocked";
        return $"posbuilder_nullability_hygiene_status={status}; scope=PosBuilder UI/bootstrap; warnings=CS8618|CS8622|CS8603|CS8600|CS8601; initialized non-null fields; nullable event sender compatibility applied; converter and brush null guards applied; provisioning response null guards applied; no checkout behavior change; no inventory mutation; no production sync enablement; no public API behavior change; no schema change; no migrations";
    }
}
