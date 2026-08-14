# MACROFASE 13C V2 - Metrics Short-Circuit Hotfix

## Status

BLOCKED issue resolved by patch design.

## Problem

After redeploy, production still returned HTTP 500 for `/metrics`. This means the previous minimal route mapping was not early enough to prevent another metrics endpoint or middleware path from being selected before the expected deterministic 404 behavior.

## Fix

`PosServer/Program.cs` now short-circuits `/metrics` and `/health/metrics` before endpoint routing, authentication, tenant middleware, controller mapping, and static file handling.

Expected production result:

```text
/metrics -> 404
/health/metrics -> 404
```

## Non-goals

This patch does not modify migrations, Supabase, checkout, inventory, sales, returns, sync, or any endpoint that mutates data.
