# POS Checksum and Artifact Verification Baseline

## Scope

PHASE 8D records the checksum and artifact verification baseline after PHASE 8C. It is documentation and verification only. It does not create packages, installers, deployments or rollouts.

## Required evidence

- checksum and artifact verification baseline documented
- PHASE 8C versioning release manifest prerequisite documented
- 405 tests passed source evidence documented
- 410 tests expected after checksum verification baseline documented
- sha256 checksum algorithm documented
- artifact checksum generation command documented
- artifact checksum verification command documented
- manifest checksum cross-check documented
- artifact tamper detection checklist documented
- artifact path existence verification documented
- artifact size verification documented
- artifact version match verification documented
- release manifest checksum linkage documented
- operator checksum review checklist documented
- checksum failure handling checklist documented
- artifact verification audit evidence documented

## Checksum baseline

The release process must use SHA-256 checksums for every generated release artifact. The checksum value must be stored in the release manifest and reviewed before any install, rollout or deployment step.

```text
algorithm: SHA256
artifact_path: relative or absolute release artifact path
checksum_sha256: lowercase hexadecimal SHA-256 digest
verified_at_utc: UTC timestamp for operator or CI verification
verified_by: release operator or CI identity
```

## Verification command baseline

The baseline verification commands must be documented before real packaging execution:

```powershell
Get-FileHash -Algorithm SHA256 <artifact-path>
Compare generated SHA-256 hash with release manifest checksum_sha256
Confirm artifact path exists
Confirm artifact size is greater than zero
Confirm artifact version matches the release manifest version
```

## Failure handling baseline

If checksum verification fails, the operator must stop the release, quarantine the artifact, regenerate the release artifact from the clean source, and record the failed checksum evidence in the release audit notes.

## Safety boundaries

- No checkout behavior change
- No inventory mutation
- No production sync enablement
- No packaging execution
- No installer execution
- No deployment execution
- No public API behavior change
- No schema change
- No migrations
