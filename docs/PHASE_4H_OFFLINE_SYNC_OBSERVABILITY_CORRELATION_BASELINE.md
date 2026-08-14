# PHASE 4H — Offline Sync Observability & Correlation Baseline

## Objective

Prepare the POS offline sync observability and correlation baseline.

## Outcome

The phase adds a helper contract, ViewModel state, UI copy, architecture guardrails and documentation for correlation id, structured logs, tenant/device scope, queue item scope, idempotency key scope, retry/backoff scope, conflict detection scope and checkpoint/last-success state scope.

## Explicit non-goals

- No production sync execution
- No queue writes
- No telemetry emission
- No checkpoint advancement
- No checkout changes
- No inventory mutation
- No schema change
- No migrations

## Progress

Offline Sync Reliability moves from 70% -> 80% after local verification.

## Next phase

PHASE 4I — Offline Sync Manual Recovery Runbook.
