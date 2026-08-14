# POS Offline Sync Observability & Correlation Baseline

**Scope:** offline sync observability and correlation baseline only.

This document defines the baseline for observing offline sync without running production sync. It is a design/diagnostic checkpoint, not an execution mechanism.

## Required observability fields

- correlation id strategy documented
- tenant id log scope documented
- device id log scope documented
- user session log scope documented
- sync operation id documented
- queue item id log scope documented
- idempotency key log scope documented
- retry attempt log scope documented
- backoff delay log scope documented
- conflict detection result log scope documented
- checkpoint state log scope documented
- last success state log scope documented
- ownership mismatch logging documented
- operator-safe sync diagnostic message documented
- sensitive data redaction documented
- structured log fields documented

## Correlation model

Every future sync attempt should be traceable using a correlation id across client, queue diagnostic, retry/backoff decision, conflict detection, checkpoint evaluation and server response.

Recommended fields:

```text
correlation_id
tenant_id
device_id
user_session_id
sync_operation_id
queue_item_id
idempotency_key
retry_attempt
backoff_delay_ms
conflict_detection_result
checkpoint_state
last_success_state
ownership_validation_result
operator_safe_message
```

## Redaction rule

Logs must avoid raw payment data, credentials, tokens, customer secrets and full personally sensitive payloads. Diagnostic messages must be operator-safe.

## Hard stops

- no production sync execution
- no queue writes
- no telemetry emission
- no checkpoint advancement
- no checkout changes
- no inventory mutation
- no schema change
- no migrations

## Status

PHASE 4H prepares observability and correlation only. It does not enable real sync behavior.
