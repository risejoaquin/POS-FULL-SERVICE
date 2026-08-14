# MACROFASE 13B verifier parser hotfix V5

This hotfix replaces the MACROFASE 13B verifier with a PowerShell parser-safe implementation.

## Root cause

The previous verifier revisions failed before executing validation because PowerShell parsed documentation marker strings as code. The failures were caused by a combination of:

- wildcard matching against literal strings that contained `[` and `*`;
- interpolation with `$Path:` instead of `${Path}:`;
- fragile marker groups with Markdown formatting variants;
- malformed string delimiters around marker arrays.

## Fix

The verifier now uses:

- `Get-Content -LiteralPath ... -Raw`;
- `$content.Contains($marker)` for literal text checks;
- `${Path}` and `${Text}` style interpolation where needed;
- flexible marker groups for Markdown variants;
- ASCII-only output messages.

## Scope

This is a verifier-only hotfix. It does not change API behavior, database schema, Railway deployment, production data, checkout, inventory, sync, or authentication behavior.

## Expected result

```text
MACROFASE 13B API endpoint inventory contract validation markers verified.
Verifier hotfix V5 verified: parser-safe, literal Contains checks, no wildcard parsing, safe colon interpolation, no smart dash parser breaks.
```
