$ErrorActionPreference = "Stop"

$requiredFiles = @(
    "PosCore\Security\PosProductionSyncFinalEnablementReadinessClosureBaseline.cs",
    "docs\POS_PRODUCTION_SYNC_FINAL_ENABLEMENT_READINESS_CLOSURE_BASELINE.md",
    "docs\PHASE_5J_PRODUCTION_SYNC_FINAL_ENABLEMENT_READINESS_CLOSURE_BASELINE.md",
    "docs\PROJECT_PROGRESS_REPORT_PHASE_5J.md"
)

foreach ($file in $requiredFiles) {
    if (-not (Test-Path $file)) {
        throw "Missing required file: $file"
    }
}

$checks = @(
    @{ Path = "PosCore\Security\PosProductionSyncFinalEnablementReadinessClosureBaseline.cs"; Text = "PosProductionSyncFinalEnablementReadinessClosureBaseline" },
    @{ Path = "PosCore\ViewModels\InventoryViewModel.cs"; Text = "PosProductionSyncFinalEnablementReadinessClosureStatus" },
    @{ Path = "PosCore\ViewModels\InventoryViewModel.cs"; Text = "PosProductionSyncFinalEnablementReadinessClosureRequiredChecks" },
    @{ Path = "PosCore\ViewModels\InventoryViewModel.cs"; Text = "PreparePosProductionSyncFinalEnablementReadinessClosureBaseline" },
    @{ Path = "PosCore\Views\InventoryWindow.xaml"; Text = "Sync Readiness" },
    @{ Path = "PosCore\Views\InventoryWindow.xaml"; Text = "no ejecuta sync real" },
    @{ Path = "PosCore\Views\InventoryWindow.xaml"; Text = "no habilita sync" },
    @{ Path = "PosCore\Views\InventoryWindow.xaml"; Text = "no alterna runtime flags" },
    @{ Path = "docs\POS_PRODUCTION_SYNC_FINAL_ENABLEMENT_READINESS_CLOSURE_BASELINE.md"; Text = "production sync final enablement readiness closure baseline only" },
    @{ Path = "docs\POS_PRODUCTION_SYNC_FINAL_ENABLEMENT_READINESS_CLOSURE_BASELINE.md"; Text = "final enablement readiness closure documented" },
    @{ Path = "docs\POS_PRODUCTION_SYNC_FINAL_ENABLEMENT_READINESS_CLOSURE_BASELINE.md"; Text = "all prior phase closures documented" },
    @{ Path = "docs\POS_PRODUCTION_SYNC_FINAL_ENABLEMENT_READINESS_CLOSURE_BASELINE.md"; Text = "verification evidence documented" },
    @{ Path = "docs\POS_PRODUCTION_SYNC_FINAL_ENABLEMENT_READINESS_CLOSURE_BASELINE.md"; Text = "test pass evidence documented" },
    @{ Path = "docs\POS_PRODUCTION_SYNC_FINAL_ENABLEMENT_READINESS_CLOSURE_BASELINE.md"; Text = "build pass evidence documented" },
    @{ Path = "docs\POS_PRODUCTION_SYNC_FINAL_ENABLEMENT_READINESS_CLOSURE_BASELINE.md"; Text = "feature flag readiness documented" },
    @{ Path = "docs\POS_PRODUCTION_SYNC_FINAL_ENABLEMENT_READINESS_CLOSURE_BASELINE.md"; Text = "kill switch readiness documented" },
    @{ Path = "docs\POS_PRODUCTION_SYNC_FINAL_ENABLEMENT_READINESS_CLOSURE_BASELINE.md"; Text = "operator sign-off documented" },
    @{ Path = "docs\PHASE_5J_PRODUCTION_SYNC_FINAL_ENABLEMENT_READINESS_CLOSURE_BASELINE.md"; Text = "No production sync execution" },
    @{ Path = "docs\PHASE_5J_PRODUCTION_SYNC_FINAL_ENABLEMENT_READINESS_CLOSURE_BASELINE.md"; Text = "No sync enablement" },
    @{ Path = "docs\PHASE_5J_PRODUCTION_SYNC_FINAL_ENABLEMENT_READINESS_CLOSURE_BASELINE.md"; Text = "No runtime flag toggle" },
    @{ Path = "docs\PHASE_5J_PRODUCTION_SYNC_FINAL_ENABLEMENT_READINESS_CLOSURE_BASELINE.md"; Text = "No inventory mutation" },
    @{ Path = "docs\PROJECT_PROGRESS_REPORT_PHASE_5J.md"; Text = "90% -> 100%" }
)

foreach ($check in $checks) {
    $content = Get-Content $check.Path -Raw
    if ($content -notlike "*$($check.Text)*") {
        throw "Missing marker '$($check.Text)' in $($check.Path)"
    }
}

Write-Host "PHASE 5J markers verified."
