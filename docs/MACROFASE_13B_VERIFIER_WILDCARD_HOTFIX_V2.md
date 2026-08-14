# MACROFASE 13B Verifier Wildcard Hotfix V2

This patch fixes the PowerShell verifier for MACROFASE 13B.

## Problem

The previous verifier used `-notlike "*$text*"` with marker text that included wildcard-sensitive characters such as `[` and `*`.
Windows PowerShell treated those characters as wildcard syntax and failed before completing validation.

## Fix

The verifier now uses:

- `Get-Content -LiteralPath ... -Raw`
- `.Contains($Text)` for literal marker checks
- ASCII-safe output markers

## Scope

This patch does not change API behavior, endpoint behavior, database schema, migrations, Railway settings, inventory logic, checkout logic, payment logic, sync logic, or tenant writes.

## Expected result

`VERIFY_MACROFASE_13B_API_ENDPOINT_INVENTORY_CONTRACT_VALIDATION.ps1` should pass and print:

```text
MACROFASE 13B API endpoint inventory contract validation markers verified.
Wildcard parser hotfix V2 verified: literal string checks use .Contains() and -LiteralPath.
```
