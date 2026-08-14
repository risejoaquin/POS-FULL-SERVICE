$ErrorActionPreference = "Stop"

function Assert-FileContains {
    param(
        [string]$Path,
        [string]$Text
    )

    if (!(Test-Path $Path)) {
        throw "Missing file: $Path"
    }

    $content = Get-Content -Raw -Path $Path
    if (!$content.Contains($Text)) {
        throw "Missing marker in ${Path}: $Text"
    }
}

Assert-FileContains "VERIFY_PHASE_11_3_UPDATED.ps1" "PHASE 11.3 markers verified."
Assert-FileContains "PosCore\Security\PosHardwareReadinessStorePilotValidation.cs" "PosHardwareReadinessStorePilotValidation"
Assert-FileContains "PosCore\Security\PosHardwareReadinessStorePilotValidation.cs" "PHASE 11.4 hardware readiness and store pilot checklist documented"
Assert-FileContains "PosCore\Security\PosHardwareReadinessStorePilotValidation.cs" "PHASE 11J POS peripheral readiness validation documented"
Assert-FileContains "PosCore\Security\PosHardwareReadinessStorePilotValidation.cs" "PHASE 11K operator training and pilot checklist documented"
Assert-FileContains "PosCore\Security\PosHardwareReadinessStorePilotValidation.cs" "PHASE 11L store pilot rehearsal validation documented"
Assert-FileContains "PosCore\Security\PosHardwareReadinessStorePilotValidation.cs" "PHASE 11.3 inventory stock offline sync prerequisite documented"
Assert-FileContains "PosCore\Security\PosHardwareReadinessStorePilotValidation.cs" "588 tests passed source evidence documented"
Assert-FileContains "PosCore\Security\PosHardwareReadinessStorePilotValidation.cs" "604 tests expected after hardware readiness store pilot validation documented"
Assert-FileContains "PosCore\Security\PosHardwareReadinessStorePilotValidation.cs" "pos-peripheral-readiness-evidence.json generation documented"
Assert-FileContains "PosCore\Security\PosHardwareReadinessStorePilotValidation.cs" "operator-training-pilot-checklist.json generation documented"
Assert-FileContains "PosCore\Security\PosHardwareReadinessStorePilotValidation.cs" "store-pilot-rehearsal-evidence.json generation documented"
Assert-FileContains "PosCore\Security\PosHardwareReadinessStorePilotValidation.cs" "hardware-readiness-store-pilot-summary.json generation documented"
Assert-FileContains "PosCore\Security\PosHardwareReadinessStorePilotValidation.cs" "thermal printer compatibility checklist documented"
Assert-FileContains "PosCore\Security\PosHardwareReadinessStorePilotValidation.cs" "cash drawer compatibility checklist documented"
Assert-FileContains "PosCore\Security\PosHardwareReadinessStorePilotValidation.cs" "barcode scanner compatibility checklist documented"
Assert-FileContains "PosCore\Security\PosHardwareReadinessStorePilotValidation.cs" "payment terminal readiness checklist documented"
Assert-FileContains "PosCore\Security\PosHardwareReadinessStorePilotValidation.cs" "device driver and port mapping checklist documented"
Assert-FileContains "PosCore\Security\PosHardwareReadinessStorePilotValidation.cs" "operator training checklist documented"
Assert-FileContains "PosCore\Security\PosHardwareReadinessStorePilotValidation.cs" "pilot store entry checklist documented"
Assert-FileContains "PosCore\Security\PosHardwareReadinessStorePilotValidation.cs" "pilot issue capture checklist documented"
Assert-FileContains "PosCore\Security\PosHardwareReadinessStorePilotValidation.cs" "go-live rehearsal checklist documented"
Assert-FileContains "PosCore\Security\PosHardwareReadinessStorePilotValidation.cs" "support escalation checklist documented"
Assert-FileContains "PosCore\Security\PosHardwareReadinessStorePilotValidation.cs" "pilot exit criteria documented"
Assert-FileContains "PosCore\Security\PosHardwareReadinessStorePilotValidation.cs" "no real hardware access"
Assert-FileContains "PosCore\Security\PosHardwareReadinessStorePilotValidation.cs" "no live device mutation"
Assert-FileContains "PosCore\Security\PosHardwareReadinessStorePilotValidation.cs" "no printer execution"
Assert-FileContains "PosCore\Security\PosHardwareReadinessStorePilotValidation.cs" "no cash drawer pulse"
Assert-FileContains "PosCore\Security\PosHardwareReadinessStorePilotValidation.cs" "no scanner capture"
Assert-FileContains "PosCore\Security\PosHardwareReadinessStorePilotValidation.cs" "no payment terminal execution"
Assert-FileContains "PosCore\Security\PosHardwareReadinessStorePilotValidation.cs" "no store pilot activation"
Assert-FileContains "PosCore\Security\PosHardwareReadinessStorePilotValidation.cs" "no production traffic routing"
Assert-FileContains "PosCore\Security\PosHardwareReadinessStorePilotValidation.cs" "no real inventory mutation"
Assert-FileContains "PosCore\Security\PosHardwareReadinessStorePilotValidation.cs" "no production sync enablement"
Assert-FileContains "PosCore\Security\PosHardwareReadinessStorePilotValidation.cs" "no public API behavior change"
Assert-FileContains "PosCore\Security\PosHardwareReadinessStorePilotValidation.cs" "no schema change"
Assert-FileContains "PosCore\Security\PosHardwareReadinessStorePilotValidation.cs" "no migrations"
Assert-FileContains "scripts\release\Validate-Phase11HardwareReadinessStorePilotValidation.ps1" "param("
Assert-FileContains "scripts\release\Validate-Phase11HardwareReadinessStorePilotValidation.ps1" "Validate-Phase11InventoryStockOfflineSyncValidation.ps1"
Assert-FileContains "scripts\release\Validate-Phase11HardwareReadinessStorePilotValidation.ps1" "PHASE 11.3 inventory stock offline sync outputs are missing"
Assert-FileContains "scripts\release\Validate-Phase11HardwareReadinessStorePilotValidation.ps1" "pos-peripheral-readiness-evidence.json"
Assert-FileContains "scripts\release\Validate-Phase11HardwareReadinessStorePilotValidation.ps1" "operator-training-pilot-checklist.json"
Assert-FileContains "scripts\release\Validate-Phase11HardwareReadinessStorePilotValidation.ps1" "store-pilot-rehearsal-evidence.json"
Assert-FileContains "scripts\release\Validate-Phase11HardwareReadinessStorePilotValidation.ps1" "hardware-readiness-store-pilot-summary.json"
Assert-FileContains "scripts\release\Validate-Phase11HardwareReadinessStorePilotValidation.ps1" "no real hardware access"
Assert-FileContains "scripts\release\Validate-Phase11HardwareReadinessStorePilotValidation.ps1" "no printer execution"
Assert-FileContains "scripts\release\Validate-Phase11HardwareReadinessStorePilotValidation.ps1" "no cash drawer pulse"
Assert-FileContains "scripts\release\Validate-Phase11HardwareReadinessStorePilotValidation.ps1" "no store pilot activation"
Assert-FileContains "scripts\release\Validate-Phase11HardwareReadinessStorePilotValidation.ps1" "PHASE 11.4 hardware readiness and store pilot checklist verified."
Assert-FileContains "scripts\release\Validate-Phase11HardwareReadinessStorePilotValidation.ps1" "AcceptedChecks: 15"
Assert-FileContains "scripts\release\Validate-Phase11HardwareReadinessStorePilotValidation.ps1" "BlockingIssues: 0"
Assert-FileContains "docs\POS_HARDWARE_READINESS_STORE_PILOT_VALIDATION.md" "PHASE 11.4 hardware readiness and store pilot checklist documented"
Assert-FileContains "docs\PHASE_11_4_HARDWARE_READINESS_STORE_PILOT_CHECKLIST.md" "588 tests passed"
Assert-FileContains "docs\PHASE_11_4_HARDWARE_READINESS_STORE_PILOT_CHECKLIST.md" "604 tests passed"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_11_4.md" "Functional business validation advanced from 75% to 100%"
Assert-FileContains "README.md" "PHASE 11.4"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "PHASE 11.4"

Write-Host "PHASE 11.4 markers verified."
