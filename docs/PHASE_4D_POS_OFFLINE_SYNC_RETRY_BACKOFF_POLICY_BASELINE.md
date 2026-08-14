# PHASE 4D - POS Offline Sync Retry Backoff Policy Baseline

## Result

PHASE 4D adds a protected retry/backoff policy baseline for future POS offline sync reliability.

## Added

- PosOfflineSyncRetryBackoffPolicyBaseline helper/contract.
- InventoryViewModel state and command for retry/backoff policy preparation.
- InventoryWindow retry/backoff button and diagnostic copy.
- Static guardrails for non-mutating behavior.
- Documentation and professional progress report.

## Explicit non-goals

- No queue writes
- No production sync execution
- No inventory mutation
- No checkout changes
- No schema change
- No migrations
- No conflict resolution execution

## Expected validation

PHASE 4D adds 5 static guardrails. The expected total is 210 passing tests after local validation.
