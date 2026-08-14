# Inventory Drift Reporting

## Purpose

Inventory drift reporting exposes drift detection as a read-only internal diagnostic service.

It compares operational inventory columns against the reconstructed inventory ledger and returns an `InventoryDriftReport`.

## Service

The reporting integration is exposed through:

```csharp
IInventoryDriftReportingService
```

Implemented by:

```csharp
InventoryDriftReportingService
```

## Methods

```csharp
GetProductDriftReportAsync(...)
GetSupplyDriftReportAsync(...)
GetCombinedDriftReportAsync(...)
```

## Calculation path

The service reads current operational quantities:

```text
Product.StockQuantity
Supply.Stock
```

It also reads:

```text
InventoryMovement
```

Then it delegates calculation to:

```text
InventoryDriftDetectionReadModel
InventoryLedgerReadModel
InventoryMovement.SignedQuantity
```

## Safety boundaries

This service is diagnostic only.

It does not auto-correct stock.
It does not call SaveChanges.
It does not mutate Product.StockQuantity.
It does not mutate Supply.Stock.
It does not change checkout.
It does not change sync.
It does not change returns.

## No schema change

This is a no schema change integration baseline.

No migrations, tables, columns, indexes, constraints, or EF mappings are added.

## Current limitation

The report is available internally through dependency injection, but there is no UI screen or API endpoint yet.

That is intentional. A future phase can expose it safely after the diagnostic service is validated locally.
