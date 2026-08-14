# POS Offline Sync Reliability Baseline

## Scope

This document defines the PHASE 4A offline sync reliability baseline. It is **offline sync reliability baseline only**.

It does not execute production sync, does not write sync queues, does not mutate inventory, does not change checkout, and does not require schema change or migrations.

## Required reliability checks

- offline queue inventory reviewed
- idempotency key strategy reviewed
- retry backoff policy documented
- conflict detection strategy documented
- sync checkpoint and last-success state reviewed
- tenant boundary validation reviewed
- observability correlation id reviewed
- operator-safe failure message documented
- no checkout changes
- no inventory mutation
- no schema change
- no migrations
- no production sync execution

## Operational rules

1. Every offline sync item must be traceable to tenant scope.
2. Every pushed order/payment must have a stable idempotency key strategy.
3. Retry behavior must use bounded backoff and must avoid duplicate business effects.
4. Conflict detection must be explicit before conflict resolution is implemented.
5. The last successful sync checkpoint must be observable before automatic recovery is added.
6. Operator-facing errors must be safe and actionable.

## Non-goals

- no production sync execution
- no inventory mutation
- no checkout changes
- no sync engine rewrite
- no schema change
- no migrations
- no conflict resolution execution
