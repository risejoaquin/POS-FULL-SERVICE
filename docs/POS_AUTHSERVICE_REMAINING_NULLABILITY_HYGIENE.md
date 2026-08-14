# POS AuthService Remaining Nullability Hygiene

AuthService remaining nullability hygiene documented.

## Scope

This phase targets the remaining `CS8602` warning in `PosInfrastructure/Services/Server/AuthService.cs` during login username normalization and entity username comparison.

## Checks

- CS8602 AuthService login username dereference hygiene documented.
- login username local variable boundary implemented.
- login password local variable boundary implemented.
- nullable entity username guard implemented.
- login behavior preserved.
- credential validation behavior preserved.

## Protected boundaries

- No checkout behavior change.
- No inventory mutation.
- No production sync enablement.
- No public API behavior change.
- No schema change.
- No migrations.
