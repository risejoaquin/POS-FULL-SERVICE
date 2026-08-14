# GUÍA REAL DE FINALIZACIÓN — SUPER POS (HISTÓRICO)

> [!WARNING]
> **DOCUMENTO HISTÓRICO**: Este archivo se conserva únicamente para referencia histórica y auditorías previas.
> La fuente de verdad actual y activa para la finalización del proyecto se encuentra exclusivamente en `/ROADMAP_FINALIZACION_POS_ACTUALIZADO.md`.
> No deben usarse las instrucciones o afirmaciones contenidas en este documento si entran en contradicción con dicho roadmap actualizado.

## PosCore + PosServer + PosBuilder

**Versión del documento:** 1.0  
**Fecha:** 2026-08-08  
**Base:** auditoría estática del ZIP `hola-app (16).zip`

---

# 0. PROPÓSITO DE ESTA GUÍA

Esta guía define **qué cambiar, en qué orden, en qué archivos, qué comportamiento debe quedar implementado y cuándo una fase puede considerarse terminada**.

No es una lista de recomendaciones opcionales.

La regla de trabajo es:

> **No se avanza a la siguiente fase hasta que todos los criterios de salida de la fase actual estén cumplidos.**

El objetivo final es obtener:

```text
PosBuilder
    ↓
provisiona una instalación
    ↓
PosCore
    ├── opera offline
    ├── guarda transacciones localmente
    ├── imprime
    ├── controla caja
    ├── controla inventario
    └── sincroniza
            ↓
        PosServer
            ├── autentica
            ├── aísla tenants
            ├── procesa operaciones idempotentes
            ├── mantiene inventario central
            ├── audita
            └── administra licencias
```

---

# 1. ESTADO REAL DEL ZIP AUDITADO

El proyecto actualmente contiene:

```text
PosBuilder/
PosCore/
PosCore.Tests/
PosServer/
```

No existe una solución `.sln` en el ZIP.

### Tecnologías actuales

- .NET 8
- WPF
- Entity Framework Core
- SQLite local
- ASP.NET Core
- PostgreSQL
- JWT
- BCrypt
- Squirrel
- Serilog
- QuestPDF
- Npgsql

### Tamaño aproximado

- PosCore: ~8,800 líneas C#
- PosServer: ~2,600 líneas C#
- PosBuilder: ~2,000 líneas C#
- Tests: ~170 líneas C#

El sistema ya tiene muchas funcionalidades. **No debe reescribirse desde cero.**

La estrategia correcta es una migración controlada.

---

# 2. BLOQUEADORES CONFIRMADOS

Estos problemas deben corregirse sí o sí.

## 2.1 PosServer depende directamente de PosCore

Archivo:

```text
PosServer/PosServer.csproj
```

Actualmente contiene:

```xml
<ProjectReference Include="../PosCore/PosCore.csproj" />
```

Esto es incorrecto porque PosCore es una aplicación WPF.

### Acción

Eliminar esa referencia.

El objetivo final es:

```text
PosServer
   ↓
PosApplication
   ↓
PosDomain
```

y:

```text
PosCore
   ↓
PosApplication
   ↓
PosDomain
```

---

# 2.2 No existe una capa de dominio común

Actualmente los modelos de negocio están duplicados:

```text
PosCore/Models
PosServer/Models
```

y parte de la lógica está en:

```text
PosCore/Services
PosServer/Services
ViewModels
Controllers
```

### Acción

Crear:

```text
PosDomain/
PosApplication/
```

Primero se migrarán las reglas críticas:

- Order
- Payment
- Product
- Inventory
- Shift
- User
- Money
- permisos
- estados de orden

---

# 2.3 El sistema permite un TenantId enviado por header

Archivo:

```text
PosServer/Middlewares/TenantMiddleware.cs
```

Actualmente:

```csharp
tenantId = context.Request.Headers["X-Tenant-Id"].FirstOrDefault();
```

Esto no puede ser la fuente de autoridad para un usuario autenticado.

### Acción

Para endpoints autenticados:

```text
JWT TenantId
      ↓
TenantContext
      ↓
Application
```

`X-Tenant-Id` no podrá cambiar el tenant de una sesión autenticada.

---

# 2.4 Existe un JWT fallback hardcodeado

Archivo:

```text
PosServer/Program.cs
```

Existe:

```text
super_secret_fallback_jwt_key_1234567890
```

### Acción

Eliminarlo.

Si `JWT_KEY` no existe en producción:

```text
la aplicación NO inicia
```

No se debe continuar con una clave conocida.

---

# 2.5 ProvisionKey basada directamente en JWT_KEY

Archivo:

```text
PosServer/Services/AuthService.cs
```

Actualmente:

```text
request.ProvisionKey == jwtKey
```

Eso convierte el secreto de firma JWT en secreto de provisioning.

### Acción

Separar:

```text
JWT signing secret
Provisioning credential
```

El provisioning debe tener su propia autorización.

---

# 2.6 Licencia predecible en provisioning

Actualmente existe:

```text
VAL-{tenantId}-123
```

en:

```text
PosBuilder/ConfigurationGenerator.cs
PosServer/Services/AuthService.cs
```

### Acción

Eliminar completamente esa estrategia.

La licencia debe ser generada aleatoriamente por PosServer.

---

# 2.7 PosBuilder genera secretos que terminan en configuración

Actualmente genera:

```text
JWT_KEY
ADMIN_PASSWORD
EMP_PASSWORD
TENANT_ID
```

### Acción

PosBuilder no debe conocer ni distribuir el JWT signing secret del servidor.

El servidor administra sus secretos.

PosBuilder recibe únicamente los datos de instalación que necesita el terminal.

---

# 2.8 Existen credenciales/defaults inseguros

En:

```text
PosCore/Models/AppSettings.cs
```

existen valores como:

```text
VAL-TRIAL-123
TENANT_001
1234
```

### Acción

Eliminar defaults de producción.

Una instalación no provisionada debe entrar en:

```text
Setup Required
```

y no iniciar como un tenant ficticio.

---

# 2.9 Sync usa DateTime como cursor

Actualmente:

```text
_lastSyncTime
```

y:

```text
/api/sync/changes?since=...
```

### Acción

Migrar a cursor/secuencia.

Ejemplo:

```text
serverSequence = 18429
```

El cliente conserva:

```text
LastServerSequence = 18429
```

---

# 2.10 Sync utiliza Last Write Wins para entidades críticas

Actualmente:

```text
product.LastUpdated > existing.LastUpdated
```

y:

```text
order.LastUpdated > existing.LastUpdated
```

Esto no es válido como estrategia general para:

- ventas
- pagos
- inventario
- devoluciones
- movimientos de caja

### Acción

Usar:

```text
Commands/Events + Idempotency
```

para operaciones.

`LastUpdated` puede mantenerse para sincronización de datos maestros no críticos.

---

# 2.11 Inventario se modifica directamente desde la venta

En:

```text
PosServer/Services/OrderService.cs
```

la venta modifica:

```text
Product.StockQuantity
```

y además crea:

```text
InventoryMovement
```

### Acción

La fuente de verdad será:

```text
InventoryMovement
```

y `StockQuantity` será una existencia materializada/cacheada.

Toda modificación de stock deberá pasar por un único servicio:

```text
InventoryService
```

---

# 2.12 PosCore tiene inicialización de base de datos demasiado manual

Archivo:

```text
PosCore/App.xaml.cs
```

contiene:

- `EnsureCreated`
- `EnsureDeleted`
- `CREATE TABLE`
- `ALTER TABLE`
- comprobaciones manuales de columnas
- seed de datos
- modificaciones de esquema

Esto debe eliminarse progresivamente.

### Acción

Usar:

```text
EF Core migrations
```

para SQLite.

No combinar como mecanismo normal:

```text
EnsureCreated
+
ALTER TABLE manual
+
migrations
```

---

# 2.13 PosServer crea/modifica esquema al arrancar

Archivo:

```text
PosServer/Program.cs
```

actualmente intenta:

```text
Create()
CreateTables()
GenerateCreateScript()
ALTER TABLE
CREATE EXTENSION
UPDATE
```

### Acción

En producción:

```text
dotnet ef database update
```

o migraciones ejecutadas por un proceso de deployment controlado.

El servidor no debe inventar cambios de esquema silenciosamente al arrancar.

---

# 2.14 Existen métodos con NotImplementedException

Encontrados en:

```text
PosCore/Converters/InverseBooleanToVisibilityConverter.cs
PosCore/Converters/LessThanZeroConverter.cs
PosBuilder/Converters.cs
PosBuilder/InverseBooleanToVisibilityConverter.cs
```

### Acción

Implementarlos.

Criterio:

```text
0 NotImplementedException
```

en todo el código de producción.

---

# 2.15 Manejo de errores del servidor expone StackTrace

`PosServer/Program.cs` devuelve:

```text
error
details
stackTrace
```

### Acción

Producción:

```json
{
  "error": "Internal server error",
  "correlationId": "..."
}
```

Nunca enviar stack traces al cliente.

---

# 2.16 Rate limiter está desactivado

Actualmente:

```text
// app.UseRateLimiter();
```

### Acción

Activarlo para:

```text
login
refresh
license validation
provisioning
```

con políticas diferentes.

---

# 3. ESTADO FINAL QUE DEBE EXISTIR

La estructura final será:

```text
/
├── PosDomain/
├── PosApplication/
├── PosInfrastructure/
├── PosCore/
├── PosServer/
├── PosBuilder/
├── PosDomain.Tests/
├── PosApplication.Tests/
├── PosCore.Tests/
├── PosServer.Tests/
├── docs/
└── deployment/
```

Dependencias permitidas:

```text
PosDomain
    ↑
PosApplication
    ↑
PosInfrastructure
    ↑
PosCore
PosServer
PosBuilder
```

Regla:

```text
PosDomain NO conoce a nadie.
```

```text
PosApplication NO conoce WPF, ASP.NET, SQLite ni PostgreSQL.
```

```text
PosCore NO referencia PosServer.
```

```text
PosServer NO referencia PosCore.
```

---

# 4. FASE 0 — CONGELAR EL PROYECTO

## Objetivo

Crear una línea base antes de tocar arquitectura.

### Hacer

Crear una rama:

```bash
git checkout -b refactor/final-architecture
```

Crear un tag:

```bash
git tag pre-final-architecture
```

Crear una solución:

```text
Pos.sln
```

y agregar los proyectos existentes.

### Debe existir

```text
dotnet build
dotnet test
```

como comandos oficiales.

Si actualmente no se puede compilar porque el entorno no tiene SDK, instalar .NET 8 SDK antes de continuar.

### Gate

No avanzar hasta tener:

```text
[ ] Pos.sln
[ ] PosCore compila
[ ] PosServer compila
[ ] PosBuilder compila
[ ] Tests ejecutan
```

---

# 5. FASE 1 — SEPARAR EL DOMINIO

## Objetivo

Eliminar la dependencia conceptual de negocio con WPF/HTTP/EF.

Crear:

```text
PosDomain/
```

### Mover primero

```text
Order
OrderItem
Payment
Product
InventoryMovement
Shift
CashMovement
Supply
RecipeItem
ProductModifier
ModifierOption
```

### Crear Value Objects

```text
Money
TenantId
OrderId
ProductId
DeviceId
```

### Money

Para el dinero se utilizarán unidades menores enteras.

Ejemplo:

```text
149.99 → 14999
```

No se permitirán cálculos monetarios dispersos por ViewModels.

Si durante la migración se mantiene `decimal` temporalmente para compatibilidad, debe existir un único adaptador y una única ruta de cálculo. La implementación final debe converger a `Money`.

---

# 6. FASE 2 — CREAR CASOS DE USO

Crear:

```text
PosApplication/
```

Casos mínimos:

```text
Orders/
    CreateOrder
    PayOrder
    CancelOrder
    ReturnOrder

Inventory/
    AdjustInventory
    RegisterInventoryMovement

Shifts/
    OpenShift
    CloseShift
    RegisterCashMovement

Products/
    CreateProduct
    UpdateProduct
    DeleteProduct

Users/
    CreateUser
    UpdateUser
    DisableUser

Synchronization/
    PushEvents
    PullEvents
```

Regla:

```text
ViewModel → Application
Controller → Application
```

Nunca:

```text
ViewModel → DbContext
Controller → DbContext
```

---

# 7. FASE 3 — MOVER INFRAESTRUCTURA

Crear:

```text
PosInfrastructure/
```

Dividir:

```text
Persistence/
    SQLite/
    PostgreSQL/

Authentication/
    JWT/

Synchronization/
    Outbox/
    Inbox/

Devices/
    Printer/

Security/
    Secrets/
```

Aquí se colocan las implementaciones concretas.

---

# 8. FASE 4 — DESACOPLAR POSSERVER

Modificar:

```text
PosServer/PosServer.csproj
```

Eliminar:

```xml
<ProjectReference Include="../PosCore/PosCore.csproj" />
```

PosServer debe usar:

```text
PosDomain
PosApplication
PosInfrastructure
```

Los Controllers dejan de contener lógica.

Ejemplo final:

```text
OrdersController
    ↓
CreateOrderHandler
    ↓
Order aggregate
    ↓
Transaction
    ↓
Outbox
```

---

# 9. FASE 5 — TENANT SECURITY

Crear:

```text
ITenantContext
TenantContext
```

Flujo:

```text
JWT
 ↓
TenantId claim
 ↓
TenantContext
 ↓
Application
 ↓
Repository
```

## Regla

El cliente jamás decide el tenant de una operación autenticada.

Eliminar el uso de:

```text
X-Tenant-Id
```

como mecanismo de selección de tenant para usuarios autenticados.

Si se conserva temporalmente por compatibilidad, solo se aceptará cuando:

```text
no haya identidad autenticada
```

y únicamente en endpoints explícitamente diseñados para provisioning.

---

# 10. FASE 6 — ROW LEVEL SECURITY

En PostgreSQL implementar aislamiento adicional.

Todas las tablas tenant-owned deben tener:

```text
TenantId NOT NULL
```

y las relaciones críticas deberán impedir referencias cruzadas.

Aplicar RLS a las tablas principales:

```text
Users
Products
Orders
OrderItems
Payments
InventoryMovements
Supplies
RecipeItems
Shifts
CashMovements
Licenses
AuditLogs
Outbox
```

El contexto de tenant deberá establecerse en la conexión/transacción de PostgreSQL.

Esto es defensa adicional, no reemplazo de la autorización de aplicación.

---

# 11. FASE 7 — AUTENTICACIÓN

## Login

Mantener:

```text
BCrypt
```

y mensajes uniformes:

```text
Credenciales inválidas
```

No revelar:

```text
usuario existe
usuario inactivo
password incorrecto
```

por separado.

## JWT

Eliminar fallback.

Variables obligatorias:

```text
JWT_KEY
JWT_ISSUER
JWT_AUDIENCE
```

El servidor debe fallar al iniciar si falta una configuración obligatoria.

## Refresh token

Implementar:

```text
rotación
revocación
expiry
jti
```

No almacenar refresh tokens en texto plano si el diseño final permite hashing.

---

# 12. FASE 8 — PROVISIONING

Eliminar:

```text
ProvisionKey == JWT_KEY
```

Crear un flujo administrativo separado.

El proceso final:

```text
PosBuilder
   ↓
Provisioning API
   ↓
Tenant
   ↓
Admin
   ↓
License
   ↓
Terminal
   ↓
Bootstrap credentials
```

El JWT signing secret pertenece exclusivamente al servidor.

---

# 13. FASE 9 — POSBUILDER

PosBuilder debe dejar de generar una instalación arbitraria.

Debe hacer:

```text
1. Validar URL del servidor
2. Crear/provisionar tenant
3. Registrar terminal
4. Obtener configuración de terminal
5. Obtener licencia
6. Guardar configuración local
7. Inicializar cliente
8. Ejecutar health check
9. Mostrar instalación exitosa
```

No debe generar:

```text
JWT_KEY del servidor
```

No debe construir una licencia:

```text
VAL-{TenantId}-123
```

---

# 14. FASE 10 — CONFIGURACIÓN LOCAL

PosCore debe recibir una configuración de instalación como:

```json
{
  "Api": {
    "BaseUrl": "https://servidor/api"
  },
  "Tenant": {
    "Id": "..."
  },
  "Device": {
    "Id": "..."
  },
  "License": {
    "Key": "..."
  }
}
```

No guardar:

```text
JWT_KEY
```

en el cliente.

Las credenciales locales sensibles deberán estar protegidas con:

```text
Windows DPAPI / Credential Manager
```

según el tipo de secreto.

---

# 15. FASE 11 — BASE LOCAL SQLITE

Eliminar progresivamente de:

```text
PosCore/App.xaml.cs
```

todo el esquema manual:

```text
CREATE TABLE
ALTER TABLE
EnsureDeleted
EnsureCreated
```

Crear migraciones:

```text
Migrations/
```

El bootstrap final será:

```text
SQLite
 ↓
Database.Migrate()
```

Nunca borrar automáticamente una BD que tenga datos de ventas.

La lógica actual:

```text
si falta Products → EnsureDeleted()
```

debe eliminarse.

Esto es un bloqueador de producción porque puede destruir datos locales.

---

# 16. FASE 12 — OUTBOX DEFINITIVO

Localmente crear:

```text
OutboxMessage
```

con mínimo:

```text
Id
EventId
TenantId
DeviceId
AggregateId
EventType
Payload
SchemaVersion
CreatedAt
AttemptCount
NextAttemptAt
ProcessedAt
LastError
Status
```

Estados:

```text
Pending
Processing
Processed
Failed
DeadLetter
```

Una venta local debe ejecutarse así:

```text
BEGIN SQLite TRANSACTION

crear Order
crear OrderItems
crear Payment
crear InventoryMovement
crear OutboxEvent

COMMIT
```

Si falla cualquier paso:

```text
ROLLBACK
```

Nunca debe quedar:

```text
venta sin outbox
```

ni:

```text
outbox sin venta
```

para una misma operación.

---

# 17. FASE 13 — SYNC POR CURSOR

Eliminar:

```text
since=DateTime
```

como mecanismo principal.

Crear:

```text
SyncCursor
```

Ejemplo:

```text
TenantId
DeviceId
LastServerSequence
```

Servidor:

```text
GET /api/v1/sync/pull?cursor=18429
```

Respuesta:

```json
{
  "events": [],
  "nextCursor": 18455,
  "hasMore": false
}
```

---

# 18. FASE 14 — IDEMPOTENCIA

Cada operación enviada desde PosCore debe tener:

```text
IdempotencyKey
```

o `EventId` único.

PostgreSQL debe tener índices únicos:

```text
TenantId + IdempotencyKey
```

El servidor:

```text
BEGIN

buscar idempotency key

si existe:
    devolver resultado anterior

si no existe:
    ejecutar operación
    guardar resultado
    COMMIT
```

Enviar dos veces la misma venta debe producir una sola venta.

---

# 19. FASE 15 — INVENTARIO

Crear:

```text
InventoryService
```

Toda modificación de stock pasa por él.

Movimientos mínimos:

```text
Sale
Return
Restock
Adjustment
Waste
RecipeConsumption
```

La venta NO debe hacer directamente:

```text
product.StockQuantity -= quantity
```

como lógica aislada.

Debe llamar:

```text
InventoryService.RegisterSale(...)
```

El servicio:

```text
valida stock
crea movimiento
actualiza materialización
```

dentro de la misma transacción.

---

# 20. FASE 16 — CONCURRENCIA DE INVENTARIO

Escenario obligatorio:

```text
Stock = 1

Terminal A vende 1
Terminal B vende 1
```

El servidor no puede aceptar ambas operaciones.

Una debe:

```text
procesarse
```

y la otra:

```text
rechazarse por stock insuficiente
```

o entrar en la política de reserva definida por el negocio.

Nunca permitir:

```text
Stock = -1
```

si el negocio no lo permite.

Crear test de concurrencia real contra PostgreSQL.

---

# 21. FASE 17 — ESTADO DE ORDEN

Crear una máquina de estados.

Estados:

```text
Draft
Open
Paid
Closed
Cancelled
Refunded
```

Transiciones permitidas:

```text
Draft → Open
Draft → Cancelled

Open → Paid
Open → Cancelled

Paid → Closed
Paid → Refunded

Closed → Refunded
```

No permitir transiciones arbitrarias desde la UI.

---

# 22. FASE 18 — PAGOS

Un pago debe estar asociado a:

```text
Order
Tenant
Shift
IdempotencyKey
```

Validaciones:

```text
Payment total == Order total
```

salvo que la política explícita permita pagos parciales.

No aceptar desde el cliente un pago que modifique silenciosamente el total de la orden.

El servidor debe recalcular/validar:

```text
subtotal
discount
tax
total
payments
```

---

# 23. FASE 19 — CAJA / TURNOS

Operaciones:

```text
OpenShift
RegisterCashMovement
CloseShift
```

Reglas:

```text
solo un turno abierto por terminal/caja según política definida
no cerrar dos veces
no registrar movimiento en turno cerrado
no vender sin turno si la configuración exige turno
```

La validación debe estar en Application/Domain.

No depender del ViewModel.

---

# 24. FASE 20 — AUDITORÍA

Crear auditoría para operaciones sensibles.

Mínimo:

```text
Login
Logout
ProductCreated
ProductUpdated
ProductDeleted
OrderCreated
OrderCancelled
OrderRefunded
PaymentCreated
InventoryAdjusted
ShiftOpened
ShiftClosed
UserCreated
UserDisabled
RoleChanged
LicenseChanged
ConfigurationChanged
```

Campos:

```text
TenantId
UserId
DeviceId
Action
EntityType
EntityId
Timestamp
CorrelationId
OldValues
NewValues
```

La auditoría debe ser append-only para usuarios normales.

---

# 25. FASE 21 — CORRELATION ID

Cada request debe recibir:

```text
X-Correlation-ID
```

Si no existe:

```text
el servidor lo genera
```

Debe aparecer en:

```text
logs
audit
sync events
error responses
```

---

# 26. FASE 22 — API V1

Establecer:

```text
/api/v1/auth
/api/v1/orders
/api/v1/products
/api/v1/shifts
/api/v1/inventory
/api/v1/users
/api/v1/sync
/api/v1/licenses
```

No continuar aumentando endpoints bajo:

```text
/api/...
```

sin versionado.

---

# 27. FASE 23 — CONTROLADORES

Los Controllers deben ser delgados.

Ejemplo:

```text
OrdersController
    ↓
CreateOrderCommand
    ↓
CreateOrderHandler
```

No:

```text
Controller
    ↓
DbContext
    ↓
20 validaciones
    ↓
stock
    ↓
payments
    ↓
audit
```

---

# 28. FASE 24 — POSCORE

Los ViewModels actuales contienen demasiado comportamiento.

Especial atención:

```text
PosCore/ViewModels/MainViewModel.cs
PosCore/ViewModels/InventoryViewModel.cs
PosCore/ViewModels/ShiftViewModel.cs
PosCore/ViewModels/ReturnsViewModel.cs
```

La UI debe llamar casos de uso.

Ejemplo:

```text
MainViewModel
    ↓
CreateOrderUseCase
```

y no:

```text
MainViewModel
    ↓
DbContext
```

---

# 29. FASE 25 — SYNC WORKER

Reemplazar:

```text
System.Timers.Timer
```

por:

```text
BackgroundService
PeriodicTimer
CancellationToken
```

El worker debe garantizar:

```text
una sincronización simultánea
```

No dos.

Flujo:

```text
Connectivity
 ↓
Push Outbox
 ↓
Receive ACK
 ↓
Mark processed
 ↓
Pull cursor
 ↓
Apply events
 ↓
Advance cursor
```

El cursor solo avanza después de aplicar correctamente los eventos.

---

# 30. FASE 26 — REINTENTOS

Usar backoff:

```text
1s
2s
4s
8s
16s
30s
60s
```

con límite.

Errores permanentes:

```text
400
422
conflicto de negocio
```

no deben reintentarse infinitamente.

Errores transitorios:

```text
timeout
502
503
network unavailable
```

sí.

Después del máximo:

```text
DeadLetter
```

---

# 31. FASE 27 — DATOS MAESTROS VS OPERACIONES

Separar explícitamente:

## Maestros

```text
Products
ProductModifiers
Supplies
Recipes
Users
Configuration
```

Pueden sincronizarse mediante:

```text
version/cursor
```

## Operaciones

```text
Orders
Payments
InventoryMovements
CashMovements
Returns
```

Deben ser:

```text
eventos/comandos idempotentes
```

Nunca tratarlas simplemente como:

```text
"el último estado gana"
```

---

# 32. FASE 28 — LICENCIAS

Servidor:

```text
License
Tenant
Device
```

Debe validar:

```text
IsActive
ValidUntil
Device authorization
MaxTerminals
Feature entitlements
```

La licencia no se puede generar localmente.

PosCore puede cachear una licencia firmada para operación offline.

La ventana offline debe tener una política fija.

Por ejemplo:

```text
7 días
```

si esa es la política comercial definida.

No debe existir fallback:

```text
si nunca se validó → permitir
```

Actualmente existe ese fallback y debe eliminarse.

---

# 33. FASE 29 — POSBUILDER FINAL

El wizard debe terminar con:

```text
Installation completed
```

solo si todas estas pruebas pasan:

```text
[ ] API reachable
[ ] Provisioning authorized
[ ] Tenant created/verified
[ ] License created/verified
[ ] Terminal registered
[ ] Initial admin created
[ ] Configuration saved
[ ] SQLite initialized
[ ] Database migration succeeded
[ ] Login test succeeded
[ ] Initial sync succeeded
```

No mostrar éxito si solo se escribió un archivo.

---

# 34. FASE 30 — IMPRESIÓN

Mantener:

```text
RawPrinterHelper
TicketPrinterService
```

pero encapsularlos detrás de:

```text
IReceiptPrinter
```

Implementación:

```text
WindowsRawReceiptPrinter
```

La lógica de venta nunca debe conocer:

```text
winspool.drv
ESC/POS
COM
```

---

# 35. FASE 31 — BACKUP LOCAL

La BD local contiene ventas que podrían no haber llegado al servidor.

Por eso:

```text
backup antes de migraciones
```

y:

```text
backup periódico
```

El backup debe estar fuera del archivo activo.

No permitir:

```text
copiar DB mientras hay una escritura sin coordinación
```

Usar SQLite backup API o estrategia segura equivalente.

---

# 36. FASE 32 — LOGS

Mantener Serilog.

Formato:

```text
Timestamp
Level
CorrelationId
TenantId
DeviceId
UserId
Message
Exception
```

Nunca escribir:

```text
password
JWT
refresh token
API secrets
database password
```

---

# 37. FASE 33 — ERRORES HTTP

Definir:

```text
400 Validation
401 Authentication
403 Authorization
404 Not Found
409 Conflict
422 Business Rule
429 Rate Limit
500 Internal
503 Dependency unavailable
```

Crear respuesta estándar:

```json
{
  "code": "ORDER_STOCK_INSUFFICIENT",
  "message": "No hay inventario suficiente.",
  "correlationId": "..."
}
```

---

# 38. FASE 34 — RATE LIMITING

Activar rate limiting.

Especialmente:

```text
POST /auth/login
POST /auth/refresh
POST /auth/provision
POST /license/validate
```

Login debe tener límites contra brute force.

---

# 39. FASE 35 — MIGRACIONES DEL SERVIDOR

Crear migraciones EF Core formales.

Eliminar del startup:

```text
GenerateCreateScript
CreateTables
ALTER TABLE manual
seed admin automático
```

El deployment debe ejecutar:

```text
database migration
```

antes de marcar la aplicación como healthy.

---

# 40. FASE 36 — SEEDING

No crear automáticamente:

```text
admin/admin123
```

en producción.

Crear un comando/proceso explícito:

```text
provision tenant
```

El primer administrador se crea durante provisioning.

---

# 41. FASE 37 — TESTS

Actualmente los tests son insuficientes para declarar el sistema terminado.

Crear mínimo:

```text
PosDomain.Tests
PosApplication.Tests
PosServer.Tests
PosCore.Tests
```

## Domain

Probar:

```text
Money
Order states
Discounts
Inventory rules
Payment rules
Shift rules
Permissions
```

## Application

Probar:

```text
CreateOrder
PayOrder
RefundOrder
CloseShift
AdjustInventory
```

## Server

Probar contra PostgreSQL real/efímero controlado:

```text
Tenant isolation
Idempotency
Concurrency
Transactions
Authentication
Authorization
Sync
```

---

# 42. TESTS DE AISLAMIENTO MULTI-TENANT

Obligatorios.

Crear:

```text
Tenant A
Tenant B
```

Insertar:

```text
Product A
Product B
Order A
Order B
User A
User B
```

Probar:

```text
Token A
```

no puede:

```text
GET Product B
GET Order B
UPDATE Product B
DELETE Product B
```

ni aunque intente:

```text
X-Tenant-Id: B
```

Resultado esperado:

```text
403
```

o respuesta equivalente de aislamiento.

---

# 43. TEST DE DUPLICACIÓN DE VENTA

Enviar dos veces:

```text
same EventId
same IdempotencyKey
```

Resultado:

```text
1 Order
1 Payment
1 inventory deduction
```

Este test es obligatorio.

---

# 44. TEST OFFLINE

Escenario:

```text
Internet ON
login
catalog sync

Internet OFF

crear venta
cobrar
imprimir
cerrar turno

reiniciar aplicación

Internet OFF

verificar venta

Internet ON

sync

verificar servidor
```

Resultado:

```text
1 venta
1 pago
1 movimiento de inventario
1 evento procesado
```

---

# 45. TEST DE CORRUPCIÓN DE RED

Durante sync:

```text
cortar conexión
```

Debe quedar:

```text
Outbox = Pending
```

No:

```text
Processed
```

hasta recibir ACK.

---

# 46. TEST DE REINICIO

Mientras existen:

```text
1000 eventos pendientes
```

cerrar PosCore.

Volver a abrir.

Debe continuar:

```text
desde Pending
```

sin duplicar operaciones.

---

# 47. TEST DE CONCURRENCIA

Dos terminales:

```text
POS-01
POS-02
```

venden el último producto simultáneamente.

Resultado:

```text
una venta aceptada
otra rechazada
```

si el stock es 1.

---

# 48. TEST DE RETORNO

Crear:

```text
venta
```

luego:

```text
return
```

Verificar:

```text
order status
payment status
inventory movement
stock
audit
sync
```

Todo debe ser consistente.

---

# 49. TEST DE CAJA

Probar:

```text
open
sale
cash movement
close
```

y:

```text
close twice
```

Debe rechazarse el segundo cierre.

---

# 50. TEST DE LICENCIA

Probar:

```text
valid
expired
inactive
wrong tenant
wrong terminal
offline within grace
offline after grace
clock manipulation
```

Nunca permitir:

```text
never validated + offline = access
```

---

# 51. FASE 38 — OBSERVABILIDAD

Agregar:

```text
/health/live
/health/ready
```

Health ready debe comprobar:

```text
database
```

y dependencias críticas.

Métricas:

```text
sync_pending
sync_failed
sync_latency
orders_created
orders_failed
inventory_conflicts
login_failures
active_terminals
```

---

# 52. FASE 39 — DEPLOYMENT

Servidor:

```text
Docker/container
PostgreSQL
HTTPS
environment variables
migrations
health checks
```

Variables mínimas:

```text
DATABASE_URL
JWT_KEY
JWT_ISSUER
JWT_AUDIENCE
ALLOWED_ORIGINS
PROVISIONING_SECRET
```

Nunca ponerlas en:

```text
Git
appsettings.json
PosBuilder
PosCore
```

---

# 53. FASE 40 — CI/CD

Pipeline mínimo:

```text
restore
↓
build
↓
unit tests
↓
integration tests
↓
publish
↓
container
↓
migration
↓
health check
↓
release
```

El workflow existente:

```text
.github/workflows/build-release.yml
```

debe modificarse para respetar esta secuencia.

Un release que no pasa tests no se publica.

---

# 54. FASE 41 — RELEASE DE POSCORE

El release debe:

```text
build
↓
test
↓
publish self-contained/required runtime
↓
package
↓
sign
↓
upload
↓
Squirrel release
```

No distribuir manualmente carpetas de `bin/Release`.

---

# 55. FASE 42 — DOCUMENTACIÓN

Actualizar:

```text
README.md
docs/Guia_Instalacion.md
docs/Guia_Personalizacion.md
PosCore/Docs/MigrationsAndSigning.md
```

Eliminar instrucciones que ya no sean ciertas.

Especialmente:

```text
admin/admin123
EnsureCreated
ALTER TABLE manual
JWT secret en cliente
```

---

# 56. DEFINITION OF DONE FINAL

El proyecto **NO se considera finalizado** hasta que todas las casillas siguientes estén marcadas.

## Arquitectura

```text
[ ] PosServer no referencia PosCore
[ ] PosCore no referencia PosServer
[ ] PosDomain existe
[ ] PosApplication existe
[ ] PosInfrastructure existe
[ ] Dependencias cumplen dirección arquitectónica
```

## Seguridad

```text
[ ] No existen secrets hardcodeados
[ ] No existe JWT fallback
[ ] Provisioning no utiliza JWT_KEY
[ ] No existe licencia predecible
[ ] Tenant no puede ser elegido por header autenticado
[ ] Refresh tokens protegidos
[ ] Rate limiting activo
[ ] Stack traces no salen a producción
[ ] Secrets nunca aparecen en logs
```

## Base de datos

```text
[ ] PostgreSQL usa migrations
[ ] SQLite usa migrations
[ ] No existe EnsureDeleted automático
[ ] No existe schema creation manual en startup
[ ] TenantId obligatorio donde corresponde
[ ] Constraints multi-tenant correctos
[ ] RLS configurado
```

## Ventas

```text
[ ] Order lifecycle formal
[ ] Payment validation
[ ] Idempotency
[ ] Transactional save
[ ] Return flow
[ ] Cancel flow
```

## Inventario

```text
[ ] InventoryService centralizado
[ ] InventoryMovement como ledger
[ ] No existen modificaciones directas dispersas de stock
[ ] Concurrencia probada
[ ] Returns restauran inventario
[ ] Recipe consumption consistente
```

## Offline

```text
[ ] SQLite transaccional
[ ] Outbox
[ ] EventId
[ ] IdempotencyKey
[ ] Cursor
[ ] Retry
[ ] DeadLetter
[ ] Sync survives restart
[ ] Sync survives network outage
```

## PosBuilder

```text
[ ] Provisioning real
[ ] Tenant registration
[ ] Terminal registration
[ ] License registration
[ ] Configuración segura
[ ] Migration local
[ ] Login test
[ ] Initial sync test
[ ] Installation validation
```

## Tests

```text
[ ] Unit tests
[ ] Integration tests
[ ] Multi-tenant tests
[ ] Idempotency tests
[ ] Concurrency tests
[ ] Offline tests
[ ] Recovery tests
[ ] License tests
[ ] Shift tests
[ ] Return tests
```

## Release

```text
[ ] Server deployable
[ ] PostgreSQL production
[ ] HTTPS
[ ] Health checks
[ ] CI/CD
[ ] PosCore package
[ ] Code signing
[ ] Squirrel release
[ ] Upgrade test
[ ] Backup/restore test
```

---

# 57. ORDEN EXACTO DE EJECUCIÓN

Este es el orden que debe seguir el equipo.

```text
01. Crear solución y baseline
02. Crear PosDomain
03. Crear PosApplication
04. Mover Order/Product/Payment/Inventory/Shift
05. Crear tests de dominio
06. Crear PosInfrastructure
07. Desacoplar PosServer de PosCore
08. Migrar Controllers a Application
09. Migrar PosCore a Application
10. Corregir TenantContext
11. Corregir JWT
12. Corregir provisioning
13. Corregir licenciamiento
14. Migrar SQLite a migrations
15. Migrar PostgreSQL a migrations
16. Crear InventoryService
17. Crear Order state machine
18. Crear Payment workflow
19. Crear Outbox definitivo
20. Crear Inbox/idempotency en servidor
21. Cambiar Sync a cursor
22. Cambiar Sync Worker
23. Implementar retries/dead letter
24. Implementar auditoría
25. Implementar correlation ID
26. Implementar rate limiting
27. Completar PosBuilder provisioning
28. Completar pruebas multi-tenant
29. Completar pruebas de concurrencia
30. Completar pruebas offline
31. Health checks
32. CI/CD
33. Deployment PostgreSQL
34. Release PosCore
35. Firma
36. Prueba de actualización
37. Backup/restore
38. Documentación
39. Release candidate
40. Producción
```

---

# 58. REGLA DE BLOQUEO

No se debe hacer esto:

```text
"terminamos la arquitectura después"
```

ni:

```text
"primero agreguemos más features"
```

Si una fase falla, se corrige antes de avanzar.

Especialmente:

```text
Tenant Security
↓
Transactions
↓
Idempotency
↓
Sync
```

porque estas cuatro áreas forman el corazón del sistema.

---

# 59. CRITERIO REAL DE FINALIZACIÓN

"Terminado" no significa:

```text
la UI abre
+
puedo vender
+
compila
```

Significa:

```text
puedo instalarlo
+
puedo provisionar un tenant
+
puedo registrar terminales
+
puedo autenticar usuarios
+
puedo vender offline
+
puedo cerrar caja offline
+
puedo reiniciar sin perder operaciones
+
puedo recuperar conexión
+
puedo sincronizar sin duplicar
+
puedo soportar dos terminales concurrentes
+
puedo aislar tenants
+
puedo auditar operaciones
+
puedo recuperar la base
+
puedo actualizar el cliente
+
puedo desplegar el servidor
+
puedo comprobar la salud del sistema
```

Ese es el estándar que debe utilizarse para declarar **Super POS finalizado**.

---

# 60. RECOMENDACIÓN OPERATIVA PARA TRABAJAR CON IA

Si se utiliza Google AI Studio/Copilot u otra IA para realizar los cambios, **no darle todo este trabajo en un único prompt**.

Debe recibir una fase por vez.

Cada prompt debe exigir:

```text
1. Inspeccionar archivos existentes.
2. No inventar archivos que ya existen.
3. Implementar únicamente la fase indicada.
4. No romper contratos existentes sin migración.
5. Compilar.
6. Ejecutar tests.
7. Corregir errores introducidos.
8. Mostrar archivos modificados.
9. Mostrar dependencias modificadas.
10. Confirmar criterios de salida.
```

Y la IA no debe avanzar a la siguiente fase hasta que el gate de la fase actual esté satisfecho.

---

# 61. PRIMERA FASE QUE DEBE EJECUTARSE

No empieces por Sync.

No empieces por PosBuilder.

No empieces por nuevas funcionalidades.

La primera intervención real sobre el ZIP debe ser:

```text
FASE 0
Baseline + solución

FASE 1
PosDomain

FASE 2
PosApplication

FASE 3
Desacoplamiento PosServer/PosCore
```

Después:

```text
Seguridad
→ persistencia
→ dominio transaccional
→ sincronización
→ provisioning
→ testing
→ deployment
```

Ese orden evita que el sistema termine con una arquitectura parcialmente migrada y con dos implementaciones diferentes de la misma regla de negocio.

---

# 62. RESULTADO FINAL ESPERADO

La arquitectura terminada debe quedar conceptualmente así:

```text
                         ┌─────────────────┐
                         │   POSBUILDER    │
                         │                 │
                         │ Provisioning    │
                         │ Terminal setup  │
                         │ License setup   │
                         └────────┬────────┘
                                  │
                                  ▼
┌──────────────────────────────────────────────────────┐
│                     POSCORE                           │
│                                                      │
│ WPF                                                  │
│  ↓                                                   │
│ Application                                          │
│  ↓                                                   │
│ Domain                                               │
│  ↓                                                   │
│ Infrastructure                                       │
│  ├── SQLite                                          │
│  ├── Outbox                                          │
│  ├── Sync                                             │
│  ├── Printer                                          │
│  └── Secure Storage                                   │
└────────────────────────┬─────────────────────────────┘
                         │
                         │ HTTPS
                         ▼
┌──────────────────────────────────────────────────────┐
│                    POSSERVER                          │
│                                                      │
│ API                                                  │
│  ↓                                                   │
│ Authentication                                       │
│  ↓                                                   │
│ Tenant Context                                       │
│  ↓                                                   │
│ Application                                          │
│  ↓                                                   │
│ Domain                                               │
│  ↓                                                   │
│ Infrastructure                                       │
│  ├── PostgreSQL                                      │
│  ├── RLS                                             │
│  ├── Inbox/Idempotency                               │
│  ├── Audit                                           │
│  └── Licensing                                       │
└──────────────────────────────────────────────────────┘
```

Y las reglas críticas quedan centralizadas:

```text
VENTA
→ Order Use Case
→ Transaction
→ Payment
→ Inventory
→ Outbox
→ Commit
```

```text
SYNC
→ Event
→ Idempotency
→ Transaction
→ ACK
→ Cursor
```

```text
TENANT
→ JWT
→ TenantContext
→ Authorization
→ Repository
→ PostgreSQL RLS
```

```text
INSTALL
→ PosBuilder
→ Provisioning
→ Tenant
→ License
→ Device
→ Config
→ SQLite
→ Login
→ Initial Sync
```

Con esta secuencia el proyecto puede evolucionar desde el código actual hasta un sistema POS terminado sin depender de decisiones improvisadas durante la implementación.
