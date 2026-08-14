# Super POS Express

Super POS Express es un sistema de Punto de Venta (POS) moderno, inteligente y diseñado con una arquitectura **Offline-First**. Esto significa que las sucursales pueden seguir operando y realizando ventas incluso si pierden su conexión a internet, sincronizando los datos automáticamente una vez que la conexión se restablece.

## 🏗️ Arquitectura del Proyecto

La solución está dividida en tres proyectos principales:

1. **`PosCore`**: La aplicación de escritorio (cliente). Construida con WPF y .NET 8. Utiliza una base de datos SQLite local para garantizar el funcionamiento offline.
2. **`PosServer`**: El servidor central (API). Construido con ASP.NET Core 8. Se encarga de centralizar las ventas, manejar el catálogo global de productos, los insumos y la autenticación. Utiliza PostgreSQL como base de datos principal.
3. **`PosCore.Tests`**: Proyecto de pruebas unitarias para asegurar la calidad y estabilidad de la lógica de negocio y la sincronización.

## 🚀 Características Principales y Capacidades

El sistema cuenta actualmente con las siguientes funcionalidades operativas:

*   **Punto de Venta (Ventas y Cobro)**: Interfaz intuitiva para añadir productos al carrito, modificar cantidades y completar ventas rápidamente.
*   **Módulo de Pagos Avanzado**: Ventana de cobro con teclado numérico táctil (Numpad), cálculo de cambio automático, cobro exacto y simulación de programa de lealtad (búsqueda de clientes por teléfono).
*   **Suspensión y Retoma de Órdenes**: Capacidad de guardar ventas en proceso (en espera) y retomarlas más tarde, ideal para no bloquear la caja.
*   **Descuentos y Modificadores**: Permite agregar notas personalizadas por producto y aplicar descuentos directos en pesos o porcentajes al subtotal. Sincronización completa de **Modificadores de Producto** (ProductModifiers) para personalización compleja.
*   **Gestión de Insumos y Recetas (Supplies & RecipeItems)**: Control del costo y disponibilidad de insumos (Supplies), permitiendo deducir cantidades específicas mediante Recetas (RecipeItems) asociadas a cada producto. Completamente sincronizado con el servidor.
*   **Autorización de Gerente**: Ventanas de control de acceso por PIN para operaciones sensibles y registro de motivos en caso de anulaciones y devoluciones.
*   **Impresión de Tickets Directa (Térmica)**: Impresión nativa mediante comandos RAW (ESC/POS y `winspool.drv`) hacia impresoras térmicas en entornos Windows. Incluye reimpresión de tickets.
*   **Feedback de Red, Monitoreo y Hardware**: 
    - Indicadores visuales en tiempo real del estado de conexión (Online/Offline) y banners de advertencia sobre problemas con la impresora.
    - Emisión periódica de pulsos (Ping/Heartbeat) al servidor para monitoreo de memoria, versión de la app y estado de la impresora.
*   **Gestión de Inventario**: Control de existencias, umbrales de stock mínimo (`MinStockThreshold`) y alertas visuales.
*   **Arqueo y Turnos**: Apertura y cierre de turnos de caja, registro de saldos iniciales, cálculo de dinero esperado contra el real e historial de diferencias (Cash Movements).
*   **Devoluciones**: Proceso de devoluciones de órdenes previas, regresando la mercancía al inventario y generando "Notas de Crédito" impresas directamente en la ticketera.
*   **Reportes y Cierres**: Generación de reportes de ventas, listado de órdenes y cierres diarios.
*   **Módulo de Logs**: Visor integrado de registros (logs) del sistema, que permite auditar errores, sincronizaciones, eventos de red o problemas con la impresora.
*   **Offline-First y Tolerancia a Fallos**: Operación ininterrumpida sin internet. Las transacciones se guardan en un sistema *Outbox* con SQLite local, para ser sincronizadas posteriormente (`SyncService`) de manera transparente.
*   **Gestión Multi-Tenant**: Soporte para múltiples sucursales con identificadores únicos, asegurando el aislamiento de datos por cada inquilino.
*   **Auto-Updater Integrado**: Integración con un sistema de actualizaciones en segundo plano, revisando periódicamente versiones en el servidor e instalándolas de forma desatendida tras reiniciar.

## 🛠️ Requisitos Previos

Para compilar y ejecutar este proyecto en tu entorno de desarrollo, necesitas:

*   [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
*   [Visual Studio 2022](https://visualstudio.microsoft.com/) o IDE compatible (como Rider o VS Code con la extensión de C#).
*   PostgreSQL (para el servidor) o una base de datos relacional compatible.
*   Sistema operativo **Windows** (requerido para impresión nativa hacia ticketeras `winspool.drv` y WPF).

## 💻 Cómo Ejecutar en Desarrollo

### 1. Levantar el Servidor (PosServer)

1. Navega a la carpeta del servidor: `cd PosServer`
2. Configura tu cadena de conexión a PostgreSQL en `appsettings.json` o `appsettings.Development.json`.
3. Aplica las migraciones: `dotnet ef database update`.
4. Ejecuta el servidor: `dotnet run` (Ej. `http://localhost:5000`).

### 2. Levantar el Cliente de Escritorio (PosCore)

1. Navega a la carpeta del cliente: `cd PosCore`
2. Configura los parámetros en `appsettings.json`:
   - `ApiSettings:BaseUrl`: URL de tu servidor local.
   - `Printer:PortName`: Nombre de la impresora (Ej. `POS-80` o `COM1`).
3. Ejecuta la aplicación: `dotnet run`
   - Se abrirá la interfaz gráfica (WPF). Al iniciar sesión, se creará la BD local y se sincronizarán los datos en segundo plano.


## PHASE 1F.2

Inventory write operations cleanup: product create/update/delete/import now go through IInventoryAppService. Supplies/recipes/modifiers remain deferred.

## PHASE 1H.1 — MainViewModel Audit + Product Read Boundary

Started the MainViewModel / Checkout cleanup by moving product catalog reads behind `IInventoryAppService`. Checkout transaction logic remains intentionally unchanged for later subphases.

## PHASE 1H.2 — Checkout Request/Payment Boundary Extraction

- Added checkout request/payment DTO boundary.
- MainViewModel now builds `CheckoutRequest` before persisting the sale.
- Checkout transaction, stock, cash movement, outbox and receipt printing remain unchanged for safety.

## PHASE 1H.3 — Checkout Transaction Extraction

- Added `PosInfrastructure/Services/Local/LocalOrderService.cs` implementing `ILocalOrderService`.
- Moved checkout transaction, stock/supply decrement, inventory movements, payments, order creation, cash movement, save/retry and rollback cleanup out of `MainViewModel`.
- `MainViewModel` keeps UI state, payment window, request construction, receipt printing and user notifications.
- See `docs/PHASE_1H3_CHECKOUT_TRANSACTION_EXTRACTION.md`.

## PHASE 1H.4 — Remove remaining DbContext/EF from MainViewModel

Removed residual direct `PosDbContext` / EF dependencies from `MainViewModel` after checkout extraction. MainViewModel now depends on application/UI services for checkout and product reads, not direct database infrastructure.

Validation required:

```powershell
dotnet test
dotnet build -c Release Pos.sln
```


## Phase 2A — Domain + Money Baseline Cleanup

- Updated `PosDomain.ValueObjects.Money` to store integer minor units internally.
- Preserved decimal `Amount` accessor for compatibility.
- Added `MoneyTests` covering rounding, arithmetic, currency normalization, and invalid currency.
- No migrations, EF mappings, checkout, returns, reports, sync, or entity monetary fields were changed.

## PHASE 2B — Domain Entity Audit + Safe Invariants

Added safe domain helper methods and tests for Order, Product, Payment, CashMovement and CashRegisterShift without changing EF mappings or migrations.


## Phase 2C — Order / Payment / Cash Domain Alignment

- Added conservative domain helpers for payment totals, cash/card totals, balance due and full payment state.
- Added payment state helpers and refund transition guard.
- Added cash movement factories and reason validation.
- Added focused domain tests.
- No EF mappings, migrations, checkout, returns, reports, sync, PosServer or PosBuilder changes.


## PHASE 2D — Product / Inventory Domain Alignment

Added conservative domain invariants for `Product`, `Supply`, `RecipeItem`, and `InventoryMovement`, plus domain tests. Persistence, migrations, checkout, returns, sync, server and builder were not changed.

## PHASE 2E — Domain Contamination Static Pass

Added conservative architecture tests ensuring `PosDomain` does not reference or expose UI, Infrastructure, Server, EF Core, WPF, or ASP.NET Core types. Removed the empty placeholder `PosDomain/Class1.cs`. EF/DataAnnotations and transport DTO placement were documented as future debt, not changed in this phase.

## PHASE 2F — Domain Documentation + Integration Safety Pass

Documented current domain rules and integration safety boundaries in `docs/DOMAIN_RULES.md`. No behavior-changing code, EF mappings, migrations, checkout, returns, sync, server, builder, or decimal money columns were modified.

## PHASE 3A — Inventory Ledger / Concurrency Baseline Audit

Status: CLOSED

This phase adds an inventory/concurrency baseline audit and architecture tests only. It documents current stock mutation paths, existing transaction/concurrency safeguards, and risks before any behavioral inventory-ledger refactor.

No migrations, EF mappings, checkout behavior, returns behavior, sync behavior, reports, or schema changes were made.


## PHASE 3B — Inventory Mutation Guardrails

Replaced direct local stock arithmetic in checkout/inventory paths with domain guardrails (`DecreaseStock`, `IncreaseStock`, `CanFulfill`, `RequiredFor`) while preserving existing transactions, migrations, EF mappings, sync protocol and `InventoryMovement.Quantity` semantics. Central server sync stock mutation remains documented debt for a dedicated sync conflict phase.

## PHASE 3C — InventoryMovement Sign Semantics Audit

This phase documents and stabilizes `InventoryMovement.Quantity` sign interpretation without changing existing data, migrations, reports or sync behavior.

Key additions:

- `AbsoluteQuantity`
- `SignedQuantity` normalization
- `HasLegacyNegativeStoredQuantity`
- `HasCanonicalPositiveStoredQuantity`
- `StockDirection`
- `ValidateForLedgerInterpretation()`
- Sign-semantics documentation and tests

Canonical future convention: store positive absolute quantities and use movement type/sign helpers for stock delta interpretation.

## PHASE 3D — Inventory Ledger Read Model Baseline

Introduced a read-only inventory ledger read model that reconstructs product and supply balances using `InventoryMovement.SignedQuantity`. This phase does not replace `Product.StockQuantity` or `Supply.Stock`; it only provides a safe baseline for future drift detection and ledger-backed inventory work.

## PHASE 3E — Inventory Drift Detection Baseline

Added a read-only drift detection baseline that compares operational stock (`Product.StockQuantity`, `Supply.Stock`) against ledger-reconstructed balances using `InventoryMovement.SignedQuantity`.

This phase does not auto-correct stock, change schema, change checkout, change sync, or replace the current operational stock source of truth.


## PHASE 3F — Inventory Drift Reporting Integration Baseline

Added a read-only local inventory drift reporting integration through `IInventoryDriftReportingService` and `InventoryDriftReportingService`. This exposes product, supply, and combined drift reports without auto-correction, schema changes, migrations, checkout changes, sync changes, or stock rebuilds.


## PHASE 3G — Inventory Drift Reporting UI/Diagnostics Hook

Added a read-only inventory drift diagnostics hook in the POS inventory screen. The hook uses `IInventoryDriftReportingService.GetCombinedDriftReportAsync` and `InventoryDriftDiagnosticsFormatter` to display drift diagnostics without auto-correction, stock mutation, schema changes, migrations, checkout changes, or sync changes.


## PHASE 3H — Inventory Drift Report UX Safety Pass

Status: PENDING LOCAL VERIFICATION.

This phase improves inventory drift diagnostic UX safety. The hook remains read-only and does not auto-correct stock. No schema change, no migrations, no checkout changes, and no sync changes were introduced.


## PHASE 3I — Inventory Drift Diagnostics Error Handling + Observability

Closed scope update: inventory drift diagnostics now record start/success/failure logs, keep last error/run state in the ViewModel, and format user-facing errors safely without stack traces by default. The feature remains read-only and diagnostic only: no schema change, no migrations, no checkout changes, no sync changes, and no automatic stock correction.

## PHASE 3J — Inventory Drift Diagnostics Export/Report Baseline

Adds a report-only baseline for copying/exporting inventory drift diagnostics.

Safety boundary:

- diagnostic only.
- report-only.
- does not auto-correct stock.
- no schema change.
- no migrations.
- no checkout changes.
- no sync changes.

Expected validation: 155 tests passed, 0 failed, 0 build errors.

## PHASE 3K — Inventory Drift Manual Review Workflow Baseline

Phase 3K adds a conservative manual review workflow for inventory drift diagnostics. It prepares the operator to review drift and export evidence, but it does not correct stock.

Safety boundaries: diagnostic only, manual review only, report-only, no auto-correction, no inventory mutation, no schema change, no migrations, no checkout changes, and no sync changes.

## PHASE 3L — Inventory Drift Controlled Manual Reconciliation Design Pass

Status: PENDING LOCAL VERIFICATION

This phase adds a design-only baseline for future controlled manual reconciliation of inventory drift. It does not execute corrections, does not mutate inventory, does not change schema, does not add migrations, does not change checkout and does not change sync.

Expected verification target: 165 tests passed, 0 failed, 0 build errors.

## PHASE 3M — Inventory Drift Reconciliation RBAC + Permission Guard Baseline

Adds a conservative permission guard baseline for future controlled inventory drift reconciliation. The phase defines the required permission `inventory.drift.reconciliation.prepare`, validates authorized roles, and exposes UI state for permission preparation. It is permission guard only: no auto-correction, no inventory mutation, no stock adjustment, no schema change, no migrations, no checkout changes, and no sync changes.

## PHASE 3N — Inventory Drift Reconciliation Audit Trail Baseline

Adds audit trail preparation for future controlled inventory drift reconciliation. The phase defines required audit fields, minimum evidence, UI state and audit preparation guardrails.

Safety boundary: audit trail baseline only, diagnostic only, manual review only, report-only, no auto-correction, no inventory mutation, no stock adjustment, no schema change, no migrations, no checkout changes and no sync changes.

Expected validation: 175 tests passed, 0 failed, 0 build errors.


## PHASE 3O — Inventory Drift Reconciliation Sync-Safe Guard Baseline

Status: PENDING LOCAL VERIFICATION. Adds sync-safe guard baseline for future controlled inventory drift reconciliation. No inventory mutation, no schema change, no checkout changes, and no sync changes.


## PHASE 3P — Inventory Drift Controlled Reconciliation Execution Design

Status: PENDING LOCAL VERIFICATION.

Prepared a controlled reconciliation execution design baseline. This is execution design only and does not execute real reconciliation, does not mutate inventory, does not change checkout, does not change sync, and does not require schema changes or migrations.


## PHASE 3Q — Inventory Drift Reconciliation Final Runbook & Operational Closure

Final runbook closure only: checklist operativo, evidencia, confirmación final y criterios de cierre. No inventory mutation, no schema change, no checkout changes, no sync changes, no real reconciliation execution.


## PHASE 4A — POS Offline Sync Reliability Baseline

Baseline de confiabilidad offline sync agregado: cola offline, idempotencia, reintentos, conflictos, checkpoint, tenant boundary y observabilidad. Alcance protegido: no production sync execution, no inventory mutation, no checkout changes, no schema change, no migrations.


## PHASE 4B — POS Offline Sync Queue Inventory & Diagnostics Baseline

Status: PENDING LOCAL VERIFICATION. Added queue inventory diagnostics baseline only. No production sync execution, no queue writes, no inventory mutation, no checkout changes, no schema change, and no migrations.


## PHASE 4C — POS Offline Sync Idempotency Key Strategy Baseline

- Status: PENDING LOCAL VERIFICATION.
- Adds idempotency key strategy baseline only.
- No production sync execution, no queue writes, no inventory mutation, no checkout changes, no schema change, no migrations.


## PHASE 4D - POS Offline Sync Retry Backoff Policy Baseline

Status: PENDING LOCAL VERIFICATION.

Adds retry/backoff policy baseline for POS offline sync reliability. Scope remains diagnostic/design only: no production sync execution, no queue writes, no checkout changes, no inventory mutation, no schema change, and no migrations.


## PHASE 4E — POS Offline Sync Conflict Detection Strategy Baseline

Status: PENDING LOCAL VERIFICATION. Adds conflict detection strategy baseline only with no production sync execution, no queue writes, no conflict resolution execution, no inventory mutation, no checkout changes, no schema change and no migrations.


## PHASE 4F — POS Offline Sync Checkpoint & Last Success State Baseline

Status: PENDING LOCAL VERIFICATION. Adds checkpoint and last-success state baseline only with no production sync execution, no queue writes, no checkpoint advancement, no inventory mutation, no checkout changes, no schema change and no migrations.

Roadmap progress: Offline Sync Reliability 50% -> 60%; overall POS stabilization approximately 78% -> 80%.


## PHASE 4G — POS Offline Sync Tenant/Device Boundary & Sync Ownership Baseline

Status: PENDING LOCAL VERIFICATION. Adds tenant/device boundary and sync ownership baseline with no production sync execution, no queue writes, no sync ownership claim, no checkpoint advancement, no inventory mutation, no checkout changes, no schema change, and no migrations. Offline Sync Reliability moves 60% -> 70% after verification.


## PHASE 4H — Offline Sync Observability & Correlation Baseline

Status: PENDING LOCAL VERIFICATION.

Adds the POS offline sync observability/correlation baseline: correlation id, structured log fields, tenant/device scope, sync operation id, queue item id, idempotency key, retry/backoff, conflict detection result, checkpoint state, last success state, ownership mismatch logging and sensitive data redaction. No production sync execution, no queue writes, no telemetry emission, no checkpoint advancement, no inventory mutation, no checkout changes, no schema change and no migrations.


## PHASE 4I — Offline Sync Manual Recovery Runbook

Status: PENDING LOCAL VERIFICATION. Adds manual recovery runbook baseline for offline sync incidents with entry criteria, operator triage, queue snapshot, checkpoint freeze, correlation evidence, tenant/device evidence, idempotency validation, retry/backoff review, conflict detection review, dead-letter review and approval requirement. No production sync execution, no queue writes, no manual recovery execution, no checkpoint advancement, no checkout changes, no inventory mutation, no schema change, no migrations. PHASE 4J BLOCKED until 4I verification.


## PHASE 4J — Offline Sync Operational Closure

PHASE 4J adds the offline sync operational closure baseline: final readiness checklist, evidence archive requirement, manual recovery closure criteria, queue health closure criteria, checkpoint closure criteria, correlation evidence, tenant/device ownership closure, idempotency closure, retry/backoff closure, conflict detection closure, observability closure, production sync enablement gate, rollback escalation path, support handoff closure and operator sign-off. It does not execute production sync, does not write queue entries, does not execute operational closure, does not advance checkpoints, does not mutate inventory, does not change checkout, does not change schema and does not add migrations.


## PHASE 5A — Production Sync Execution Gate & Safe Enablement Baseline

Status: PENDING LOCAL VERIFICATION. Adds production sync execution gate and safe enablement baseline only; no production sync execution, no queue writes, no sync enablement, no checkpoint advancement, no inventory mutation, no checkout changes, no schema change, no migrations.


## PHASE 5B — Production Sync Feature Flag & Kill Switch Baseline

Status: PENDING LOCAL VERIFICATION. Adds a design-only production sync feature flag and kill switch baseline. It documents default disabled state, tenant/device scoped flags, kill switch, safe disable behavior, emergency rollback trigger, queue processing pause, checkpoint freeze, idempotency preservation and audit logging. No production sync execution, no queue writes, no sync enablement, no runtime flag toggle, no checkpoint advancement, no checkout changes, no inventory mutation, no schema change and no migrations.


## PHASE 5C — Production Sync Canary Rollout Baseline

Production sync canary rollout baseline only. Defines canary cohort selection, tenant/device canary scope, rollout percentage cap, monitoring window, success metrics, failure thresholds, automatic pause, rollback, kill switch integration and feature flag promotion gate. No production sync execution, no queue writes, no sync enablement, no runtime flag toggle, no checkpoint advancement, no inventory mutation, no checkout changes, no schema change and no migrations.


## PHASE 5D - Production Sync Queue Processor Execution Baseline

Status: PENDING LOCAL VERIFICATION.

Adds production sync queue processor execution baseline guardrails: processor ownership, feature flag prerequisite, kill switch prerequisite, canary prerequisite, tenant/device validation, queue claim strategy, idempotency enforcement, checkpoint commit boundary, conflict/dead-letter/manual recovery handoffs, dry-run evidence, and operator-safe processor messages. This phase does not execute production sync, does not write queue entries, does not claim queue items, does not advance checkpoints, does not mutate inventory, does not change checkout, and does not change schema.


## PHASE 5E — Production Sync Server Acknowledgement & Checkpoint Commit Baseline

Adds a baseline for production sync server acknowledgement validation and checkpoint commit boundaries. This is guardrail/documentation only: no production sync execution, no queue writes, no acknowledgement send, no checkpoint commit, no checkout changes, no inventory mutation, no schema change and no migrations.


## PHASE 5F - Production Sync Conflict Resolution Execution Gate Baseline

Status: PENDING LOCAL VERIFICATION.

Adds production sync conflict resolution execution gate guardrails: conflict classification, server acknowledgement prerequisite, checkpoint prerequisite, deterministic resolution rule, manual approval requirement, tenant/device validation, correlation/idempotency evidence, queue item evidence, inventory mutation prohibition before approval, rollback plan, dead-letter/manual recovery handoffs, audit logs, and operator-safe conflict messages. This phase does not execute production sync, does not resolve conflicts, does not write queue entries, does not confirm checkpoints, does not mutate inventory, does not change checkout, and does not change schema.


## PHASE 5G — Production Sync Dead-Letter Queue & Manual Intervention Baseline

Status: Pending local verification. Adds baseline-only dead-letter queue and manual intervention guardrails for production sync enablement. No production sync execution, no queue writes, no dead-letter move, no manual intervention execution, no checkpoint commit, no inventory mutation, no checkout changes, no schema change and no migrations.


## PHASE 5G — Production Sync Dead-Letter Queue & Manual Intervention Baseline

Status: pending local verification. Adds a guarded baseline for dead-letter routing, terminal failure criteria, retry exhaustion, evidence packages, checkpoint freeze, manual intervention ownership, requeue gate, support escalation and audit logs. No production sync execution, no dead-letter processing execution, no queue writes, no dead-letter writes, no checkpoint confirmation, no inventory mutation, no checkout changes, no schema change and no migrations. Production Sync Enablement: 60% -> 70% after verification.


## PHASE 5H — Production Sync Observability Runtime Metrics Baseline

Status: PENDING LOCAL VERIFICATION. Scope: production sync observability runtime metrics baseline only. Adds runtime metrics contract, queue depth, processing latency, acknowledgement latency, checkpoint lag, retry/dead-letter/conflict/error rates, throughput, tenant/device dimensions, correlation id trace, sensitive data redaction, alert thresholds and operator dashboard. No production sync execution, no queue writes, no runtime metrics emission, no alerting configuration change, no checkpoint commit, no checkout changes, no inventory mutation, no schema change, no migrations. PHASE 5I BLOCKED until 5H verification.


## PHASE 5I — Production Sync Operational Runbook & Support Handoff Baseline

Status: PENDING LOCAL VERIFICATION.

Adds production sync operational runbook and support handoff baseline only: operational runbook documented, support handoff workflow documented, incident severity classification documented, first response checklist documented, escalation matrix documented, support evidence package documented, queue snapshot evidence documented, runtime metrics evidence documented, correlation id evidence documented, tenant/device evidence documented, idempotency key evidence documented, checkpoint state evidence documented, feature flag state evidence documented, kill switch state evidence documented, dead-letter state evidence documented, operator communication template documented, support closure criteria documented and operator-safe runbook message documented.

Guards: no production sync execution, no queue writes, no support handoff execution, no runtime operation change, no checkpoint commit, no checkout changes, no inventory mutation, no schema change, no migrations.

Production Sync Enablement moves 80% -> 90% after local verification. PHASE 5J BLOCKED until 5I verification.


## PHASE 5J — Production Sync Final Enablement Readiness Closure Baseline

Production Sync Enablement block: 90% -> 100%. This phase adds final readiness closure guardrails only: no production sync execution, no sync enablement, no queue writes, no runtime flag toggle, no checkpoint advancement, no support handoff execution, no checkout changes, no inventory mutation, no schema change, no migrations.


## PHASE 6A — Production Sync Feature Flag Persistence Implementation

Status: Pending local verification.

Adds controlled feature flag persistence implementation evidence for production sync. This phase does not execute production sync, does not enable sync, does not write queue entries, does not toggle runtime flags, does not advance checkpoints, does not mutate inventory, does not change checkout, and does not change schema.

Expected verification: PHASE 6A markers verified; 295 tests passed; Release build successful.

## PHASE 6B - Production Sync Kill Switch Runtime Enforcement Implementation

Status: PENDING LOCAL VERIFICATION.

This phase adds controlled kill switch runtime enforcement readiness while preserving hard stops: no production sync execution, no sync enablement, no queue writes, no runtime flag toggle, no checkpoint advancement, no checkout changes, no inventory mutation, no schema change, and no migrations.


## PHASE 6C — Production Sync Queue Processor Dry-Run Execution Implementation

Status: PENDING LOCAL VERIFICATION.

Adds controlled dry-run queue processor readiness: queue processor dry-run mode, read-only queue scan, no queue claim, no queue writes, no item status transition, no checkpoint advancement, feature flag read requirement, kill switch enforcement requirement, tenant/device dry-run scope, idempotency key inspection, correlation id dry-run evidence and operator approval evidence.

Hard stops: no production sync execution, no sync enablement, no queue claim, no queue writes, no runtime flag toggle, no checkpoint advancement, no checkout changes, no inventory mutation, no schema change, no migrations.

Production Sync Controlled Execution Implementation moves from 20% -> 30% after local verification.

PHASE 6D BLOCKED until PHASE 6C is locally verified.


## PHASE 6D — Production Sync Queue Claim & Lease Implementation

Status: PENDING LOCAL VERIFICATION.

Adds controlled claim/lease readiness: queue claim contract, lease ownership contract, tenant/device queue claim, claim only after feature flag read, claim blocked by kill switch, claim blocked before dry-run readiness, lease expiration, lease renewal boundary, stale lease recovery, idempotency key claim guard, correlation id claim evidence, no payload mutation during claim, claim result audit evidence and rollback-safe lease release.

Hard stops: no production sync execution, no sync enablement, no queue payload writes, no item processing, no server acknowledgement, no runtime flag toggle, no checkpoint advancement, no checkout changes, no inventory mutation, no schema change, no migrations.

Production Sync Controlled Execution Implementation moves from 30% -> 40% after local verification.

PHASE 6E BLOCKED until PHASE 6D is locally verified.


## PHASE 6E - Production Sync Server Acknowledgement Integration Implementation

Status: PENDING LOCAL VERIFICATION

Adds controlled server acknowledgement integration readiness after queue claim/lease and before checkpoint commit.

Required local validation:

```powershell
.\VERIFY_PHASE_6E_UPDATED.ps1
dotnet test
dotnet build -c Release Pos.sln
```

Expected result: 315 tests passed, 0 failed, Release build successful.

PHASE 6F - Production Sync Checkpoint Commit Runtime Implementation remains blocked until PHASE 6E closes.


## PHASE 6F - Production Sync Checkpoint Commit Runtime Implementation

Status: PENDING LOCAL VERIFICATION

PHASE 6F prepares the controlled checkpoint commit runtime implementation after durable server acknowledgement. It documents checkpoint commit contract, durable acknowledgement prerequisite, checkpoint candidate state, checkpoint monotonicity guard, tenant/device scope, queue item matching, lease ownership guard, idempotency guard, correlation evidence, last success boundary, rollback boundary, checkpoint audit evidence and operator approval evidence.

Protected boundaries: no production sync execution, no sync enablement, no real checkpoint commit, no queue payload writes, no item processing, no real server acknowledgement send, no runtime flag toggle, no checkout changes, no inventory mutation, no schema change, no migrations.

Production Sync Controlled Execution moves from 50% -> 60% after local verification.

PHASE 6G - Production Sync Conflict Detection Runtime Implementation remains blocked until PHASE 6F closes.


## PHASE 6G — Production Sync Conflict Detection Runtime Implementation

Status: PENDING LOCAL VERIFICATION.

Adds controlled production sync conflict detection runtime implementation markers: conflict detection contract, local/server version evidence, checkpoint comparison, tenant/device scope, queue item conflict matching, lease ownership guard, idempotency key guard, correlation id evidence, conflict classification and manual resolution handoff.

Hard stops: no production sync execution, no sync enablement, no automatic conflict resolution, no real checkpoint commit, no queue payload writes, no item processing, no inventory mutation, no checkout changes, no schema change and no migrations.


## PHASE 6H - Production Sync Dead-Letter Queue Persistence Implementation

Status: PENDING LOCAL VERIFICATION. Adds controlled production sync dead-letter queue persistence implementation evidence and guardrails. No production sync execution, no sync enablement, no automatic replay, no item processing, no queue payload mutation, no real checkpoint commit, no checkout changes, no inventory mutation, no schema change, no migrations. Production Sync Controlled Execution moves from 70% -> 80% after local verification. PHASE 6I remains blocked until verification passes.


## PHASE 6I — Production Sync Runtime Metrics Emission Implementation

Status: PENDING LOCAL VERIFICATION.

Adds production sync runtime metrics emission implementation guardrails: runtime metrics emission contract, queue depth metric, processing latency metric, acknowledgement latency metric, checkpoint lag metric, retry rate metric, dead-letter rate metric, conflict rate metric, error rate metric, sync throughput metric, tenant/device metric scope, correlation id metric evidence, idempotency key metric evidence, redacted metric tags, alert threshold metric handoff, operator dashboard metric handoff and operator-safe runtime metrics message documented.

Protected boundaries: no production sync execution, no sync enablement, no external telemetry emission, no item processing, no queue payload mutation, no real checkpoint commit, no inventory mutation, no checkout changes, no schema change, no migrations.

Production Sync Controlled Execution Implementation moves from **80% -> 90%** after local verification. PHASE 6J remains blocked until PHASE 6I is verified.


## PHASE 6J — Production Sync Canary Tenant/Device Controlled Enablement

Status: PENDING LOCAL VERIFICATION. Adds canary tenant/device controlled enablement guardrails with no global sync enablement, no production-wide rollout, no automatic tenant/device expansion, no queue payload mutation, no unchecked checkpoint commit, no conflict auto-resolution, no dead-letter replay, no checkout changes, no inventory mutation, no schema change and no migrations. PHASE 6 Controlled Execution moves from **90% -> 100%** after local verification.


## PHASE 7A — Security Dependency Hardening

- Started Security & Dependency Hardening.
- Pinned `System.Text.Json` in `PosBuilder` from `8.0.0` to `8.0.5`.
- Added `PosSecurityDependencyHardening` guardrails and verification script.
- Expected local gate: `PHASE 7A markers verified.`, `345 tests passed`, `0 failed`, `Compilación correcta.`


## PHASE 7B — Nullability Warning Hardening Baseline

- Added `PosNullabilityWarningHardeningBaseline` guardrails and verification script.
- Documented nullable warning classes: CS8602, CS8601, CS8618, CS8622, CS8600 and CS8603.
- Documented server service, CentralDbContext, SyncService and PosBuilder nullability hotspots.
- Security & Dependency Hardening: 10% -> 20%.
- Expected local gate: `PHASE 7B markers verified.`, `350 tests passed`, `0 failed`, `Compilación correcta.`
- Protected boundaries: no checkout behavior change, no inventory mutation, no production sync enablement, no schema change, no migrations.


## PHASE 7C — Targeted Nullability Remediation: Server Services

- Added `PosTargetedNullabilityServerServicesRemediation` guardrails and verification script.
- Applied targeted nullability remediation in `AuthService`, `UserService` and `CentralDbContext`.
- Added guarded password hash verification, guarded token claims, guarded provision payloads, guarded username comparisons and safe audit/outbox string conversions.
- Security & Dependency Hardening: 20% -> 30%.
- Expected local gate: `PHASE 7C markers verified.`, `355 tests passed`, `0 failed`, `Compilación correcta.`
- Protected boundaries: no checkout behavior change, no inventory mutation, no production sync enablement, no schema change, no migrations.


## PHASE 7D — Duplicate Using Cleanup & Analyzer Hygiene

- Added `PosDuplicateUsingCleanupAnalyzerHygiene` guardrails and verification script.
- Removed exact duplicate using directives from local repositories, server controllers and selected PosCore services.
- Analyzer target: CS0105 duplicate using directive.
- Security & Dependency Hardening: 30% -> 40%.
- Expected local gate: `PHASE 7D markers verified.`, `360 tests passed`, `0 failed`, `Compilación correcta.`
- Protected boundaries: no checkout behavior change, no inventory mutation, no production sync enablement, no public API behavior change, no schema change, no migrations.


## PHASE 7E — ASP.NET Header Analyzer Hygiene

PHASE 7E applies ASP.NET header analyzer hygiene for CorrelationIdMiddleware by replacing HeaderDictionary.Add with safe indexer assignment. It preserves correlation id behavior and keeps no checkout behavior change, no inventory mutation, no production sync enablement, no schema change and no migrations.


## PHASE 7F — PosBuilder Nullability Hygiene

Status: Pending local verification. PosBuilder nullability hygiene for UI/bootstrap initialization, event compatibility and safe conversion boundaries. Expected: 370 tests passed, 0 failed, Release build successful.


## PHASE 7G — SyncService Nullability Hygiene

Status: Pending local verification. SyncService nullability hygiene for nullable username normalization during pull updates. Expected: 375 tests passed, 0 failed, Release build successful.


## PHASE 7H — AuthService Remaining Nullability Hygiene

Status: Pending local verification. AuthService remaining nullability hygiene for the final login `CS8602` warning. Expected: 380 tests passed, 0 failed, Release build successful.

Protected boundaries: no checkout behavior change, no inventory mutation, no production sync enablement, no public API behavior change, no schema change and no migrations.


## PHASE 7I — ClientOrderService Async Hygiene

- Status: PENDING LOCAL VERIFICATION.
- Security & Dependency Hardening: 80% -> 90% after local verification.
- Scope: targeted CS1998 remediation in ClientOrderService CreateDraftOrderAsync.
- Keeps Task-based contract with Task.FromResult result boundary.
- Guardrails: no checkout behavior change, no inventory mutation, no production sync enablement, no public API behavior change, no schema change and no migrations.
- Expected local gate: `PHASE 7I markers verified.`, `385 tests passed`, `0 failed`, `Compilación correcta.`
- PHASE 7J remains BLOCKED until PHASE 7I is closed.

## PHASE 7J — Security Hardening Closure & Zero-Warning Evidence

- Status: PENDING LOCAL VERIFICATION.
- Security & Dependency Hardening: 90% -> 100% after local verification.
- Scope: closure evidence for zero-warning and zero-error Release build after PHASE 7I.
- Source evidence: 385 tests passed, 0 failed, Compilación correcta, 0 Advertencia(s), 0 Errores.
- Expected local gate: `PHASE 7J markers verified.`, `390 tests passed`, `0 failed`, `Compilación correcta.`, `0 Advertencia(s)`, `0 Errores`.
- Guardrails: no checkout behavior change, no inventory mutation, no production sync enablement, no public API behavior change, no schema change and no migrations.
- PHASE 8 remains BLOCKED until PHASE 7J is closed.


## PHASE 8A - Production Readiness Operational Baseline

Status: PENDING LOCAL VERIFICATION.

PHASE 8A starts Release Packaging and Operational Readiness after PHASE 7 closed with 390 tests passed, Compilacion correcta, 0 Advertencia(s), and 0 Errores.

Expected after verification: 395 tests passed, 0 failed, Compilacion correcta, 0 Advertencia(s), 0 Errores.

Scope: operational baseline only. No checkout behavior change. No inventory mutation. No production sync enablement. No packaging execution. No deployment execution. No public API behavior change. No schema change. No migrations.


## PHASE 8B - Release Artifact Inventory and Packaging Baseline

Status: PENDING LOCAL VERIFICATION.

PHASE 8B documents the release artifact inventory and packaging baseline after PHASE 8A closed with 395 tests passed, Compilacion correcta, 0 Advertencia(s), and 0 Errores.

Expected result after local verification: 400 tests passed, 0 failed, Compilacion correcta, 0 Advertencia(s), 0 Errores.

Scope remains protected: no checkout behavior change, no inventory mutation, no production sync enablement, no packaging execution, no installer execution, no deployment execution, no public API behavior change, no schema change, no migrations.


## PHASE 8C - Versioning and Release Manifest Baseline

Status: PENDING LOCAL VERIFICATION.

PHASE 8C documents the versioning and release manifest baseline after PHASE 8B closed with 400 tests passed, Compilacion correcta, 0 Advertencia(s), and 0 Errores.

Expected result after local verification: 405 tests passed, 0 failed, Compilacion correcta, 0 Advertencia(s), 0 Errores.

Scope remains protected: no checkout behavior change, no inventory mutation, no production sync enablement, no packaging execution, no installer execution, no deployment execution, no public API behavior change, no schema change, no migrations.


## PHASE 8D - Checksum and Artifact Verification Baseline

Status: PENDING LOCAL VERIFICATION.

PHASE 8D documents checksum and artifact verification baseline evidence after PHASE 8C closed with 405 tests passed, Compilacion correcta, 0 Advertencia(s), and 0 Errores.

Expected result after local verification: 410 tests passed, 0 failed, Compilacion correcta, 0 Advertencia(s), 0 Errores.

Scope remains protected: no checkout behavior change, no inventory mutation, no production sync enablement, no packaging execution, no installer execution, no deployment execution, no public API behavior change, no schema change, no migrations.


## PHASE 8E - Installer Readiness and Setup Packaging Baseline

Status: PENDING LOCAL VERIFICATION.

PHASE 8E documents installer readiness and setup packaging baseline evidence after PHASE 8D closed with 410 tests passed, Compilacion correcta, 0 Advertencia(s), and 0 Errores.

Expected verification result: 415 tests passed, 0 failed, Compilacion correcta, 0 Advertencia(s), 0 Errores.

Safety: no checkout behavior change, no inventory mutation, no production sync enablement, no packaging execution, no installer execution, no deployment execution, no public API behavior change, no schema change, no migrations.

## PHASE 8F - Release Notes and Operator Handoff Baseline

Status: PENDING LOCAL VERIFICATION.

Release Packaging and Operational Readiness: 50% -> 60%.

PHASE 8F documents release notes and operator handoff baseline evidence after PHASE 8E closed with 415 tests passed, Compilacion correcta, 0 Advertencia(s), and 0 Errores.

Expected verification result: 420 tests passed, 0 failed, Compilacion correcta, 0 Advertencia(s), 0 Errores.

Safety: no checkout behavior change, no inventory mutation, no production sync enablement, no packaging execution, no installer execution, no deployment execution, no public API behavior change, no schema change, no migrations.


## PHASE 8G - Smoke Test and Release Candidate Validation Baseline

Status: PENDING LOCAL VERIFICATION.

Release Packaging and Operational Readiness: 60% -> 70%.

PHASE 8G documents smoke test and release candidate validation baseline evidence after PHASE 8F closed with 420 tests passed, Compilacion correcta, 0 Advertencia(s), and 0 Errores.

Expected verification result: 425 tests passed, 0 failed, Compilacion correcta, 0 Advertencia(s), 0 Errores.

Safety: no checkout behavior change, no inventory mutation, no production sync enablement, no packaging execution, no installer execution, no deployment execution, no public API behavior change, no schema change, no migrations.

## PHASE 8H - Rollback Drill and Recovery Evidence Baseline

PHASE 8H documents rollback drill and recovery evidence for Release Packaging and Operational Readiness. Evidence target: 430 tests passed, Compilación correcta, 0 Advertencia(s), 0 Errores.

Boundaries: no checkout behavior change, no inventory mutation, no production sync enablement, no packaging execution, no installer execution, no deployment execution, no public API behavior change, no schema change, no migrations.


## PHASE 8I - Monitoring and Post-Release Support Evidence Baseline

PHASE 8I documents monitoring and post-release support evidence for Release Packaging and Operational Readiness. Evidence target: 435 tests passed, Compilación correcta, 0 Advertencia(s), 0 Errores.

Boundaries: no checkout behavior change, no inventory mutation, no production sync enablement, no packaging execution, no installer execution, no deployment execution, no public API behavior change, no schema change, no migrations.


## PHASE 8J - Release Go No-Go and Operational Readiness Closure

Release Packaging and Operational Readiness: 90% -> 100%

PHASE 8J documents final release go/no-go and operational readiness closure evidence. Expected verification: 440 tests passed, 0 failed, Compilacion correcta, 0 Advertencia(s), 0 Errores.

Safety: no checkout behavior change, no inventory mutation, no production sync enablement, no packaging execution, no installer execution, no deployment execution, no public API behavior change, no schema change, no migrations.


## PHASE 9A - Installer Generation and Release Artifact Execution

Status: PENDING LOCAL VERIFICATION.

PHASE 9A starts Release Execution with controlled installer generation and release artifact execution. Expected evidence: 445 tests passed, 0 failed, Compilación correcta, 0 Advertencia(s), 0 Errores.

Release script: `scripts/release/Generate-Phase9ReleaseArtifacts.ps1`.

Safety boundaries: no checkout behavior change, no inventory mutation, no production sync enablement, no deployment execution, no public API behavior change, no schema change, no migrations.


## PHASE 9B — Installer Package Generation Execution

Status: PENDING LOCAL VERIFICATION. Release Execution: 10% -> 20%. Expected 450 tests passed. Adds installer package generation execution, package manifest generation, installer package checksums, package zip archive generation, and operator package generation command. Safety: no checkout behavior change; no inventory mutation; no production sync enablement; no deployment execution; no public API behavior change; no schema change; no migrations.


### PHASE 9B hotfix - installer package prerequisite regeneration
`Generate-Phase9InstallerPackage.ps1` now regenerates missing PHASE 9A release artifacts before packaging, while preserving no-deployment and no-schema-change boundaries.


## PHASE 9C - Installer Package Verification and Integrity Execution

Status: PENDING LOCAL VERIFICATION. Release Execution: 20% -> 30%. Expected 455 tests passed. Adds installer package verification integrity execution, archive SHA-256 verification, manifest cross-check, unzip verification, required content verification, and operator package verification command. Safety: no checkout behavior change; no inventory mutation; no production sync enablement; no deployment execution; no public API behavior change; no schema change; no migrations.

## PHASE 9D — Installer Smoke Install Simulation and Package Extraction Validation

PHASE 9D adds installer smoke install simulation package extraction validation documented. It verifies simulated install directory creation, installer package extraction to simulated install directory, required PosCore/PosBuilder/PosServer content, release manifest content, checksums content, file count evidence, executable candidate discovery, and smoke install evidence manifest generation.

Operator command:

```powershell
.\scripts\release\Simulate-Phase9InstallerSmokeInstall.ps1 -ReleaseVersion 0.9.0-rc.1 -ReleaseChannel release-candidate
```

Safety boundary: no real installer execution, no deployment execution, no checkout behavior change, no inventory mutation, no production sync enablement, no public API behavior change, no schema change, and no migrations.


## PHASE 9E - Installer Launch Script and Desktop Shortcut Packaging

Status: PENDING LOCAL VERIFICATION. Release Execution advances from 40% to 50%. Adds Generate-Phase9LaunchAndShortcutPackage.ps1 to package Start-PosCore.ps1, Start-PosBuilder.ps1, Start-PosServer.ps1, desktop-shortcut-spec.json, Create-DesktopShortcuts.ps1, launcher-package-manifest.json, launcher-checksums.sha256, and a launch package archive. No real shortcut creation, no real installer execution, no deployment execution, no schema change, and no migrations.


## PHASE 9F - Installer Uninstall and Cleanup Simulation Validation

PHASE 9F adds dry-run installer uninstall cleanup simulation validation. It generates `uninstall-cleanup-plan.json` and `uninstall-cleanup-evidence.json` without deleting real files, shortcuts, Program Files content, Desktop content, registry keys, release manifests, checksums, or audit evidence.

Command:

```powershell
.\scripts\release\Simulate-Phase9InstallerUninstallCleanup.ps1 -ReleaseVersion 0.9.0-rc.1 -ReleaseChannel release-candidate
```


## PHASE 9G - Installer Upgrade Simulation and Version Preservation Validation

Status: PENDING LOCAL VERIFICATION. PHASE 9G adds installer upgrade simulation version preservation validation documented with upgrade-simulation-plan.json and upgrade-preservation-evidence.json. It depends on PHASE 9F uninstall cleanup simulation prerequisite documented and targets 475 tests expected after installer upgrade simulation version preservation validation documented. Guardrails: no real upgrade execution, no file overwrite, no database writes, no Windows registry mutation, no Desktop mutation, no Program Files mutation, no real installer execution, no deployment execution, no checkout behavior change, no inventory mutation, no production sync enablement, no public API behavior change, no schema change, no migrations.


## PHASE 9H - Installer Rollback Simulation and Previous Version Recovery Validation

Status: PENDING LOCAL VERIFICATION. PHASE 9H adds installer rollback simulation previous version recovery validation documented with rollback-simulation-plan.json and previous-version-recovery-evidence.json. It depends on PHASE 9G upgrade simulation prerequisite documented and targets 480 tests expected after installer rollback simulation previous version recovery validation documented. Guardrails: no real rollback execution, no file overwrite, no database writes, no Windows registry mutation, no Desktop mutation, no Program Files mutation, no real installer execution, no deployment execution, no checkout behavior change, no inventory mutation, no production sync enablement, no public API behavior change, no schema change, no migrations.


## PHASE 9I - Installer Release Candidate Final Evidence and Operator Acceptance Validation

Status: PENDING LOCAL VERIFICATION. Release Execution advances from 80% to 90%. PHASE 9I adds installer release candidate final evidence operator acceptance validation documented with release-candidate-final-evidence.json and operator-acceptance-checklist.json. It depends on PHASE 9H rollback simulation prerequisite documented and targets 485 tests expected after installer release candidate final evidence operator acceptance validation documented. Guardrails: no real release execution, no real installer execution, no real rollback execution, no file overwrite, no database writes, no Windows registry mutation, no Desktop mutation, no Program Files mutation, no deployment execution, no checkout behavior change, no inventory mutation, no production sync enablement, no public API behavior change, no schema change, no migrations.


## PHASE 9J - Installer Release Execution Closure and Production Handoff Validation

Adds dry-run release execution closure and production handoff validation with release-execution-closure-evidence.json and production-handoff-package.json. No real release execution, installer execution, rollback execution, deployment, checkout, inventory, production sync, public API, schema, or migration changes.


## PHASE 10.1 - Production Environment Readiness

Status: PENDING LOCAL VERIFICATION.

PHASE 10.1 groups PHASE 10A, PHASE 10B, and PHASE 10C.
It documents production environment configuration validation, secrets and runtime configuration hardening, and database production migration dry run validation.

Evidence outputs:

- production-environment-readiness-evidence.json
- production-runtime-configuration-report.json
- database-migration-dry-run-report.json

Guardrails: no real deployment execution, no Railway mutation, no Supabase mutation, no production database migration execution, no live secret disclosure, no checkout behavior change, no inventory mutation, no production sync enablement, no public API behavior change, no schema change, no migrations.


## PHASE 10.2 — Backup, Restore and Deployment Simulation

Status: PENDING LOCAL VERIFICATION.

PHASE 10.2 backup restore and deployment simulation documented. Groups PHASE 10D backup and restore drill validation documented and PHASE 10E production deployment pipeline simulation documented.

Expected validation: 515 tests passed, 0 failed, 0 Advertencia(s), 0 Errores. Script: scripts/release/Validate-Phase10BackupRestoreDeploymentSimulation.ps1. Guardrails: no real deployment execution, no Railway mutation, no Supabase mutation, no production database mutation, no backup deletion, no restore execution against production, no release promotion, no schema change, no migrations.


## PHASE 10.3 - Staging Execution and Smoke Tests

PHASE 10.3 staging execution and smoke tests documented. PHASE 10F staging deployment execution validation documented. PHASE 10G production smoke test checklist documented. Expected tests: 525 passed. Guardrails: no real production deployment, no production traffic routing, no Railway mutation, no Supabase mutation, no production database mutation, no real payment capture, no real inventory mutation, no release promotion, no schema change, no migrations.


## PHASE 10.4 - Monitoring, Rollback and Go/No-Go

PHASE 10.4 monitoring rollback and go no-go documented. Groups PHASE 10H monitoring and alerting activation validation, PHASE 10I production rollback procedure validation, and PHASE 10J production release go no-go final closure. Expected test baseline advances from 525 tests passed to 540 tests passed while preserving no production deployment, no production traffic routing, no Railway mutation, no Supabase mutation, no production database mutation, no release promotion, no schema change, and no migrations.


## PHASE 11.1 - Cashier Shift and Sales Flow Validation

PHASE 11.1 starts POS Functional Business Validation after PHASE 10.4 production readiness closure. It documents cashier shift opening, initial cash drawer balance, basic sale calculation, controlled discount application, payment registration checklist, shift close workflow, cash reconciliation checklist, and functional evidence handoff. Guardrails: no real checkout execution, no real payment capture, no receipt printing, no inventory mutation, no hardware access, no production sync enablement, no public API behavior change, no schema change, no migrations. Expected validation baseline: 556 tests passed.


## PHASE 11.2 — Payments, Receipts and Returns Validation

PHASE 11.2 payments receipts and returns validation documented. Payments, Receipts and Returns Validation moves functional business validation to 50%. Expected validation target: 572 tests passed. Guardrails preserved: no real payment capture, no live payment gateway call, no receipt printing, no refund execution, no inventory mutation, no real checkout execution, no hardware access, no production sync enablement, no public API behavior change, no schema change, no migrations.

## PHASE 11.3 — Inventory, Stock Movement and Offline Sync Validation

PHASE 11.3 adds controlled functional business validation for inventory availability, stock movement auditability, and offline sync readiness. Expected result: 588 tests passed, 0 failed, with no real inventory mutation, no stock write execution, no production sync enablement, no live server commit, no checkout behavior change, no public API behavior change, no schema change, and no migrations.

## PHASE 11.4 - Hardware Readiness and Store Pilot Checklist

PHASE 11.4 closes POS Functional Business Validation with hardware readiness and store pilot checklist evidence.

Scope: Hardware Readiness and Store Pilot Checklist.

Expected validation: 604 tests passed, 0 failed, Release build clean, AcceptedChecks: 15, BlockingIssues: 0.

Guardrails: no real hardware access, no printer execution, no cash drawer pulse, no scanner capture, no payment terminal execution, no store pilot activation, no production traffic routing, no real inventory mutation, no production sync enablement, no public API behavior change, no schema change, no migrations.


## PHASE 11 FINAL — POS Functional Business Validation Closure

PHASE 11 FINAL closes POS Functional Business Validation after PHASE 11.1, 11.2, 11.3, and 11.4. Expected regression state: 620 tests passed, 0 failed. Evidence: functional-business-closure-evidence.json, functional-business-readiness-scorecard.json, store-pilot-entry-decision-report.json, phase11-final-closure-summary.json. Guardrails: no checkout real, no payment capture, no receipt printing, no refund execution, no real inventory mutation, no hardware access, no store pilot activation, no production sync enablement, no public API behavior change, no schema change, no migrations.

## Railway Dockerfile Hotfix

A root-level `Dockerfile` and `.dockerignore` were added so Railway can deploy `PosServer` without failing on Dockerfile discovery.

Expected Railway configuration:

```text
Root Directory: /
Dockerfile Path: Dockerfile
```

Local validation:

```powershell
.\VERIFY_RAILWAY_DOCKERFILE_UPDATED.ps1
docker build -t posserver-railway-test .
docker run --rm -p 8080:8080 -e PORT=8080 posserver-railway-test
```

Guardrails: no business logic change, no checkout behavior change, no inventory mutation change, no public API contract change, no schema change, no migrations, no Railway variable mutation, no Supabase data mutation.
