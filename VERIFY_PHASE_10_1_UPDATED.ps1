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

Assert-FileContains "VERIFY_PHASE_9J_UPDATED.ps1" "PHASE 9J markers verified."
Assert-FileContains "PosCore\Security\PosProductionEnvironmentReadinessValidation.cs" "PosProductionEnvironmentReadinessValidation"
Assert-FileContains "PosCore\Security\PosProductionEnvironmentReadinessValidation.cs" "PHASE 10.1 production environment readiness documented"
Assert-FileContains "PosCore\Security\PosProductionEnvironmentReadinessValidation.cs" "PHASE 10A production environment configuration validation documented"
Assert-FileContains "PosCore\Security\PosProductionEnvironmentReadinessValidation.cs" "PHASE 10B secrets and runtime configuration hardening documented"
Assert-FileContains "PosCore\Security\PosProductionEnvironmentReadinessValidation.cs" "PHASE 10C database production migration dry run validation documented"
Assert-FileContains "PosCore\Security\PosProductionEnvironmentReadinessValidation.cs" "PHASE 9J production handoff prerequisite documented"
Assert-FileContains "PosCore\Security\PosProductionEnvironmentReadinessValidation.cs" "490 tests passed source evidence documented"
Assert-FileContains "PosCore\Security\PosProductionEnvironmentReadinessValidation.cs" "505 tests expected after production environment readiness validation documented"
Assert-FileContains "PosCore\Security\PosProductionEnvironmentReadinessValidation.cs" "production-environment-readiness-evidence.json generation documented"
Assert-FileContains "PosCore\Security\PosProductionEnvironmentReadinessValidation.cs" "production-runtime-configuration-report.json generation documented"
Assert-FileContains "PosCore\Security\PosProductionEnvironmentReadinessValidation.cs" "database-migration-dry-run-report.json generation documented"
Assert-FileContains "PosCore\Security\PosProductionEnvironmentReadinessValidation.cs" "JWT_KEY validation documented"
Assert-FileContains "PosCore\Security\PosProductionEnvironmentReadinessValidation.cs" "PROVISION_KEY validation documented"
Assert-FileContains "PosCore\Security\PosProductionEnvironmentReadinessValidation.cs" "connection string validation documented"
Assert-FileContains "PosCore\Security\PosProductionEnvironmentReadinessValidation.cs" "CORS production origin validation documented"
Assert-FileContains "PosCore\Security\PosProductionEnvironmentReadinessValidation.cs" "health check endpoint readiness documented"
Assert-FileContains "PosCore\Security\PosProductionEnvironmentReadinessValidation.cs" "secrets are not printed documented"
Assert-FileContains "PosCore\Security\PosProductionEnvironmentReadinessValidation.cs" "database migrations dry run only documented"
Assert-FileContains "PosCore\Security\PosProductionEnvironmentReadinessValidation.cs" "Railway configuration checklist documented"
Assert-FileContains "PosCore\Security\PosProductionEnvironmentReadinessValidation.cs" "Supabase configuration checklist documented"
Assert-FileContains "PosCore\Security\PosProductionEnvironmentReadinessValidation.cs" "no real deployment execution"
Assert-FileContains "PosCore\Security\PosProductionEnvironmentReadinessValidation.cs" "no Railway mutation"
Assert-FileContains "PosCore\Security\PosProductionEnvironmentReadinessValidation.cs" "no Supabase mutation"
Assert-FileContains "PosCore\Security\PosProductionEnvironmentReadinessValidation.cs" "no production database migration execution"
Assert-FileContains "PosCore\Security\PosProductionEnvironmentReadinessValidation.cs" "no live secret disclosure"
Assert-FileContains "PosCore\Security\PosProductionEnvironmentReadinessValidation.cs" "no schema change"
Assert-FileContains "PosCore\Security\PosProductionEnvironmentReadinessValidation.cs" "no migrations"

Assert-FileContains "scripts\release\Validate-Phase10ProductionReadiness.ps1" "param("
Assert-FileContains "scripts\release\Validate-Phase10ProductionReadiness.ps1" "Simulate-Phase9ReleaseExecutionClosure.ps1"
Assert-FileContains "scripts\release\Validate-Phase10ProductionReadiness.ps1" "production-environment-readiness-evidence.json"
Assert-FileContains "scripts\release\Validate-Phase10ProductionReadiness.ps1" "production-runtime-configuration-report.json"
Assert-FileContains "scripts\release\Validate-Phase10ProductionReadiness.ps1" "database-migration-dry-run-report.json"
Assert-FileContains "scripts\release\Validate-Phase10ProductionReadiness.ps1" "JWT_KEY"
Assert-FileContains "scripts\release\Validate-Phase10ProductionReadiness.ps1" "PROVISION_KEY"
Assert-FileContains "scripts\release\Validate-Phase10ProductionReadiness.ps1" "DATABASE_URL"
Assert-FileContains "scripts\release\Validate-Phase10ProductionReadiness.ps1" "ALLOWED_CORS_ORIGINS"
Assert-FileContains "scripts\release\Validate-Phase10ProductionReadiness.ps1" "no real deployment execution"
Assert-FileContains "scripts\release\Validate-Phase10ProductionReadiness.ps1" "no Railway mutation"
Assert-FileContains "scripts\release\Validate-Phase10ProductionReadiness.ps1" "no Supabase mutation"
Assert-FileContains "scripts\release\Validate-Phase10ProductionReadiness.ps1" "no production database migration execution"
Assert-FileContains "scripts\release\Validate-Phase10ProductionReadiness.ps1" "no live secret disclosure"
Assert-FileContains "scripts\release\Validate-Phase10ProductionReadiness.ps1" "PHASE 10.1 production environment readiness verified."
Assert-FileContains "scripts\release\Validate-Phase10ProductionReadiness.ps1" "AcceptedChecks: 15"
Assert-FileContains "scripts\release\Validate-Phase10ProductionReadiness.ps1" "BlockingIssues: 0"

Assert-FileContains "docs\POS_PRODUCTION_ENVIRONMENT_READINESS.md" "PHASE 10.1 production environment readiness documented"
Assert-FileContains "docs\PHASE_10_1_PRODUCTION_ENVIRONMENT_READINESS.md" "490 tests passed"
Assert-FileContains "docs\PHASE_10_1_PRODUCTION_ENVIRONMENT_READINESS.md" "505 tests passed"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_10_1.md" "Production readiness advanced from 0% to 30%"
Assert-FileContains "README.md" "PHASE 10.1"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "PHASE 10.1"

Write-Host "PHASE 10.1 markers verified."
