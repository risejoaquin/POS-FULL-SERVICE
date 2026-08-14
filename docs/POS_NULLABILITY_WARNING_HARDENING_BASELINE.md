# POS Nullability Warning Hardening Baseline

## Purpose

PHASE 7B establishes the nullability warning hardening baseline before changing nullable reference handling in production code.

The goal is to classify the remaining nullable reference warnings and define the safe remediation order while preserving runtime behavior.

## Warning classes tracked

- CS8602 possible null dereference classified.
- CS8601 possible null assignment classified.
- CS8618 non-nullable initialization classified.
- CS8622 delegate nullability mismatch classified.
- CS8600 possible null conversion classified.
- CS8603 possible null return classified.

## Hotspots documented

- Server service nullability hotspots documented: `AuthService`, `UserService`.
- Central db context nullability hotspots documented: `CentralDbContext`.
- Sync service nullability hotspots documented: `SyncService`.
- Builder nullability hotspots documented: `MainWindow`, `WizardViewModel`, controls, notification/config models and converters.

## Remediation order documented

1. Classify warnings by risk and runtime behavior impact.
2. Prefer safe nullable annotations where the runtime value can legitimately be null.
3. Add explicit null guards only where existing behavior already requires a non-null value.
4. Avoid broad refactors during warning hardening.
5. Keep all tests and Release build green after each slice.

## Fail-safe null handling requirement documented

Any future nullability remediation must fail closed or preserve the existing safe behavior. It must not hide real errors, must not introduce default business values silently, and must not bypass authorization, tenant checks, sync gates, inventory guards or checkout boundaries.

## Safety boundary

- No checkout behavior change.
- No inventory mutation.
- No production sync enablement.
- No schema change.
- No migrations.

## Operator-safe message

Nullability warning hardening baseline prepared. This is a classification and remediation-planning phase only; it does not change checkout, inventory, production sync, schema or migrations.
