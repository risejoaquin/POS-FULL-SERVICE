# POS Production Sync Dead-Letter Queue & Manual Intervention Baseline

**Scope:** production sync dead-letter queue and manual intervention baseline only.

This document defines the dead-letter queue and manual intervention baseline for production sync failures. It is design/guardrail only.

## Required checks

- dead-letter queue contract documented
- terminal failure criteria documented
- manual intervention workflow documented
- operator assignment requirement documented
- support escalation requirement documented
- evidence package requirement documented
- correlation id evidence documented
- tenant device scope evidence documented
- idempotency key evidence documented
- queue item evidence documented
- retry history evidence documented
- conflict state evidence documented
- checkpoint freeze requirement documented
- manual resolution approval documented
- audit trail requirement documented
- operator-safe dead-letter message documented

## Evidence package

Every future dead-letter handoff must preserve: `queue_item_id`, `correlation_id`, `tenant_id/device_id`, `idempotency_key`, `retry_history`, `conflict_state`, `checkpoint_candidate`, `failure_classification`, `operator_assignment`, `support_escalation_reference`, and `operator_safe_dead_letter_message`.

## Manual intervention rule

Manual intervention must not mutate inventory, requeue events, move dead-letter items, confirm checkpoints, or change checkout without explicit approval, operator assignment, tenant/device evidence, idempotency evidence, and an audit trail.

## Hard stops

- No production sync execution
- No queue writes
- No dead-letter move
- No manual intervention execution
- No checkpoint commit
- No checkout changes
- No inventory mutation
- No schema change
- No migrations
