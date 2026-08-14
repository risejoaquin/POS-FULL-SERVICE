# PHASE 9A Hotfix — Verifier and Release Script Root Layout Fix

This hotfix preserves PHASE 9A scope and fixes two packaging/execution blockers:

1. Ensures PHASE 8E, 8F, 8G, 8H, 8I, 8J, and PHASE 9A verifier scripts are present at repository root for cumulative architecture tests.
2. Moves the PowerShell `param(...)` block to the first executable position in `scripts/release/Generate-Phase9ReleaseArtifacts.ps1` so parameter defaults parse correctly.

No checkout behavior change, no inventory mutation, no production sync enablement, no deployment execution, no public API behavior change, no schema change, and no migrations.
