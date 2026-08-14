# POS Offline Sync Checkpoint & Last Success State Baseline

## Scope

This is **offline sync checkpoint and last success state baseline only**.

It defines checkpoint, last-success, resume-safety, and recovery-state requirements before real sync behavior is changed.

## Required checks

- checkpoint strategy documented
- last successful sync timestamp reviewed
- last successful queue item id reviewed
- last successful server cursor reviewed
- resume from checkpoint behavior documented
- partial sync failure state documented
- atomic checkpoint update documented
- checkpoint rollback safety documented
- duplicate replay prevention documented
- idempotency key interaction documented
- retry/backoff interaction documented
- conflict detection interaction documented
- tenant boundary validation reviewed
- device id boundary reviewed
- operator-safe resume message documented
- correlation id logging reviewed

## Baseline state shape

```text
tenant_id
device_id
last_successful_sync_at
last_successful_queue_item_id
last_successful_server_cursor
last_attempted_sync_at
last_sync_result
last_sync_error_summary
last_correlation_id
```

## Resume rule

```text
resume_from = last_successful_server_cursor or last_successful_queue_item_id
checkpoint_update = only after confirmed successful processing
```

## Safety posture

- no production sync execution
- no queue writes
- no checkpoint advancement
- no checkout changes
- no inventory mutation
- no schema change
- no migrations

## Operational principle

A checkpoint can only be advanced after the related sync operation is confirmed successful. Partial failure must preserve the previous checkpoint and require safe retry/manual review.
