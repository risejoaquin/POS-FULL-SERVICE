# POS Offline Sync Idempotency Key Strategy Baseline

Status: PHASE 4C baseline.

This document defines the **offline sync idempotency key strategy baseline only** for future POS offline synchronization work.

## Scope

This phase is diagnostic/design only:

- no production sync execution
- no queue writes
- no inventory mutation
- no checkout changes
- no schema change
- no migrations
- no real conflict resolution

## Required checks

- deterministic event identity documented
- tenant id included in key scope
- device id included in key scope
- local event id included in key scope
- entity type included in key scope
- entity id included in key scope
- operation type included in key scope
- created at timestamp reviewed
- idempotency key immutability documented
- duplicate submission handling documented
- retry reuse of same key documented
- conflict-safe server behavior documented
- operator-safe duplicate message documented

## Proposed key shape

The future idempotency key should be deterministic and stable across retries:

```text
{tenant_id}:{device_id}:{local_event_id}:{entity_type}:{entity_id}:{operation_type}
```

The same offline event must reuse the same key on every retry. A duplicate submission must be treated as already processed or safely rejected with an operator-safe message.

## Guardrails

This baseline does not write queue entries and does not execute production sync. It only prepares the strategy needed before future implementation.
