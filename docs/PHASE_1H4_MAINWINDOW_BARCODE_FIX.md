# PHASE 1H.4 Hotfix — MainWindow Barcode Boundary

## Cause
`MainViewModel.DbContext` was removed as part of PHASE 1H.4, but `MainWindow.xaml.cs` still referenced it when creating `BarcodeProcessor`.

## Fix
`MainWindow` now receives `IProductLookupService` through DI and creates `BarcodeProcessor` with the application port instead of using `MainViewModel.DbContext`.

## Files Modified
- `PosCore/Views/MainWindow.xaml.cs`

## Scope
- No checkout transaction changes.
- No inventory service changes.
- No domain/server/migration changes.
- No rollback of MainViewModel cleanup.
