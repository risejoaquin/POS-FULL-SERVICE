# POS Production Sync Server Acknowledgement & Checkpoint Commit Baseline

**Scope:** production sync server acknowledgement checkpoint commit baseline only.

This baseline defines the safety contract for server acknowledgements and checkpoint commits before any real production sync can advance local checkpoints.

## Required checks

- production sync server acknowledgement checkpoint commit baseline documented
- server acknowledgement contract documented
- acknowledgement status validation documented
- server accepted state documented
- server rejected state documented
- durable acknowledgement evidence documented
- correlation id acknowledgement matching documented
- idempotency key acknowledgement matching documented
- tenant id acknowledgement matching documented
- device id acknowledgement matching documented
- queue item id acknowledgement matching documented
- checkpoint commit boundary documented
- checkpoint commit after acknowledgement documented
- no checkpoint commit on partial failure documented
- retry/backoff after rejected acknowledgement documented
- dead-letter after terminal rejection documented
- manual recovery handoff after ambiguous acknowledgement documented
- operator-safe acknowledgement message documented

## Acknowledgement evidence fields

- `correlation_id`
- `tenant_id`
- `device_id`
- `queue_item_id`
- `idempotency_key`
- `server_acknowledgement_id`
- `acknowledgement_status`
- `server_accepted_at`
- `server_rejected_reason`
- `checkpoint_candidate`
- `checkpoint_commit_decision`
- `failure_handoff`

## Commit rule

A checkpoint can only be committed after a durable server acknowledgement confirms the expected tenant, device, queue item, idempotency key and correlation id.

Partial failure, ambiguous acknowledgement, missing acknowledgement evidence or rejected acknowledgement must block checkpoint commit and route to retry/backoff, dead-letter or manual recovery according to the documented handoff.

## Hard stops

- No production sync execution
- No queue writes
- No acknowledgement send
- No checkpoint commit
- No checkout changes
- No inventory mutation
- No schema change
- No migrations
