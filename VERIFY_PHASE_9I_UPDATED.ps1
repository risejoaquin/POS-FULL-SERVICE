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

Assert-FileContains "PosCore\Security\PosInstallerReleaseCandidateFinalEvidenceOperatorAcceptanceValidation.cs" "PosInstallerReleaseCandidateFinalEvidenceOperatorAcceptanceValidation"
Assert-FileContains "PosCore\Security\PosInstallerReleaseCandidateFinalEvidenceOperatorAcceptanceValidation.cs" "POS Installer Release Candidate Final Evidence and Operator Acceptance Validation"
Assert-FileContains "PosCore\Security\PosInstallerReleaseCandidateFinalEvidenceOperatorAcceptanceValidation.cs" "installer release candidate final evidence operator acceptance validation documented"
Assert-FileContains "PosCore\Security\PosInstallerReleaseCandidateFinalEvidenceOperatorAcceptanceValidation.cs" "PHASE 9H rollback simulation prerequisite documented"
Assert-FileContains "PosCore\Security\PosInstallerReleaseCandidateFinalEvidenceOperatorAcceptanceValidation.cs" "480 tests passed source evidence documented"
Assert-FileContains "PosCore\Security\PosInstallerReleaseCandidateFinalEvidenceOperatorAcceptanceValidation.cs" "485 tests expected after installer release candidate final evidence operator acceptance validation documented"
Assert-FileContains "PosCore\Security\PosInstallerReleaseCandidateFinalEvidenceOperatorAcceptanceValidation.cs" "release-candidate-final-evidence.json generation documented"
Assert-FileContains "PosCore\Security\PosInstallerReleaseCandidateFinalEvidenceOperatorAcceptanceValidation.cs" "operator-acceptance-checklist.json generation documented"
Assert-FileContains "PosCore\Security\PosInstallerReleaseCandidateFinalEvidenceOperatorAcceptanceValidation.cs" "operator acceptance checklist documented"
Assert-FileContains "PosCore\Security\PosInstallerReleaseCandidateFinalEvidenceOperatorAcceptanceValidation.cs" "blocking issues count documented"
Assert-FileContains "PosCore\Security\PosInstallerReleaseCandidateFinalEvidenceOperatorAcceptanceValidation.cs" "accepted checks count documented"
Assert-FileContains "PosCore\Security\PosInstallerReleaseCandidateFinalEvidenceOperatorAcceptanceValidation.cs" "release artifact chain evidence documented"
Assert-FileContains "PosCore\Security\PosInstallerReleaseCandidateFinalEvidenceOperatorAcceptanceValidation.cs" "installer integrity evidence documented"
Assert-FileContains "PosCore\Security\PosInstallerReleaseCandidateFinalEvidenceOperatorAcceptanceValidation.cs" "smoke install evidence documented"
Assert-FileContains "PosCore\Security\PosInstallerReleaseCandidateFinalEvidenceOperatorAcceptanceValidation.cs" "launcher package evidence documented"
Assert-FileContains "PosCore\Security\PosInstallerReleaseCandidateFinalEvidenceOperatorAcceptanceValidation.cs" "uninstall cleanup evidence documented"
Assert-FileContains "PosCore\Security\PosInstallerReleaseCandidateFinalEvidenceOperatorAcceptanceValidation.cs" "upgrade preservation evidence documented"
Assert-FileContains "PosCore\Security\PosInstallerReleaseCandidateFinalEvidenceOperatorAcceptanceValidation.cs" "rollback recovery evidence documented"
Assert-FileContains "PosCore\Security\PosInstallerReleaseCandidateFinalEvidenceOperatorAcceptanceValidation.cs" "no real release execution"
Assert-FileContains "PosCore\Security\PosInstallerReleaseCandidateFinalEvidenceOperatorAcceptanceValidation.cs" "no real installer execution"
Assert-FileContains "PosCore\Security\PosInstallerReleaseCandidateFinalEvidenceOperatorAcceptanceValidation.cs" "no real rollback execution"
Assert-FileContains "PosCore\Security\PosInstallerReleaseCandidateFinalEvidenceOperatorAcceptanceValidation.cs" "no file overwrite"
Assert-FileContains "PosCore\Security\PosInstallerReleaseCandidateFinalEvidenceOperatorAcceptanceValidation.cs" "no database writes"
Assert-FileContains "PosCore\Security\PosInstallerReleaseCandidateFinalEvidenceOperatorAcceptanceValidation.cs" "no deployment execution"
Assert-FileContains "PosCore\Security\PosInstallerReleaseCandidateFinalEvidenceOperatorAcceptanceValidation.cs" "no schema change"
Assert-FileContains "PosCore\Security\PosInstallerReleaseCandidateFinalEvidenceOperatorAcceptanceValidation.cs" "no migrations"

Assert-FileContains "scripts\release\Simulate-Phase9ReleaseCandidateAcceptance.ps1" "Simulate-Phase9InstallerRollback.ps1"
Assert-FileContains "scripts\release\Simulate-Phase9ReleaseCandidateAcceptance.ps1" "release-candidate-final-evidence.json"
Assert-FileContains "scripts\release\Simulate-Phase9ReleaseCandidateAcceptance.ps1" "operator-acceptance-checklist.json"
Assert-FileContains "scripts\release\Simulate-Phase9ReleaseCandidateAcceptance.ps1" "PHASE 9I installer release candidate final evidence and operator acceptance verified."
Assert-FileContains "scripts\release\Simulate-Phase9ReleaseCandidateAcceptance.ps1" "AcceptedChecks"
Assert-FileContains "scripts\release\Simulate-Phase9ReleaseCandidateAcceptance.ps1" "BlockingIssues"
Assert-FileContains "scripts\release\Simulate-Phase9ReleaseCandidateAcceptance.ps1" "noRealReleaseExecution"
Assert-FileContains "scripts\release\Simulate-Phase9ReleaseCandidateAcceptance.ps1" "noMigrations"

Assert-FileContains "docs\POS_INSTALLER_RELEASE_CANDIDATE_FINAL_EVIDENCE_OPERATOR_ACCEPTANCE_VALIDATION.md" "installer release candidate final evidence operator acceptance validation documented"
Assert-FileContains "docs\POS_INSTALLER_RELEASE_CANDIDATE_FINAL_EVIDENCE_OPERATOR_ACCEPTANCE_VALIDATION.md" "PHASE 9H rollback simulation prerequisite documented"
Assert-FileContains "docs\PHASE_9I_INSTALLER_RELEASE_CANDIDATE_FINAL_EVIDENCE_OPERATOR_ACCEPTANCE_VALIDATION.md" "480 tests passed"
Assert-FileContains "docs\PHASE_9I_INSTALLER_RELEASE_CANDIDATE_FINAL_EVIDENCE_OPERATOR_ACCEPTANCE_VALIDATION.md" "485 tests passed"
Assert-FileContains "docs\PROJECT_PROGRESS_REPORT_PHASE_9I.md" "Release Execution advanced from 80% to 90%"
Assert-FileContains "README.md" "PHASE 9I"
Assert-FileContains "ROADMAP_FINALIZACION_POS_ACTUALIZADO.md" "PHASE 9I"

Write-Host "PHASE 9I markers verified."
