# PHASE 4G — POS Offline Sync Tenant/Device Boundary & Sync Ownership Baseline

## Status

PENDING LOCAL VERIFICATION

## Purpose

Prepare tenant/device boundary and sync ownership decisions before allowing future production offline sync execution.

## Guardrails

- No production sync execution
- No queue writes
- No sync ownership claim
- No checkpoint advancement
- No inventory mutation
- No checkout changes
- No schema change
- No migrations

## Baseline decisions

- tenant id boundary
- device id boundary
- user/session boundary
- local queue owner
- sync ownership boundary
- single writer ownership rule
- ownership mismatch rejection
- checkpoint ownership validation

## Roadmap impact

PHASE 4F moved the offline sync reliability block from 50% -> 60%.
PHASE 4G moves the offline sync reliability block from 60% -> 70% after local verification.

PHASE 4H remains blocked until PHASE 4G passes local verification.
