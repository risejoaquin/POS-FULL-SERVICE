# PHASE 4C — POS Offline Sync Idempotency Key Strategy Baseline

## Result

PHASE 4C prepares the POS Offline Sync Idempotency Key Strategy Baseline.

## Safety boundaries

- No production sync execution
- No queue writes
- No inventory mutation
- No checkout changes
- No schema change
- No migrations
- No real conflict resolution

## Added design

- Deterministic event identity
- Tenant id scope
- Device id scope
- Local event id scope
- Entity type and entity id scope
- Operation type scope
- Retry reuse of the same key
- Duplicate handling strategy
- Conflict-safe server behavior
- Operator-safe duplicate message

## Notes

This is a protected baseline only. It does not change sync execution and does not persist queue data.
