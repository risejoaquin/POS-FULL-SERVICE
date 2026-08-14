# MACROFASE 13B - Verifier Colon Interpolation Hotfix V3

This patch fixes the PowerShell parser error caused by interpolating `$Path:` inside a double-quoted string.

PowerShell treats `$Path:` as a scoped variable expression. The verifier now uses `${Path}: ${Text}` to safely format the error message.

No production logic is changed. No API endpoints, migrations, database schema, checkout, inventory, sales, sync, or tenant behavior are modified.

Expected verifier output:

```text
MACROFASE 13B API endpoint inventory contract validation markers verified.
Wildcard parser hotfix V3 verified: literal string checks use .Contains(), -LiteralPath, and safe variable interpolation.
PowerShell colon interpolation hotfix V3 verified.
```
