# POS SyncService Nullability Hygiene

SyncService nullability hygiene documented.

## Scope

PHASE 7G applies a targeted CS8602 hygiene fix in `PosCore/Services/SyncService.cs`.

## Warning addressed

- CS8602 SyncService username dereference hygiene documented.
- Cloud user names can be nullable at the model boundary.
- Local user names can be nullable at the EF/query boundary.

## Implementation markers

- cloud username null guard implemented.
- normalized cloud username boundary documented.
- local username null guard implemented.
- invalid cloud username skip boundary documented.
- pull updates behavior preserved.

## Safety boundaries

- No checkout behavior change.
- No inventory mutation.
- No production sync enablement.
- No public API behavior change.
- No schema change.
- No migrations.
