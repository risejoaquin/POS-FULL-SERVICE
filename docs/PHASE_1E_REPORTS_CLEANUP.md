# PHASE 1E — ReportsViewModel Cleanup

## Goal
Move report data queries and calculations out of `PosCore/ViewModels/ReportsViewModel.cs` and behind the local application port `IReportsService`.

## Scope
Modified only the reports area and dependency registration. No checkout, returns, inventory operations, sync, domain, server, migrations, licensing, provisioning, or builder logic was changed.

## Result
`ReportsViewModel` now depends on `IReportsService` and keeps only UI orchestration, observable collections, filter dates, and export commands.

`ReportsService` in Infrastructure encapsulates `PosDbContext`, EF Core includes, queries, grouping, payment parsing, low-stock lookups, shift history, and cash movement retrieval.

## Local validation required
Run:

```powershell
dotnet test
dotnet build -c Release Pos.sln
```

Acceptance gate:

- Tests: 0 failed
- Release build: 0 errors
