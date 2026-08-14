# POS Production Sync Dead-Letter Queue Persistence Implementation

Scope: production sync dead-letter queue persistence implementation controlled only.

This phase defines the controlled persistence contract for dead-letter queue records after conflict detection, retry exhaustion and manual intervention prerequisites. It does not execute production sync and does not perform automatic replay.

Required evidence fields:

- tenant_id
- device_id
- operator_id
- queue_item_id
- lease_owner
- dead_letter_reason_code
- retry_exhaustion_state
- manual_intervention_state
- payload_snapshot_state
- idempotency_key
- correlation_id

Required controls:

- dead-letter queue persistence contract documented
- dead-letter record envelope documented
- dead-letter reason code documented
- tenant scoped dead-letter persistence documented
- device scoped dead-letter persistence documented
- queue item dead-letter matching documented
- lease ownership dead-letter guard documented
- idempotency key dead-letter guard documented
- correlation id dead-letter evidence documented
- conflict detection prerequisite documented
- manual intervention prerequisite documented
- retry exhaustion prerequisite documented
- payload snapshot redaction documented
- dead-letter audit evidence documented
- dead-letter replay prohibition documented
- operator approval evidence documented

Hard stops:

- No production sync execution
- No sync enablement
- No automatic replay
- No item processing
- No queue payload mutation
- No real checkpoint commit
- No inventory mutation
- No checkout changes
- No schema change
- No migrations

Operator-safe message: a queue item can only be marked for dead-letter persistence after evidence is collected and reviewed. Payload snapshots must be redacted, replay is prohibited until manual approval, and inventory remains unchanged.
