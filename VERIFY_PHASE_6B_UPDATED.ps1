$ErrorActionPreference = "Stop"

$requiredFiles = @(
    "PosCore\Security\PosProductionSyncKillSwitchRuntimeEnforcementImplementation.cs",
    "docs\POS_PRODUCTION_SYNC_KILL_SWITCH_RUNTIME_ENFORCEMENT_IMPLEMENTATION.md",
    "docs\PHASE_6B_PRODUCTION_SYNC_KILL_SWITCH_RUNTIME_ENFORCEMENT_IMPLEMENTATION.md",
    "docs\PROJECT_PROGRESS_REPORT_PHASE_6B.md"
)

foreach ($file in $requiredFiles) {
    if (!(Test-Path $file)) {
        throw "Missing required PHASE 6B file: $file"
    }
}

$markers = @(
    "PosProductionSyncKillSwitchRuntimeEnforcementImplementationStatus",
    "PosProductionSyncKillSwitchRuntimeEnforcementImplementationReady",
    "PosProductionSyncKillSwitchRuntimeEnforcementImplementationReviewedAt",
    "PosProductionSyncKillSwitchRuntimeEnforcementImplementationRequiredChecks",
    "PosProductionSyncKillSwitchRuntimeEnforcementImplementationSummary",
    "PosProductionSyncKillSwitchRuntimeEnforcementImplementationEvidence",
    "PreparePosProductionSyncKillSwitchRuntimeEnforcementImplementationCommand",
    "POS Production Sync Kill Switch Runtime Enforcement Implementation",
    "production sync kill switch runtime enforcement implementation documented",
    "kill switch runtime enforcement documented",
    "kill switch precedence over feature flag documented",
    "tenant scoped kill switch read documented",
    "device scoped kill switch read documented",
    "default fail-closed state documented",
    "read-before-processing requirement documented",
    "read-before-checkpoint requirement documented",
    "read-before-queue-claim requirement documented",
    "operator override prohibition documented",
    "auditable runtime decision documented",
    "correlation id runtime decision documented",
    "tenant device runtime decision documented",
    "idempotent block decision documented",
    "operator-safe kill switch message documented",
    "rollback to disabled documented",
    "manual support escalation documented",
    "no production sync execution",
    "no sync enablement",
    "no queue writes",
    "no runtime flag toggle",
    "no checkpoint advancement",
    "no inventory mutation",
    "no checkout changes",
    "Phase6B_Should_Be_Documented_As_Production_Sync_KillSwitchRuntimeEnforcementImplementation_Controlled_Only"
)

$allText = Get-ChildItem -Recurse -Include *.cs,*.xaml,*.md | ForEach-Object { Get-Content $_.FullName -Raw } | Out-String

foreach ($marker in $markers) {
    if ($allText -notlike "*$marker*") {
        throw "Missing PHASE 6B marker: $marker"
    }
}

Write-Host "PHASE 6B markers verified."
