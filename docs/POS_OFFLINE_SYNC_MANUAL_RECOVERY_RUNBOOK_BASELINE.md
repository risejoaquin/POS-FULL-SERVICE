# POS Offline Sync Manual Recovery Runbook Baseline

**Scope:** offline sync manual recovery runbook baseline only.

This document defines the manual recovery runbook for offline sync incidents before any real recovery execution exists. It is intentionally design-only and operator-safe.

## Purpose

The POS must not auto-recover problematic offline sync states without evidence, ownership validation, checkpoint protection, and explicit approval. Manual recovery starts only after the operator/admin has collected enough evidence to understand the queue state, retry/backoff state, conflict detection result, checkpoint state, last success state, tenant/device boundary, and correlation id chain.

## Required evidence

- manual recovery entry criteria documented
- operator triage workflow documented
- queue snapshot before recovery documented
- checkpoint freeze before recovery documented
- correlation id evidence collection documented
- tenant id evidence collection documented
- device id evidence collection documented
- queue item id evidence collection documented
- idempotency key validation documented
- retry/backoff state review documented
- conflict detection state review documented
- dead-letter review workflow documented
- manual recovery approval requirement documented
- support handoff package documented
- operator-safe recovery message documented
- rollback prohibition documented

## Manual recovery entry criteria

Manual recovery is allowed to be considered only when one or more conditions exist:

1. Offline queue contains failed items past retry/backoff thresholds.
2. Conflict detection blocks automatic processing.
3. Ownership validation detects a tenant/device mismatch.
4. Checkpoint state cannot safely advance.
5. Last success state is stale or inconsistent with queue diagnostics.
6. Operator receives an operator-safe sync diagnostic message requiring intervention.

## Mandatory procedure

1. Stop automated sync attempts for the affected device/tenant partition.
2. Capture a queue snapshot before recovery.
3. Freeze checkpoint state before recovery.
4. Collect correlation id, tenant id, device id, sync operation id and queue item ids.
5. Validate idempotency key reuse and duplicate replay risk.
6. Review retry/backoff state and dead-letter status.
7. Review conflict detection result.
8. Validate tenant/device ownership before any intervention.
9. Prepare support handoff package.
10. Require manual approval before future recovery execution.

## Explicit prohibitions

- no production sync execution
- no queue writes
- no manual recovery execution
- no checkpoint advancement
- no checkout changes
- no inventory mutation
- no schema change
- no migrations

## Operator-safe message

When manual recovery is required, the operator should see a safe message similar to:

> La sincronización offline requiere revisión manual. No se modificó inventario, no se escribió la cola y no se avanzó checkpoint. Contacta soporte con el correlation id y el snapshot de cola.

## Recovery approval rule

Manual recovery cannot execute until the recovery package includes queue snapshot, checkpoint freeze evidence, idempotency validation, ownership validation, correlation id chain, conflict/retry state and explicit approval.
