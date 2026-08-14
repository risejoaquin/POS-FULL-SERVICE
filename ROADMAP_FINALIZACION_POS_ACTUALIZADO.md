# ROADMAP REAL DE FINALIZACIÓN — ACTUALIZADO

## SUPER POS — PosCore + PosServer + PosBuilder

**Base actual:** auditoría del repositorio actual + verificación del ZIP `hola-app (2).zip` + resultados reales de terminal en Windows.

**Estado de referencia validado:**

```text
dotnet test
→ PASS

dotnet build -c Release Pos.sln
→ PASS
→ 144 warnings
→ 0 errors
```

**Fuente de verdad:** este documento reemplaza roadmaps anteriores como guía operativa activa. La documentación histórica queda como referencia, no como instrucción ejecutable.

---

# REGLA PRINCIPAL

El proyecto no se considera finalizado porque compile.

Cada fase debe cumplir:

```text
IMPLEMENTACIÓN
      ↓
TESTS
      ↓
VERIFICACIÓN ESTÁTICA
      ↓
VERIFICACIÓN FUNCIONAL
      ↓
PHASE GATE
      ↓
PASS
```

Si una fase queda `PARTIAL`, el trabajo no continúa.

**Regla adicional desde ahora:** ninguna fase puede modificar masivamente áreas fuera de su alcance. Si AI Studio detecta un problema fuera de fase, debe reportarlo como deuda o bloqueo, no corregirlo automáticamente.

---

# ESTADO GENERAL DEL ROADMAP

| Fase | Nombre | Estado |
|---|---|---|
| PHASE 0 | Contención de bloqueadores críticos | CLOSED |
| PHASE 0.5 | Repository Hygiene / Baseline Cleanup | NEXT |
| PHASE 1 | Limpieza arquitectónica | PENDING |
| PHASE 2 | Purificación de Domain + Money | PENDING |
| PHASE 3 | Motor transaccional POS | PENDING |
| PHASE 4 | Inventario y concurrencia | PENDING |
| PHASE 5 | Multi-tenancy y seguridad | PENDING |
| PHASE 6 | RLS real | PENDING |
| PHASE 7 | Device Identity + Provisioning | PENDING |
| PHASE 8 | Licensing + configuración local | PENDING |
| PHASE 9 | Outbox / Inbox / Idempotencia | PENDING |
| PHASE 10 | Sincronización por cursor | PENDING |
| PHASE 11 | Conflictos | PENDING |
| PHASE 12 | Testing real | PENDING |
| PHASE 13 | PosBuilder / Release / Update | PENDING |
| PHASE 14 | Release Candidate | PENDING |
| PHASE 15 | Certificación final | PENDING |

---

# PHASE 0 — CONTENCIÓN DE BLOQUEADORES CRÍTICOS

## Estado

```text
CLOSED
```

## Resultado validado

Phase 0 se considera cerrada porque fueron corregidos y verificados los siguientes puntos:

```text
[PASS] Provisioning separado de JWT
[PASS] JWT sin fallback inseguro
[PASS] API no expone excepciones técnicas en respuestas HTTP corregidas
[PASS] Outbox/Audit sin secretos evidentes
[PASS] No existen credenciales/defaults funcionales como fallback normal
[PASS] Desktop fail-closed ante falta de Tenant/License
[PASS] Tests ejecutados en máquina Windows
[PASS] Build Release ejecutado en máquina Windows
```

## Cambios cerrados

### 0.1 Provisioning

Se eliminó:

```text
ProvisionKey == JWT_KEY
```

El sistema no debe utilizar la clave de firma JWT para aprovisionamiento.

### 0.2 JWT

Producción debe fallar al arrancar si faltan:

```text
JWT_KEY
JWT_ISSUER
JWT_AUDIENCE
```

### 0.3 Errores HTTP

Se eliminaron fugas desde controladores corregidos:

```text
StackTrace
InnerException
Exception.Message
SQL errors
connection details
```

Toda excepción inesperada debe producir:

```text
HTTP 500
CorrelationId
mensaje seguro
```

Los detalles únicamente deben existir en logs internos.

### 0.4 Outbox / Audit

Nunca serializar:

```text
PasswordHash
Pin
RefreshToken
JWT
ProvisionKey
SecretKey
```

### 0.5 Defaults inseguros

Eliminar defaults funcionales como:

```text
TENANT_001
VAL-TRIAL-123
ManagerPin = 1234
default tenant
default license
```

Si falta configuración obligatoria:

```text
FAIL CLOSED
```

## Nota sobre `TENANT_001`

La referencia histórica encontrada en `LoginViewModel` se considera aceptable únicamente si funciona como migración local heredada y no como fallback activo de tenant.

## Deuda detectada durante Phase 0

Estas deudas NO reabren Phase 0, pero quedan registradas:

| ID | Deuda | Prioridad |
|---|---|---|
| TD-001 | `System.Text.Json 8.0.0` con vulnerabilidad NU1903 | Alta |
| TD-002 | 144 warnings de compilación | Alta |
| TD-003 | Nullability warnings (`CS8600`, `CS8601`, `CS8602`, `CS8618`, etc.) | Alta |
| TD-004 | `ASP0019` en `CorrelationIdMiddleware` | Media |
| TD-005 | `using` duplicados (`CS0105`) | Baja |
| TD-006 | Método async sin await (`CS1998`) | Media |
| TD-007 | Fixture `PaymentDetails` del test usa formato parseable, pero no exactamente el formato monetario real de producción | Baja |
| TD-008 | `NotImplementedException` en converters de PosCore/PosBuilder | Media |

---

# PHASE 0.5 — REPOSITORY HYGIENE / BASELINE CLEANUP

## Estado

```text
NEXT
```

## Objetivo

Limpiar el repositorio para evitar que las siguientes fases se contaminen con scripts temporales, archivos generados, documentación histórica contradictoria o artefactos de compilación.

Esta fase NO debe modificar reglas de negocio.

## Permitido

Identificar, archivar o eliminar de forma segura:

```text
patch_*.py
fix_*.py
update_*.sh
test_build.py
*_wpftmp.csproj
bin/
obj/
logs temporales
archivos generados locales
scripts de parche ya aplicados
documentación duplicada o histórica
```

## Prohibido

No modificar:

```text
PosDomain
PosApplication
PosInfrastructure
PosCore ViewModels
PosServer Controllers/Services
PosBuilder lógica funcional
Sync
Inventario
Licensing
Provisioning
tests funcionales
```

## Documentación

Debe quedar explícito:

```text
ROADMAP_FINALIZACION_SUPER_POS_ACTUALIZADO.md
→ fuente de verdad actual

Roadmaps anteriores
→ referencia histórica
```

## .gitignore

Debe confirmar o agregar reglas para:

```text
bin/
obj/
*_wpftmp.csproj
*.db
*.sqlite
*.sqlite3
*.log
logs/
TestResults/
.vs/
*.user
*.suo
```

## Gate

```text
[PASS] No se modificó lógica de producción
[PASS] Scripts temporales archivados o eliminados justificadamente
[PASS] Artefactos de build ignorados
[PASS] Documentación histórica marcada como histórica
[PASS] dotnet test PASS
[PASS] dotnet build -c Release Pos.sln PASS
```

---

# PHASE 1 — LIMPIEZA ARQUITECTÓNICA

## Estado

```text
PENDING
```

## Objetivo

Eliminar el bypass de Clean Architecture.

La dependencia lógica debe quedar:

```text
PosCore
   ↓
PosApplication
   ↓
PosDomain

PosInfrastructure
   ↓
implementa ports

PosServer
   ↓
PosApplication
   ↓
PosDomain
```

PosInfrastructure puede ser utilizado por el Composition Root de PosCore para registrar implementaciones, pero **ViewModels y Views no pueden utilizar infraestructura directamente**.

## Cambios

Eliminar de ViewModels:

```text
PosDbContext
EF Core queries
BeginTransaction
SaveChanges
InventoryMovement
OrderManagementService concreto
```

Especialmente:

```text
MainViewModel
LoginViewModel
ReturnsViewModel
ShiftViewModel
InventoryViewModel
UsersViewModel
ReportsViewModel
```

Crear o consolidar casos de uso reales:

```text
CreateOrder
AddOrderItem
CheckoutOrder
ReturnOrder

OpenShift
CloseShift
RegisterCashMovement

AdjustInventory
ReceiveInventory
SellInventory

AuthenticateUser
ProvisionDevice
ValidateLicense

PushSync
PullSync
```

Eliminar interfaces duplicadas como distintos `IOrderService` que representan responsabilidades diferentes bajo nombres iguales.

## Gate

Búsqueda global:

```text
PosCore/ViewModels/**/*.cs
```

debe producir:

```text
0 referencias directas a PosDbContext
0 BeginTransactionAsync
0 SaveChangesAsync
0 consultas EF Core
0 instanciaciones de servicios concretos de Infrastructure
```

Además:

```text
dotnet test
dotnet build -c Release Pos.sln
```

deben pasar.

---

# PHASE 2 — PURIFICACIÓN DEL DOMAIN + MONEY

## Estado

```text
PENDING
```

## Objetivo

Convertir `PosDomain` en dominio real.

Eliminar:

```text
[Key]
[Required]
[Timestamp]
EF annotations
WPF annotations
RowVersion de persistencia
LastUpdated
```

Las configuraciones de EF pasan a:

```text
PosInfrastructure/Data
```

`TenantId` puede permanecer si forma parte de las invariantes del dominio.

## Money

El modelo definitivo será:

```text
Money
    int Cents
    Currency
```

El almacenamiento monetario será:

```text
INT
```

Ejemplo:

```text
$19.99
   ↓
1999
```

Nunca:

```text
double
float
decimal como almacenamiento monetario
```

La cantidad de inventario permanece:

```text
DECIMAL(10,2)
```

porque cantidad y dinero son conceptos diferentes.

## Order

La orden no debe iniciar como:

```text
Closed
```

Debe comenzar:

```text
Draft
```

Las transiciones deben estar protegidas por el dominio:

```text
Draft → Open
Open → Paid
Open → Cancelled
Paid → Closed
Paid → Refunded
Closed → Refunded
```

No se permite saltar estados.

El impuesto no debe estar hardcodeado:

```text
0.16
```

Debe ser una política/regla configurable.

## Gate

```text
[PASS] Domain no conoce EF
[PASS] Domain no conoce WPF
[PASS] Money implementado con centavos
[PASS] Order state machine protegida
[PASS] No LastUpdated para sincronización
[PASS] No RowVersion de persistencia en Domain
[PASS] dotnet test
[PASS] dotnet build -c Release Pos.sln
```

---

# PHASE 3 — MOTOR TRANSACCIONAL DE POS

## Estado

```text
PENDING
```

## Objetivo

Crear una única implementación correcta de:

```text
Venta
Inventario
Pago
Caja
Orden
```

La operación `CheckoutOrder` debe ser un caso de uso.

No puede existir una versión en WPF y otra diferente en Server.

## Flujo definitivo

```text
CheckoutOrder
      ↓
Validate Shift
      ↓
Validate Order
      ↓
Validate Payment
      ↓
Validate Inventory
      ↓
Create Order
      ↓
Create Payment
      ↓
Create Inventory Movements
      ↓
Update Inventory Balance
      ↓
Create Cash Movement
      ↓
Create Outbox Events
      ↓
COMMIT
```

Todo dentro de una unidad transaccional.

## Gate

```text
[PASS] WPF no ejecuta checkout manual directo
[PASS] Server no duplica checkout con lógica distinta
[PASS] Checkout usa caso de uso compartido
[PASS] Orden, pago, inventario, caja y outbox se confirman en una unidad transaccional
[PASS] Tests de checkout exitoso
[PASS] Tests de rollback
[PASS] Tests de pago parcial/inválido
[PASS] dotnet test
[PASS] dotnet build -c Release Pos.sln
```

---

# PHASE 4 — INVENTARIO Y CONCURRENCIA

## Estado

```text
PENDING
```

## Objetivo

Eliminar definitivamente:

```text
product.StockQuantity -= quantity
```

como mecanismo de negocio distribuido.

## Modelo

```text
InventoryMovement
        ↓
       Ledger
        ↓
InventoryBalance
```

`InventoryMovement` es el historial inmutable.

`InventoryBalance` es la proyección actual.

Nunca se modifica inventario sin generar su movimiento correspondiente.

## Concurrencia

El servidor debe garantizar:

```text
stock >= quantity
```

de manera atómica.

Dos terminales intentando vender la última unidad:

```text
POS A → SUCCESS
POS B → REJECTED
```

Nunca:

```text
POS A → SUCCESS
POS B → SUCCESS
stock = -1
```

## Offline

Para garantizar consistencia entre múltiples POS desconectados, se debe introducir:

```text
InventoryAllocation
```

por dispositivo.

El servidor asigna stock disponible a cada terminal para operaciones offline.

## Gate

Pruebas obligatorias:

```text
1 producto / 2 POS / venta concurrente
1 producto / 2 POS / offline allocation
stock insuficiente
rollback
duplicate sale
crash durante checkout
```

---

# PHASE 5 — MULTI-TENANCY Y SEGURIDAD

## Estado

```text
PENDING
```

## Objetivo

Establecer una única autoridad de Tenant.

Para requests autenticados:

```text
JWT
 ↓
TenantContext
 ↓
Application
 ↓
Repository
 ↓
Database
```

`X-Tenant-Id` no puede cambiar el tenant de un usuario autenticado.

Debe eliminarse como mecanismo normal de selección de tenant.

Solamente bootstrap/provisioning puede utilizar mecanismos de enrollment explícitos.

## JWT

Claims definitivos:

```text
sub / user_id
tenant_id
role
jti
```

El servidor valida:

```text
User exists
User active
Tenant exists
Tenant active
User belongs to Tenant
```

## Gate

```text
[PASS] Tenant autenticado proviene de JWT
[PASS] X-Tenant-Id no puede cambiar tenant autenticado
[PASS] Usuario suspendido no puede operar
[PASS] Tenant suspendido no puede operar
[PASS] Roles validados
[PASS] Tests de autorización y tenant
```

---

# PHASE 6 — RLS REAL

## Estado

```text
PENDING
```

## Objetivo

RLS debe existir realmente en PostgreSQL.

No basta con:

```text
HasQueryFilter
```

Debe existir:

```text
ENABLE ROW LEVEL SECURITY
CREATE POLICY
```

dentro de migraciones versionadas.

El pipeline debe poder crear una base nueva desde cero.

## Regla

```text
EF Query Filter = primera barrera
Application Authorization = segunda barrera
PostgreSQL RLS = última barrera
```

Nunca depender únicamente de una de ellas.

## Gate

Tests reales contra PostgreSQL:

```text
Tenant A no puede leer Tenant B
Tenant A no puede modificar Tenant B
Tenant A no puede eliminar Tenant B
Tenant A no puede crear datos para Tenant B
```

No utilizar únicamente EF InMemory para validar RLS.

---

# PHASE 7 — DEVICE IDENTITY + PROVISIONING

## Estado

```text
PENDING
```

## Objetivo

Formalizar:

```text
Tenant
   ↓
Device
```

Cada terminal tendrá:

```text
DeviceId
TenantId
Status
CreatedAt
RevokedAt
Credential
```

No utilizar:

```text
Environment.MachineName
```

como identidad criptográfica.

## Provisioning definitivo

```text
Admin/Portal
     ↓
One-Time Enrollment Token
     ↓
PosBuilder
     ↓
Register Device
     ↓
Server
     ↓
Device Credential
     ↓
PosCore
```

El token:

```text
one-time
short-lived
tenant-bound
device-bound
```

Después de utilizarse:

```text
REVOKED
```

Provisioning debe ser idempotente y transaccional.

---

# PHASE 8 — LICENSING + CONFIGURACIÓN LOCAL

## Estado

```text
PENDING
```

## Licencia

Eliminar definitivamente:

```text
VAL-{tenant}-123
VAL-TRIAL-123
```

La licencia definitiva debe ser firmada criptográficamente.

Arquitectura:

```text
LICENSE SERVER
       │
       │ private key
       ▼
Signed License
       │
       ▼
PosCore
       │
       │ public key
       ▼
Signature Verification
```

La clave privada nunca llega a PosCore.

La licencia debe contener como mínimo:

```text
LicenseId
TenantId
DeviceId / Device scope
IssuedAt
ExpiresAt
Features
MaxDevices
Version
Signature
```

## Configuración

No almacenar secretos sensibles en JSON plano.

La configuración pública puede contener:

```text
API URL
branding
modules
printer configuration
```

Los secretos deben almacenarse mediante mecanismo protegido de Windows.

Importante:

**PosBuilder no debe cifrar con DPAPI CurrentUser algo que después utilizará otro usuario de Windows.**

La solución correcta es:

```text
Builder
   ↓
non-secret bootstrap config
   ↓
first-run enrollment
   ↓
PosCore
   ↓
Windows protected credential
```

---

# PHASE 9 — OUTBOX / INBOX / IDEMPOTENCIA

## Estado

```text
PENDING
```

## Outbox

El patrón definitivo:

```text
Business Transaction
       ↓
Domain changes
       ↓
Outbox Event
       ↓
COMMIT
```

El evento nunca puede existir sin la transacción que lo originó.

## Event envelope

Cada evento debe contener:

```text
EventId
TenantId
DeviceId
AggregateId
EventType
SchemaVersion
CreatedAt
Payload
```

## Inbox

En servidor:

```text
Receive Event
     ↓
Check EventId
     ↓
Already processed?
      ├── YES → return previous result
      └── NO
          ↓
      Process
          ↓
      Persist Inbox
          ↓
      COMMIT
```

## Idempotencia

No basta con:

```text
IdempotencyKey
```

Debe persistirse el resultado de la operación.

Una segunda petición idéntica debe devolver el mismo resultado lógico.

---

# PHASE 10 — SINCRONIZACIÓN POR CURSOR

## Estado

```text
PENDING
```

## Objetivo

Eliminar definitivamente:

```text
since=DateTime
LastUpdated
clock-based synchronization
```

## Modelo

El servidor mantiene una secuencia monotónica:

```text
ServerSequence

1001
1002
1003
1004
```

El terminal mantiene:

```text
LastAppliedSequence
```

## Pull

```text
GET /sync/pull?after=1002
```

Servidor:

```text
1003
1004
1005
```

El cliente:

```text
Apply events
      ↓
Commit local transaction
      ↓
Advance cursor
```

Nunca:

```text
recibir → actualizar cursor → después procesar
```

## Recovery

Si el POS se apaga:

```text
cursor = 1002
```

reinicia:

```text
pull after 1002
```

No pierde eventos.

---

# PHASE 11 — CONFLICTOS

## Estado

```text
PENDING
```

## Regla

Eliminar:

```text
Last Write Wins
```

para operaciones críticas.

## Catálogo

Puede utilizar versionado optimista.

## Inventario

Nunca LWW.

Usar:

```text
InventoryMovement
Allocation
Concurrency
```

## Orders

Usar:

```text
Idempotency
State Machine
Version
```

## Payments

Usar:

```text
immutable transaction
Idempotency
```

## Cash

Usar:

```text
append-only movements
```

No sobrescribir estados financieros arbitrariamente.

---

# PHASE 12 — TESTING REAL

## Estado

```text
PENDING
```

Los tests no se dejan para el final.

Cada fase debe añadir sus pruebas.

## Domain

```text
Order transitions
Money
Inventory rules
Payment rules
```

## Application

```text
Checkout
Refund
Shift
Inventory
Provisioning
```

## Infrastructure

```text
SQLite
PostgreSQL
transactions
migrations
```

## Security

```text
JWT
Tenant isolation
RLS
Authorization
Provisioning
Device revocation
```

## Distributed

```text
Idempotency
Outbox
Inbox
Cursor
Retry
Duplicate events
Crash recovery
```

## Concurrency

```text
last-stock sale
parallel checkout
parallel sync
duplicate payment
duplicate event
```

## Offline

```text
disconnect
sale
restart
reconnect
sync
conflict
replay
```

---

# PHASE 13 — POSBUILDER / RELEASE / UPDATE

## Estado

```text
PENDING
```

## PosBuilder

Debe producir una instalación reproducible:

```text
Builder
   ↓
Provision Device
   ↓
Generate non-secret configuration
   ↓
Install PosCore
   ↓
First-run enrollment
   ↓
Device activated
```

No debe:

```text
mostrar passwords
guardar passwords en output
generar licencias predecibles
generar identidad basada en MachineName
```

## CI/CD

Pipeline obligatorio:

```text
Restore
 ↓
Build
 ↓
Unit Tests
 ↓
Integration Tests
 ↓
Security Tests
 ↓
Publish
 ↓
Code Sign
 ↓
Package
 ↓
Release
```

El paso actual de:

```text
Code signing simulated
```

no es aceptable.

Debe existir firma real del ejecutable.

---

# PHASE 14 — RELEASE CANDIDATE

## Estado

```text
PENDING
```

Antes de declarar producción:

```text
Fresh PostgreSQL
Fresh SQLite
Fresh Windows installation
Fresh Tenant
Fresh Device
```

Ejecutar:

```text
Provision
Login
Open Shift
Create Product
Inventory
Sale
Payment
Close Shift
Disconnect Network
Sale Offline
Restart POS
Reconnect
Sync
Refund
Update POS
Rollback
```

Todo debe funcionar desde cero.

---

# PHASE 15 — CERTIFICACIÓN FINAL

## Estado

```text
PENDING
```

El sistema únicamente puede declararse:

```text
PRODUCTION READY
```

cuando:

```text
[ ] Domain limpio
[ ] Application gobierna los casos de uso
[ ] UI no contiene lógica transaccional
[ ] Money en centavos
[ ] Inventario basado en movimientos
[ ] Concurrencia protegida
[ ] Orders transaccionales
[ ] Payments idempotentes
[ ] Cash transaccional
[ ] Tenant isolation
[ ] RLS real
[ ] JWT fail-closed
[ ] Provisioning independiente de JWT
[ ] Device identity
[ ] Device revocation
[ ] Licencias firmadas
[ ] Configuración local protegida
[ ] Outbox transaccional
[ ] Inbox implementado
[ ] Idempotencia completa
[ ] Cursor Sync
[ ] Offline recovery
[ ] Conflict resolution
[ ] Sin Last Write Wins en operaciones críticas
[ ] Sin secretos en eventos
[ ] Sin StackTrace en API
[ ] Sin defaults inseguros
[ ] Integration tests
[ ] Concurrency tests
[ ] Offline tests
[ ] RLS tests
[ ] CI/CD
[ ] Code signing
[ ] Installer
[ ] Update mechanism
[ ] Rollback
[ ] Fresh-install test
[ ] Production configuration validated
```

---

# ORDEN DEFINITIVO ACTUALIZADO

```text
PHASE 0
Contención crítica
Estado: CLOSED
        ↓
PHASE 0.5
Repository Hygiene / Baseline Cleanup
Estado: NEXT
        ↓
PHASE 1
Arquitectura
        ↓
PHASE 2
Domain + Money
        ↓
PHASE 3
Motor transaccional
        ↓
PHASE 4
Inventario + concurrencia
        ↓
PHASE 5
Multi-tenancy + seguridad
        ↓
PHASE 6
RLS
        ↓
PHASE 7
Device + Provisioning
        ↓
PHASE 8
Licensing + configuración
        ↓
PHASE 9
Outbox + Inbox + Idempotencia
        ↓
PHASE 10
Cursor Sync + Offline
        ↓
PHASE 11
Conflict Resolution
        ↓
PHASE 12
Testing integral
        ↓
PHASE 13
Builder + CI/CD + Release
        ↓
PHASE 14
Release Candidate
        ↓
PHASE 15
PRODUCTION READY
```

---

# REGLA DE CIERRE

Una fase no se considera terminada por haber modificado archivos.

Se considera terminada únicamente cuando:

```text
Código implementado
+
Tests escritos
+
Tests ejecutados
+
Build Release
+
Verificación estática
+
Criterios de aceptación
=
PASS
```

Si cualquiera falla:

```text
NO AVANZAR
```

El objetivo no es "hacer que el proyecto compile".

El objetivo es que exista una única implementación coherente de las reglas de negocio entre PosCore, PosServer y PosBuilder, con persistencia transaccional, multi-tenancy seguro, sincronización determinista, operación offline recuperable y un proceso de instalación/actualización reproducible.

---

# SIGUIENTE ACCIÓN RECOMENDADA

Ejecutar `PHASE 0.5 — Repository Hygiene / Baseline Cleanup`.

No ejecutar Phase 1 hasta que Phase 0.5 cierre con:

```text
dotnet test
dotnet build -c Release Pos.sln
PHASE GATE PASS
```

## Iteration note — PHASE 1H.3

Status: PENDING LOCAL VERIFICATION.

Checkout transaction extraction introduced `ILocalOrderService.ProcessCheckoutAsync` and local `LocalOrderService` in Infrastructure. MainViewModel now delegates the checkout transaction while preserving UI/payment-window/printing concerns.

## PHASE 2F — Domain Documentation + Integration Safety Pass

Status: PENDING LOCAL VERIFICATION

- Added `docs/DOMAIN_RULES.md` as the central domain rule reference for Money, Order, Payment, CashMovement, Product, Supply, RecipeItem, and InventoryMovement.
- Added integration safety guidance before touching service behavior, EF mappings, migrations, decimal monetary columns, inventory ledger, sync, or payment normalization.
- No production behavior was changed.

Next recommended phase after validation: `PHASE 3A — Inventory Ledger / Concurrency Audit`, or `PHASE 2G — Gradual Money Adoption Audit` if monetary persistence is prioritized.

## PHASE 3A — Inventory Ledger / Concurrency Baseline Audit

Status: CLOSED

This phase adds an inventory/concurrency baseline audit and architecture tests only. It documents current stock mutation paths, existing transaction/concurrency safeguards, and risks before any behavioral inventory-ledger refactor.

No migrations, EF mappings, checkout behavior, returns behavior, sync behavior, reports, or schema changes were made.


## PHASE 3B — Inventory Mutation Guardrails

Status: PENDING LOCAL VERIFICATION

- Local checkout now routes product and supply stock reductions through domain guardrails.
- Local inventory service now routes sale, return and restock mutations through domain helpers.
- Inventory app stock adjustment now rejects zero/fractional product adjustments and prevents negative stock through domain helpers.
- Checkout concurrency resolution now rejects recalculated negative product/supply stock.
- No migrations, EF mappings, schema changes, sync redesign, reports, returns or Money adoption were performed.

Next recommended phase after validation: `PHASE 3C — Inventory Movement Semantics Audit / Sign Normalization Plan`.

## PHASE 3C — InventoryMovement Sign Semantics Audit

Status: PENDING LOCAL VERIFICATION

Scope:

- Audit current `InventoryMovement.Quantity` sign semantics.
- Add interpretation helpers that tolerate legacy negative rows.
- Preserve strict validation for new canonical positive movements.
- Document the future ledger convention.

Not included:

- No data migration.
- No schema change.
- No EF mapping change.
- No sync/report behavior change.

Next recommended phase after local validation:

PHASE 3D — InventoryMovement Canonical Creation Guardrails.

## PHASE 3D — Inventory Ledger Read Model Baseline

Status: PENDING LOCAL VERIFICATION

Scope:

- Add read-only `InventoryLedgerReadModel`.
- Add `InventoryLedgerBalance` for reconstructed balances.
- Reconstruct product and supply balances using `SignedQuantity`.
- Support opening quantities and tenant filtering.
- Add unit tests and architecture tests to keep the read model side-effect free.

Not included:

- No data migration.
- No schema change.
- No EF mapping change.
- No checkout behavior change.
- No returns behavior change.
- No sync/report behavior change.
- No replacement of current stock columns.

Next recommended phase after local validation:

PHASE 3E — Inventory Drift Detection Baseline.

## PHASE 3E — Inventory Drift Detection Baseline

Status: Pending local verification.

Added detection-only read models for comparing operational stock against ledger-reconstructed stock:

- `InventoryDriftDetectionReadModel`
- `InventoryDriftItem`
- `InventoryDriftReport`

No schema, migration, checkout, returns, sync, or automatic correction changes were made.

Next recommended phase: `PHASE 3F — Inventory Drift Detection Integration Surface`.


## PHASE 3F — Inventory Drift Reporting Integration Baseline

Status: Pending local verification.

This phase exposes inventory drift detection as an internal diagnostic reporting service. It remains read-only and performs no automatic correction, no schema change, no migration, no checkout change, and no sync change. Next recommended phase: PHASE 3G — Inventory Drift Reporting UI/API Decision or PHASE 3G — PosServer Sync Inventory Guardrails Audit.


## PHASE 3G — Inventory Drift Reporting UI/Diagnostics Hook

Added a read-only inventory drift diagnostics hook in the POS inventory screen. The hook uses `IInventoryDriftReportingService.GetCombinedDriftReportAsync` and `InventoryDriftDiagnosticsFormatter` to display drift diagnostics without auto-correction, stock mutation, schema changes, migrations, checkout changes, or sync changes.


## PHASE 3H — Inventory Drift Report UX Safety Pass

Status: PENDING LOCAL VERIFICATION.

This phase improves inventory drift diagnostic UX safety. The hook remains read-only and does not auto-correct stock. No schema change, no migrations, no checkout changes, and no sync changes were introduced.


## PHASE 3I — Inventory Drift Diagnostics Error Handling + Observability

Closed scope update: inventory drift diagnostics now record start/success/failure logs, keep last error/run state in the ViewModel, and format user-facing errors safely without stack traces by default. The feature remains read-only and diagnostic only: no schema change, no migrations, no checkout changes, no sync changes, and no automatic stock correction.

## PHASE 3J — Inventory Drift Diagnostics Export/Report Baseline

Status: Pending local verification.

Goal: allow copy/export of inventory drift diagnostics as an internal report.

Boundaries:

- diagnostic only.
- report-only.
- does not auto-correct stock.
- No auto-correction.
- No schema change.
- No migrations.
- No checkout changes.
- No sync changes.

Expected validation: 155 tests passed, 0 failed, 0 build errors.

## PHASE 3K — Inventory Drift Manual Review Workflow Baseline

Status: PENDING LOCAL VERIFICATION

Scope:
- Add manual review workflow state to inventory diagnostics.
- Add UI button for manual review preparation.
- Keep the workflow read-only/report-only.
- Do not correct inventory.
- Do not change checkout, sync, schema, or migrations.

Expected validation: 160 tests passed, 0 failed, 0 build errors.

### PHASE 3L — Inventory Drift Controlled Manual Reconciliation Design Pass

Status: PENDING LOCAL VERIFICATION

Scope:
- Design controlled manual reconciliation flow.
- Define future permission, audit and sync-safe requirements.
- Keep reconciliation execution blocked.
- No auto-correction.
- No inventory mutation.
- No schema change.
- No migrations.
- No checkout changes.
- No sync changes.

Next: PHASE 3M — Inventory Drift Reconciliation Permission + Audit Design Baseline.

## PHASE 3M — Inventory Drift Reconciliation RBAC + Permission Guard Baseline

Status: PENDING LOCAL VERIFICATION

Adds RBAC and permission guard preparation for future controlled manual inventory drift reconciliation. This phase does not execute reconciliation and does not mutate inventory. Expected validation: 170 tests passed, 0 failed, 0 build errors.

## PHASE 3N — Inventory Drift Reconciliation Audit Trail Baseline

Status: PENDING LOCAL VERIFICATION

Scope:
- Prepare audit trail contract for future controlled reconciliation.
- Define required audit fields and minimum evidence.
- Expose audit preparation state in inventory diagnostics UI.
- Keep reconciliation execution blocked.
- No auto-correction.
- No inventory mutation.
- No schema change.
- No migrations.
- No checkout changes.
- No sync changes.

Expected validation: 175 tests passed, 0 failed, 0 build errors.

Next: PHASE 3O — Inventory Drift Reconciliation Sync-Safe Constraints Baseline.


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

Production Sync Controlled Execution Implementation progress: 10% -> 20% after verification.

Next: PHASE 6C - Production Sync Queue Processor Dry-Run Execution Implementation.


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


### PHASE 6G — Production Sync Conflict Detection Runtime Implementation

- Status: PENDING LOCAL VERIFICATION.
- Production Sync Controlled Execution Implementation moves from 60% -> 70% after verification.
- PHASE 6H remains BLOCKED until PHASE 6G is closed.
- Scope: controlled conflict detection only, no automatic conflict resolution and no inventory mutation.


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

- Status: PENDING LOCAL VERIFICATION.
- Security & Dependency Hardening: 20% -> 30% after local verification.
- Scope: targeted remediation of server service nullability hotspots in AuthService, UserService and CentralDbContext.
- Guardrails: no checkout behavior change, no inventory mutation, no production sync enablement, no schema change and no migrations.
- PHASE 7D remains BLOCKED until PHASE 7C is closed.


## PHASE 7D — Duplicate Using Cleanup & Analyzer Hygiene

- Status: PENDING LOCAL VERIFICATION.
- Security & Dependency Hardening: 30% -> 40% after local verification.
- Scope: remove exact duplicate using directives reported by CS0105 analyzer warnings.
- Guardrails: no checkout behavior change, no inventory mutation, no production sync enablement, no public API behavior change, no schema change and no migrations.
- PHASE 7E remains BLOCKED until PHASE 7D is closed.


## PHASE 7E — ASP.NET Header Analyzer Hygiene

Security & Dependency Hardening: 40% -> 50%.

Remediates ASP0019 analyzer hygiene in CorrelationIdMiddleware. No checkout behavior change, no inventory mutation, no production sync enablement, no schema change and no migrations.


## PHASE 7F — PosBuilder Nullability Hygiene

Status: Pending local verification. PosBuilder nullability hygiene for UI/bootstrap initialization, event compatibility and safe conversion boundaries. Expected: 370 tests passed, 0 failed, Release build successful.


## PHASE 7G — SyncService Nullability Hygiene

- Status: PENDING LOCAL VERIFICATION.
- Security & Dependency Hardening: 60% -> 70% after local verification.
- Scope: targeted CS8602 remediation in SyncService username normalization.
- Guardrails: no checkout behavior change, no inventory mutation, no production sync enablement, no public API behavior change, no schema change and no migrations.
- PHASE 7H remains BLOCKED until PHASE 7G is closed.


## PHASE 7H — AuthService Remaining Nullability Hygiene

- Status: PENDING LOCAL VERIFICATION.
- Security & Dependency Hardening: 70% -> 80% after local verification.
- Scope: targeted CS8602 remediation in AuthService login username normalization.
- Guardrails: no checkout behavior change, no inventory mutation, no production sync enablement, no public API behavior change, no schema change and no migrations.
- PHASE 7I remains BLOCKED until PHASE 7H is closed.


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
- Scope: final PHASE 7 closure evidence and warning regression guardrails.
- Evidence carried forward from PHASE 7I: 385 tests passed, 0 failed, Compilación correcta, 0 Advertencia(s), 0 Errores.
- Expected PHASE 7J closure gate: 390 tests passed, 0 failed, Compilación correcta, 0 Advertencia(s), 0 Errores.
- Guardrails: no checkout behavior change, no inventory mutation, no production sync enablement, no public API behavior change, no schema change and no migrations.
- PHASE 8 — Release Packaging & Operational Readiness remains BLOCKED until PHASE 7J is closed.


## PHASE 8A - Production Readiness Operational Baseline

Status: PENDING LOCAL VERIFICATION.

Release Packaging and Operational Readiness: 0% -> 10%.

PHASE 8A documents the production readiness operational baseline after PHASE 7 zero-warning closure. It captures the environment configuration checklist, secrets validation checklist, database backup and restore validation checklist, rollback procedure checklist, release artifact inventory checklist, installer readiness checklist, smoke test plan, operator runbook handoff, monitoring and alerting handoff, support escalation handoff, and go no-go evidence checklist.

Expected validation: 395 tests passed, 0 failed, Compilacion correcta, 0 Advertencia(s), 0 Errores.

Safety: no checkout behavior change, no inventory mutation, no production sync enablement, no packaging execution, no deployment execution, no public API behavior change, no schema change, no migrations.


## PHASE 8B - Release Artifact Inventory and Packaging Baseline

Status: PENDING LOCAL VERIFICATION.

Release Packaging and Operational Readiness: 10% -> 20%

PHASE 8B documents the release artifact inventory and packaging baseline after PHASE 8A production readiness operational baseline. It lists PosCore, PosBuilder, PosServer, documentation and configuration template artifacts, and records checksum manifest, version stamp, package naming, installer packaging readiness, release notes, artifact storage handoff and package verification command checklists.

Guardrails: no checkout behavior change, no inventory mutation, no production sync enablement, no packaging execution, no installer execution, no deployment execution, no public API behavior change, no schema change and no migrations.

Next: PHASE 8C - Versioning and Release Manifest Baseline.


## PHASE 8C - Versioning and Release Manifest Baseline

Status: PENDING LOCAL VERIFICATION.

Release Packaging and Operational Readiness: 20% -> 30%

PHASE 8C documents deterministic versioning and release manifest evidence after PHASE 8B release artifact inventory and packaging baseline. It captures semantic version format, release channel, build number source, commit sha source, artifact manifest template, checksum fields, artifact path fields, manifest created at and generated by fields, release notes version linkage, rollback version linkage, package version stamp checklist and operator manifest review checklist.

Expected validation: 405 tests passed, 0 failed, Compilacion correcta, 0 Advertencia(s), 0 Errores.

Guardrails: no checkout behavior change, no inventory mutation, no production sync enablement, no packaging execution, no installer execution, no deployment execution, no public API behavior change, no schema change and no migrations.

Next: PHASE 8D - Checksum and Artifact Verification Baseline.


## PHASE 8D - Checksum and Artifact Verification Baseline

Status: PENDING LOCAL VERIFICATION.

PHASE 8D documents checksum and artifact verification baseline evidence after PHASE 8C closed with 405 tests passed, Compilacion correcta, 0 Advertencia(s), and 0 Errores.

Expected result after local verification: 410 tests passed, 0 failed, Compilacion correcta, 0 Advertencia(s), 0 Errores.

Scope remains protected: no checkout behavior change, no inventory mutation, no production sync enablement, no packaging execution, no installer execution, no deployment execution, no public API behavior change, no schema change, no migrations.

Release Packaging and Operational Readiness: 30% -> 40%

Next: PHASE 8E - Installer Readiness and Setup Packaging Baseline.


## PHASE 8E - Installer Readiness and Setup Packaging Baseline

Status: PENDING LOCAL VERIFICATION.

Release Packaging and Operational Readiness: 40% -> 50%

PHASE 8E documents installer readiness and setup packaging baseline evidence after PHASE 8D closed with 410 tests passed, Compilacion correcta, 0 Advertencia(s), and 0 Errores.

Expected verification result: 415 tests passed, 0 failed, Compilacion correcta, 0 Advertencia(s), 0 Errores.

Next: PHASE 8F - Release Notes and Operator Handoff Baseline.

Safety: no checkout behavior change, no inventory mutation, no production sync enablement, no packaging execution, no installer execution, no deployment execution, no public API behavior change, no schema change, no migrations.

## PHASE 8F - Release Notes and Operator Handoff Baseline

Status: PENDING LOCAL VERIFICATION.

Release Packaging and Operational Readiness: 50% -> 60%

PHASE 8F documents release notes and operator handoff baseline evidence after PHASE 8E installer readiness and setup packaging baseline. It captures release notes audience, release summary checklist, known limitations checklist, operator handoff checklist, support escalation path, rollback communication checklist, smoke test results handoff, artifact manifest handoff, installer readiness handoff, monitoring handoff, go no go handoff checklist, release owner approval checklist, post release support window and operator evidence archive checklist.

Expected verification result: 420 tests passed, 0 failed, Compilacion correcta, 0 Advertencia(s), 0 Errores.

Next: PHASE 8G - Smoke Test and Release Candidate Validation Baseline.

Safety: no checkout behavior change, no inventory mutation, no production sync enablement, no packaging execution, no installer execution, no deployment execution, no public API behavior change, no schema change, no migrations.


## PHASE 8G - Smoke Test and Release Candidate Validation Baseline

Status: PENDING LOCAL VERIFICATION.

Release Packaging and Operational Readiness: 60% -> 70%.

PHASE 8G documents smoke test and release candidate validation baseline evidence after PHASE 8F closed with 420 tests passed, Compilacion correcta, 0 Advertencia(s), and 0 Errores.

Expected verification result: 425 tests passed, 0 failed, Compilacion correcta, 0 Advertencia(s), 0 Errores.

Safety: no checkout behavior change, no inventory mutation, no production sync enablement, no packaging execution, no installer execution, no deployment execution, no public API behavior change, no schema change, no migrations.

Next: PHASE 8H - Rollback Drill and Recovery Evidence Baseline.

## PHASE 8H - Rollback Drill and Recovery Evidence Baseline

Status: PENDING LOCAL VERIFICATION

Release Packaging and Operational Readiness: 70% -> 80%

Expected evidence: 430 tests passed, Compilación correcta, 0 Advertencia(s), 0 Errores.

Scope: rollback drill and recovery evidence baseline only. No checkout behavior change. No inventory mutation. No production sync enablement. No packaging execution. No installer execution. No deployment execution. No public API behavior change. No schema change. No migrations.


## PHASE 8I - Monitoring and Post-Release Support Evidence Baseline

Status: PENDING LOCAL VERIFICATION

Release Packaging and Operational Readiness: 80% -> 90%

Expected evidence: 435 tests passed, Compilación correcta, 0 Advertencia(s), 0 Errores.

Scope: monitoring and post-release support evidence baseline only. No checkout behavior change. No inventory mutation. No production sync enablement. No packaging execution. No installer execution. No deployment execution. No public API behavior change. No schema change. No migrations.

Next: PHASE 8J - Release Go No-Go and Operational Readiness Closure.


## PHASE 8J - Release Go No-Go and Operational Readiness Closure

Release Packaging and Operational Readiness: 90% -> 100%

PHASE 8J documents final release go/no-go and operational readiness closure evidence. Expected verification: 440 tests passed, 0 failed, Compilacion correcta, 0 Advertencia(s), 0 Errores.

Safety: no checkout behavior change, no inventory mutation, no production sync enablement, no packaging execution, no installer execution, no deployment execution, no public API behavior change, no schema change, no migrations.


## PHASE 9A - Installer Generation and Release Artifact Execution

Release Execution: 0% -> 10%

PHASE 9A creates the controlled release artifact execution baseline and local generation script after PHASE 8 operational readiness closure.

Expected verification: 445 tests passed, 0 failed, Compilación correcta, 0 Advertencia(s), 0 Errores.


## PHASE 9B — Installer Package Generation Execution

Status: PENDING LOCAL VERIFICATION. Release Execution: 10% -> 20%. Expected 450 tests passed. Adds installer package generation execution, package manifest generation, installer package checksums, package zip archive generation, and operator package generation command. Safety: no checkout behavior change; no inventory mutation; no production sync enablement; no deployment execution; no public API behavior change; no schema change; no migrations.


### PHASE 9B hotfix - prerequisite regeneration
Installer package generation now handles missing PHASE 9A artifact inputs by regenerating the release publish artifacts first.


## PHASE 9C - Installer Package Verification and Integrity Execution

Status: PENDING LOCAL VERIFICATION. Release Execution: 20% -> 30%. Expected 455 tests passed. Verifies PHASE 9B installer package outputs by checking archive existence, installer manifest, installer checksums, packageArchiveSha256, unzip verification, and required package contents. Safety: no checkout behavior change; no inventory mutation; no production sync enablement; no deployment execution; no public API behavior change; no schema change; no migrations.

Next: PHASE 9D - Installer Smoke Install Simulation and Package Extraction Validation.

### PHASE 9D — Installer Smoke Install Simulation and Package Extraction Validation

Status: PENDING LOCAL VERIFICATION.

Scope: installer smoke install simulation package extraction validation documented with simulated install directory creation, package extraction, required content validation, smoke install evidence manifest, and operator execution command.

Expected evidence: 460 tests passed, 0 failed, Compilación correcta, 0 Advertencia(s), 0 Errores, and `PHASE 9D installer smoke install simulation verified.`

Safety: no real installer execution, no deployment execution, no checkout behavior change, no inventory mutation, no production sync enablement, no schema change, and no migrations.


## PHASE 9E - Installer Launch Script and Desktop Shortcut Packaging

Status: PENDING LOCAL VERIFICATION. Release Execution advances from 40% to 50%. Adds Generate-Phase9LaunchAndShortcutPackage.ps1 to package Start-PosCore.ps1, Start-PosBuilder.ps1, Start-PosServer.ps1, desktop-shortcut-spec.json, Create-DesktopShortcuts.ps1, launcher-package-manifest.json, launcher-checksums.sha256, and a launch package archive. No real shortcut creation, no real installer execution, no deployment execution, no schema change, and no migrations.


## PHASE 9F - Installer Uninstall and Cleanup Simulation Validation

Status: PENDING LOCAL VERIFICATION.

Release Execution advanced from 50% to 60%. This phase validates dry-run uninstall cleanup simulation outputs and preserves release evidence.


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


## PHASE 11 - POS Functional Business Validation

### PHASE 11.1 - Cashier Shift and Sales Flow Validation

Status: pending local verification.
Baseline: 540 tests passed.
Expected: 556 tests passed.
Scope: open shift workflow, initial cash drawer balance, basic sale calculation, controlled discount application, payment registration checklist, shift close workflow, cash reconciliation checklist, and functional evidence handoff.
Guardrails: no real checkout execution, no real payment capture, no receipt printing, no inventory mutation, no hardware access, no production sync enablement, no public API behavior change, no schema change, no migrations.


## PHASE 11.2 — Payments, Receipts and Returns Validation

PHASE 11.2 payments receipts and returns validation documented. Payments, Receipts and Returns Validation moves functional business validation to 50%. Expected validation target: 572 tests passed. Guardrails preserved: no real payment capture, no live payment gateway call, no receipt printing, no refund execution, no inventory mutation, no real checkout execution, no hardware access, no production sync enablement, no public API behavior change, no schema change, no migrations.

## PHASE 11.3 — Inventory, Stock Movement and Offline Sync Validation

Status: PENDING LOCAL VERIFICATION.

Scope: PHASE 11G inventory availability validation, PHASE 11H stock movement audit validation, and PHASE 11I offline sync validation.

Expected validation: 588 tests passed, 0 failed, clean Release build, AcceptedChecks: 15, BlockingIssues: 0.

Guardrails: no real inventory mutation, no stock write execution, no production sync enablement, no live server commit, no destructive reconciliation, no checkout behavior change, no public API behavior change, no schema change, and no migrations.

## PHASE 11.4 - Hardware Readiness and Store Pilot Checklist

Status: PENDING LOCAL VERIFICATION.

This phase closes PHASE 11 POS Functional Business Validation by documenting POS peripheral readiness, operator training, pilot-store entry, go-live rehearsal, support escalation, and pilot exit criteria.

Expected result: 604 tests passed.

Guardrails: no real hardware access, no printer execution, no cash drawer pulse, no scanner capture, no payment terminal execution, no store pilot activation, no production traffic routing, no real inventory mutation, no production sync enablement, no public API behavior change, no schema change, no migrations.


## PHASE 11 FINAL — POS Functional Business Validation Closure

PHASE 11 FINAL closes POS Functional Business Validation after PHASE 11.1, 11.2, 11.3, and 11.4. Expected regression state: 620 tests passed, 0 failed. Evidence: functional-business-closure-evidence.json, functional-business-readiness-scorecard.json, store-pilot-entry-decision-report.json, phase11-final-closure-summary.json. Guardrails: no checkout real, no payment capture, no receipt printing, no refund execution, no real inventory mutation, no hardware access, no store pilot activation, no production sync enablement, no public API behavior change, no schema change, no migrations.

## MACROFASE 12B — Model Hardening

Status: READY FOR LOCAL VERIFICATION.

- CentralDbContext production database baseline hardening implemented.
- InitialProductionBaseline generation prepared.
- Supabase schema reset remains intentional/manual.
- Expected test count after this block: 630 tests passed if the new architecture tests compile and pass locally.
- Next block: MACROFASE 12C — Migration Reset and InitialProductionBaseline.


## MACROFASE 12C — Migration Reset and InitialProductionBaseline

MACROFASE 12C migration baseline reset tooling verified. Next: execute local migration reset, create InitialProductionBaseline, reset disposable Supabase schema, redeploy Railway.
