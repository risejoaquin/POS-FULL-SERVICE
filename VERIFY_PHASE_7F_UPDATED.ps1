$ErrorActionPreference = "Stop"

function Assert-FileContains {
    param(
        [string]$Path,
        [string]$Text
    )

    if (!(Test-Path $Path)) {
        throw "Missing required file: $Path"
    }

    $content = Get-Content $Path -Raw
    if (!$content.Contains($Text)) {
        throw "Missing marker '$Text' in $Path"
    }
}

Assert-FileContains "PosCore\Security\PosBuilderNullabilityHygiene.cs" "PosBuilderNullabilityHygiene"
Assert-FileContains "PosCore\Security\PosBuilderNullabilityHygiene.cs" "POS PosBuilder Nullability Hygiene"
Assert-FileContains "PosCore\Security\PosBuilderNullabilityHygiene.cs" "CS8618 non-nullable initialization hygiene documented"
Assert-FileContains "PosCore\Security\PosBuilderNullabilityHygiene.cs" "CS8622 event handler nullability hygiene documented"
Assert-FileContains "PosCore\Security\PosBuilderNullabilityHygiene.cs" "CS8603 converter return nullability hygiene documented"
Assert-FileContains "PosCore\Security\PosBuilderNullabilityHygiene.cs" "CS8600 and CS8601 possible null assignment hygiene documented"
Assert-FileContains "PosCore\Security\PosBuilderNullabilityHygiene.cs" "no checkout behavior change"
Assert-FileContains "PosCore\Security\PosBuilderNullabilityHygiene.cs" "no inventory mutation"
Assert-FileContains "PosCore\Security\PosBuilderNullabilityHygiene.cs" "no production sync enablement"
Assert-FileContains "PosCore\Security\PosBuilderNullabilityHygiene.cs" "no public API behavior change"
Assert-FileContains "PosCore\Security\PosBuilderNullabilityHygiene.cs" "no schema change"
Assert-FileContains "PosCore\Security\PosBuilderNullabilityHygiene.cs" "no migrations"

Assert-FileContains "PosBuilder\App.xaml.cs" "private string _logFilePath = string.Empty"
Assert-FileContains "PosBuilder\App.xaml.cs" "TaskScheduler_UnobservedTaskException(object? sender"
Assert-FileContains "PosBuilder\Models\ConfigModel.cs" "public string LicenseKey { get; set; } = string.Empty"
Assert-FileContains "PosBuilder\ViewModels\WizardViewModel.cs" "private string _currentStepTitle = string.Empty"
Assert-FileContains "PosBuilder\ViewModels\WizardViewModel.cs" "private string _currentStepSubTitle = string.Empty"
Assert-FileContains "PosBuilder\ViewModels\WizardViewModel.cs" "private string _currentStepCategory = string.Empty"
Assert-FileContains "PosBuilder\MainWindow.xaml.cs" "private string _title = string.Empty"
Assert-FileContains "PosBuilder\MainWindow.xaml.cs" "License key response is missing"
Assert-FileContains "PosBuilder\Converters.cs" "parameter?.ToString() ?? string.Empty"
Assert-FileContains "PosBuilder\Views\Controls\FileBrowserControl.xaml.cs" "public event EventHandler<string>? FileSelected"
Assert-FileContains "PosBuilder\Views\Controls\ColorPickerControl.xaml.cs" "public event EventHandler<string>? ColorChanged"
Assert-FileContains "PosBuilder\Views\Controls\ColorPickerControl.xaml.cs" "ParseColorOrDefault"
Assert-FileContains "PosBuilder\Services\NotificationService.cs" "ResolveBrush"

Assert-FileContains "docs\POSBUILDER_NULLABILITY_HYGIENE.md" "PosBuilder nullability hygiene documented"
Assert-FileContains "docs\PHASE_7F_POSBUILDER_NULLABILITY_HYGIENE.md" "370 tests passed"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_7F.md" "50% -> 60%"

Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "PosBuilderNullabilityHygiene_Should_Define_PosBuilder_Nullability_Checks"
Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "Phase7F_PosBuilder_Source_Should_Apply_Targeted_Nullability_Hygiene"
Assert-FileContains "PosInfrastructure.Tests\Architecture\InventoryLedgerConcurrencyBaselineTests.cs" "VerifyPhase7F_Should_Require_PosBuilder_Nullability_Hygiene_Markers"

Write-Host "PHASE 7F markers verified."
