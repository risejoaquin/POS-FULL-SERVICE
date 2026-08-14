# POS Production Sync Conflict Detection Runtime Implementation

## Scope

This document defines the **POS Production Sync Conflict Detection Runtime Implementation** for PHASE 6G.

The scope is controlled conflict detection only. It does not execute production sync, enable sync, automatically resolve conflicts, commit real checkpoints, write queue payloads, process items, mutate inventory, change checkout, change schema, or run migrations.

## Required checks

- production sync conflict detection runtime implementation documented
- conflict detection contract documented
- local version evidence documented
- server version evidence documented
- checkpoint comparison documented
- tenant scoped conflict detection documented
- device scoped conflict detection documented
- queue item conflict matching documented
- lease ownership conflict guard documented
- idempotency key conflict guard documented
- correlation id conflict evidence documented
- durable acknowledgement prerequisite documented
- checkpoint prerequisite documented
- conflict classification documented
- conflict result audit evidence documented
- manual resolution handoff documented
- operator approval evidence documented
- no automatic conflict resolution documented
- operator-safe conflict message documented

## Evidence package

- tenant_id
- device_id
- operator_id
- queue_item_id
- lease_owner
- local_version_state
- server_version_state
- checkpoint_comparison_state
- conflict_classification
- manual_resolution_state
- idempotency_key
- correlation_id

## Conflict classification

1. No conflict: local and server version evidence match.
2. Version conflict: local_version_state differs from server_version_state.
3. Checkpoint conflict: checkpoint_comparison_state is behind or ambiguous.
4. Ownership conflict: lease_owner does not match the expected tenant/device scope.
5. Manual resolution required: automated resolution is intentionally blocked.

## Hard stops

- No production sync execution
- No sync enablement
- No automatic conflict resolution
- No real checkpoint commit
- No queue payload writes
- No item processing
- No inventory mutation
- No checkout changes
- No schema change
- No migrations

## Operator-safe message

Conflict detection runtime is prepared in controlled mode. No conflict was resolved automatically, no inventory was mutated, no queue payload was written, and no checkpoint was committed. Provide support with tenant_id, device_id, queue_item_id, lease_owner, local_version_state, server_version_state, checkpoint_comparison_state, idempotency_key and correlation_id.
