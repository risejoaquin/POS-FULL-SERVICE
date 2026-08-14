# PHASE 3N Hotfix V3

Este paquete contiene el identificador literal requerido por el guardrail:

`InventoryDriftReconciliationAuditRequired`

Si al extraer el ZIP el test sigue fallando, ejecuta desde la raíz del proyecto extraído:

```powershell
.\APPLY_PHASE_3N_HOTFIX_V3.ps1
Select-String -Path .\PosCore\ViewModels\InventoryViewModel.cs -Pattern "InventoryDriftReconciliationAuditRequired"
dotnet test
dotnet build -c Release Pos.sln
```
