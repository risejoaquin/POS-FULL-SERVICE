# POS Security Dependency Hardening

## Scope

PHASE 7A applies the first security hardening slice after PHASE 6J closed the controlled production sync implementation block.

This phase remediates the known `System.Text.Json` dependency vulnerability warning in `PosBuilder` by replacing the vulnerable `8.0.0` package reference with the patched .NET 8 compatible `8.0.5` package reference.

## Required checks

- security dependency hardening documented
- System.Text.Json vulnerability remediation documented
- System.Text.Json 8.0.0 removed from PosBuilder
- System.Text.Json 8.0.5 pinned in PosBuilder
- GHSA-8g4q-xg66-9fp4 remediation tracked
- GHSA-hh2w-p6rv-4g7w remediation tracked
- dependency update scope limited to PosBuilder
- no checkout behavior change
- no inventory mutation
- no production sync enablement
- no schema change
- no migrations
- operator-safe dependency hardening message documented

## Dependency change

| Project | Package | Previous | Patched |
|---|---|---:|---:|
| PosBuilder | System.Text.Json | 8.0.0 | 8.0.5 |

## Safety boundaries

- No checkout behavior change
- No inventory mutation
- No production sync enablement
- No queue processing change
- No checkpoint behavior change
- No schema change
- No migrations

## Operator-safe message

Security dependency hardening prepared: `System.Text.Json` is pinned to `8.0.5` in `PosBuilder`. This is a dependency remediation only and does not change checkout, inventory, production sync, schema, or migrations.
