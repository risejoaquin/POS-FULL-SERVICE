# POS Offline Sync Conflict Detection Strategy Baseline

## Scope

This document defines the **offline sync conflict detection strategy baseline only** for the POS offline sync reliability block.

This phase is diagnostic/design only:

- no production sync execution
- no queue writes
- no conflict resolution execution
- no inventory mutation
- no checkout changes
- no schema change
- no migrations

## Required checks

- conflict detection strategy documented
- server version comparison documented
- local version comparison documented
- last synced version reviewed
- entity type conflict scope documented
- entity id conflict scope documented
- tenant boundary validation reviewed
- idempotency key interaction documented
- retry/backoff interaction documented
- manual review conflict threshold documented
- operator-safe conflict message documented
- correlation id logging reviewed

## Strategy baseline

A future production sync must detect conflicts before applying changes. The baseline compares server version, local version and last synced version within tenant and entity scope.

Suggested conflict check:

```text
if server_version != last_synced_version and local_version != last_synced_version:
    conflict_detected = true
```

## Operator handling

Detected conflicts must be sent to manual review. This phase does not resolve conflicts automatically.

## Guardrails

Conflict detection must remain compatible with idempotency keys and retry/backoff. A duplicate retry with the same idempotency key must not be treated as a new conflict by itself.
