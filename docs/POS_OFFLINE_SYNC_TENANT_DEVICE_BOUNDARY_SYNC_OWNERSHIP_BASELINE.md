# POS Offline Sync Tenant/Device Boundary & Sync Ownership Baseline

## Scope

This phase is **offline sync tenant device boundary and sync ownership baseline only**.

It defines how future offline sync work must bind every queued event, checkpoint, retry, idempotency key, and conflict check to the correct tenant/device ownership boundary.

## Required checks

- tenant id boundary documented
- device id boundary documented
- user session boundary documented
- local queue owner documented
- sync ownership boundary documented
- single writer ownership rule documented
- device registration requirement reviewed
- tenant mismatch rejection documented
- device mismatch rejection documented
- queue item ownership validation documented
- checkpoint ownership validation documented
- idempotency key tenant device scope documented
- retry/backoff tenant device scope documented
- conflict detection tenant device scope documented
- operator-safe ownership mismatch message documented
- correlation id logging reviewed
- no production sync execution
- no queue writes
- no sync ownership claim
- no checkpoint advancement
- no checkout changes
- no inventory mutation
- no schema change
- no migrations

## Ownership rule

A POS device must only process offline queue entries that match its tenant id, device id, user/session boundary, and local queue owner.

## Single writer rule

Only one active sync owner may process a given tenant/device queue partition at a time.

## Safety rule

Ownership mismatch must reject processing and send the event to operator-safe review. Detection is allowed; automatic ownership correction is not allowed.

## Explicit non-goals

- no production sync execution
- no queue writes
- no sync ownership claim
- no checkpoint advancement
- no checkout changes
- no inventory mutation
- no schema change
- no migrations
