# POS Offline Sync Retry Backoff Policy Baseline

## Scope

This is an offline sync retry backoff policy baseline only. It documents the retry/backoff policy required before production offline sync execution is enabled.

## Required checks

- retryable error classification documented
- non retryable error classification documented
- exponential backoff policy documented
- jitter strategy documented
- max retry attempts documented
- retry attempt counter reviewed
- next retry at decision documented
- dead letter/manual review threshold documented
- operator-safe retry failure message documented
- idempotency key reuse during retry documented
- tenant boundary validation reviewed
- correlation id logging reviewed

## Retry policy baseline

Future sync execution should classify errors before retrying. Network timeouts, transient server failures and temporary connectivity loss may be retryable. Validation errors, tenant mismatch, authorization failures and malformed payloads should be non retryable and moved toward manual review.

The future policy should use exponential backoff with jitter, a max retry attempts limit, and a dead letter/manual review threshold. The same idempotency key must be reused for every retry of the same offline event.

## Guardrails

- no production sync execution
- no queue writes
- no checkout changes
- no inventory mutation
- no schema change
- no migrations
- no conflict resolution execution

## Operator copy

When retries are exhausted, the operator-facing message must be safe: it should explain that sync is delayed and requires review, without exposing stack traces or internal payloads.
