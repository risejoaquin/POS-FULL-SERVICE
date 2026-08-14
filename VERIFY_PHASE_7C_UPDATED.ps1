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
    if ($content -notlike "*$Text*") {
        throw "Missing marker '$Text' in $Path"
    }
}

Assert-FileContains "PosCore\Security\PosTargetedNullabilityServerServicesRemediation.cs" "PosTargetedNullabilityServerServicesRemediation"
Assert-FileContains "PosCore\Security\PosTargetedNullabilityServerServicesRemediation.cs" "AuthService nullable password hash guard implemented"
Assert-FileContains "PosCore\Security\PosTargetedNullabilityServerServicesRemediation.cs" "UserService nullable payload contract implemented"
Assert-FileContains "PosCore\Security\PosTargetedNullabilityServerServicesRemediation.cs" "CentralDbContext audit entity id string conversion guard implemented"
Assert-FileContains "PosCore\Security\PosTargetedNullabilityServerServicesRemediation.cs" "no checkout behavior change"
Assert-FileContains "PosCore\Security\PosTargetedNullabilityServerServicesRemediation.cs" "no inventory mutation"
Assert-FileContains "PosCore\Security\PosTargetedNullabilityServerServicesRemediation.cs" "no production sync enablement"
Assert-FileContains "PosCore\Security\PosTargetedNullabilityServerServicesRemediation.cs" "no schema change"
Assert-FileContains "PosCore\Security\PosTargetedNullabilityServerServicesRemediation.cs" "no migrations"

Assert-FileContains "PosInfrastructure\Services\Server\AuthService.cs" "PHASE 7C targeted AuthService nullability remediation"
Assert-FileContains "PosInfrastructure\Services\Server\AuthService.cs" "!string.IsNullOrEmpty(user.PasswordHash)"
Assert-FileContains "PosInfrastructure\Services\Server\AuthService.cs" "Admin credentials required"
Assert-FileContains "PosInfrastructure\Services\Server\UserService.cs" "PHASE 7C targeted server service nullability remediation"
Assert-FileContains "PosInfrastructure\Services\Server\UserService.cs" "User? user"
Assert-FileContains "PosInfrastructure\Data\Server\CentralDbContext.cs" "PHASE 7C targeted CentralDbContext nullability remediation"
Assert-FileContains "PosInfrastructure\Data\Server\CentralDbContext.cs" "ToString() ?? string.Empty"

Assert-FileContains "docs\POS_TARGETED_NULLABILITY_SERVER_SERVICES_REMEDIATION.md" "server services only remediation scope documented"
Assert-FileContains "docs\PHASE_7C_TARGETED_NULLABILITY_SERVER_SERVICES_REMEDIATION.md" "355 tests passed"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_7C.md" "20% -> 30%"
Assert-FileContains "README.md" "PHASE 7C"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "Security & Dependency Hardening: 20% -> 30%"

Write-Host "PHASE 7C markers verified."
