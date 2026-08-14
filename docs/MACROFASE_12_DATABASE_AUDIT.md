# MACROFASE 12 — Production Database Baseline Audit

## Estado auditado

Fuente auditada: proyecto actualizado con Railway Docker/config-as-code y diagnóstico de build context.

Resultado de infraestructura observado por logs del usuario:

- Railway build: PASS.
- Dockerfile path: `PosServer/Dockerfile`: PASS.
- Build context raíz: corregido con `Root Directory: /` o vacío.
- Variables JWT de producción: superadas después de agregar `JWT_KEY`, `JWT_ISSUER`, `JWT_AUDIENCE`.
- Conexión PostgreSQL/Supabase: PASS.
- EF Core `Database.Migrate()`: FAIL por schema drift.

Error actual de despliegue:

```text
42P07: relation "CashRegisterShifts" already exists
```

## Diagnóstico ejecutivo

El problema actual no es Railway, Docker, JWT ni conexión a base de datos. El problema es que la base de datos Supabase ya tenía tablas existentes, pero EF Core no tenía un historial de migraciones consistente en `__EFMigrationsHistory`.

La base quedó en estado híbrido:

- EF Core intentó crear `__EFMigrationsHistory`.
- EF Core empezó a aplicar `20260810230421_InitialServer`.
- La migración pudo crear `AuditLogs`.
- La migración falló al crear `CashRegisterShifts` porque esa tabla ya existía.

Como la base no contiene datos importantes, la decisión correcta es reinicializar el esquema y crear una línea base limpia.

## Hallazgos del código

### 1. Migraciones automáticas en arranque

`PosServer/Program.cs` ejecuta migraciones en startup:

```csharp
dbContext.Database.Migrate();
```

Riesgo: en producción real, cada arranque del contenedor intenta aplicar migraciones automáticamente. Esto es útil durante staging, pero debe quedar gobernado por una política explícita antes de producción final.

Recomendación para MACROFASE 12:

- Mantenerlo temporalmente para reconstruir Supabase staging.
- Después agregar una compuerta explícita: `APPLY_DATABASE_MIGRATIONS=true`.
- En producción final, ejecutar migraciones como paso controlado, no accidental.

### 2. Contrato de variables JWT

El código usa estas variables en producción:

```text
JWT_KEY
JWT_ISSUER
JWT_AUDIENCE
```

Aunque también existe fallback para `Jwt:Issuer` y `Jwt:Audience`, en producción el código exige específicamente `JWT_ISSUER` y `JWT_AUDIENCE`.

Recomendación:

- Documentar oficialmente `JWT_KEY`, `JWT_ISSUER`, `JWT_AUDIENCE`.
- Evitar mezclar `Jwt__Issuer` / `Jwt__Audience` con `JWT_ISSUER` / `JWT_AUDIENCE` en Railway.

### 3. Migración servidor actual

Migración detectada:

```text
PosInfrastructure/Migrations/20260810230421_InitialServer.cs
```

Modelo snapshot:

```text
PosInfrastructure/Migrations/CentralDbContextModelSnapshot.cs
```

Nombre recomendado para reemplazo:

```text
InitialProductionBaseline
```

### 4. Tablas del modelo servidor

`CentralDbContext` expone 17 conjuntos persistentes:

```text
AuditLogs
CashRegisterShifts
CashMovements
InboxMessages
InventoryMovements
Licenses
ModifierOptions
OrderItems
Orders
OutboxMessages
Payments
ProductModifierLinks
ProductModifiers
Products
RecipeItems
Supplies
Users
```

Estas son las tablas que debe crear una nueva línea base de producción.

### 5. Multi-tenancy

La mayoría de entidades persistentes tienen `TenantId` y query filters por tenant.

Riesgo encontrado: algunas entidades tienen `TenantId` nullable en dominio aunque el modelo operativo requiere aislamiento estricto por tenant.

Entidades a revisar para obligatoriedad de `TenantId`:

```text
User
License
CashRegisterShift
InboxMessage
```

Recomendación:

- Para entidades multi-tenant de negocio, `TenantId` debe ser requerido.
- Excepciones permitidas solo para entidades globales explícitas, si existen.

### 6. Precisión decimal

El modelo contiene montos y cantidades decimales:

```text
Order.SubTotal
Order.TaxAmount
Order.TotalAmount
Payment.Amount
CashMovement.Amount
CashRegisterShift.StartingCash
CashRegisterShift.ExpectedEndingCash
CashRegisterShift.ActualEndingCash
CashRegisterShift.Difference
Product.Price
Supply.Cost
Supply.Stock
Supply.MinStockThreshold
RecipeItem.Quantity
ModifierOption.PriceAdjustment
InventoryMovement.Quantity
```

Riesgo: la migración actual usa `numeric` sin precisión explícita en PostgreSQL.

Recomendación:

- Dinero: `numeric(18,2)` o, idealmente en futuras fases, centavos enteros.
- Cantidades/inventario: `numeric(18,3)` o `numeric(18,4)` según negocio.
- Definir esto en `CentralDbContext.OnModelCreating` antes de regenerar baseline.

### 7. Foreign Keys y relaciones

Relaciones detectadas en la migración actual:

```text
CashMovements -> CashRegisterShifts
Payments -> Orders
ModifierOptions -> ProductModifiers
OrderItems -> Orders
OrderItems -> Products
ProductModifierLinks -> ProductModifiers
ProductModifierLinks -> Products
RecipeItems -> Products
RecipeItems -> Supplies
```

Relaciones a revisar:

```text
Orders.ShiftId
Payments.ShiftId
InventoryMovements.ProductId
InventoryMovements.SupplyId
```

Recomendación:

- Crear FK explícitas donde el modelo de negocio lo requiera.
- Definir `DeleteBehavior.Restrict`/`NoAction` para datos auditables.
- Permitir cascada solo en detalles claramente dependientes, por ejemplo `Order -> OrderItems`, si así se decide.

### 8. Índices actuales relevantes

Índices detectados:

```text
Users:             Username + TenantId unique
Users:             TenantId
Products:          TenantId + Barcode unique
Products:          TenantId
Orders:            TenantId + OrderDate
Orders:            TenantId + IdempotencyKey unique, filter IdempotencyKey != ''
Payments:          TenantId + IdempotencyKey unique, filter IdempotencyKey != ''
OrderItems:        OrderId
```

Índices recomendados adicionales para baseline:

```text
InventoryMovements: TenantId + ProductId + MovementDate
InventoryMovements: TenantId + SupplyId + MovementDate
CashRegisterShifts: TenantId + OpenedAt
CashMovements:      TenantId + ShiftId + CreatedAt
OutboxMessages:     TenantId + Status + NextAttemptAt
InboxMessages:      TenantId + EventId unique
Licenses:           TenantId + LicenseKey unique
```

### 9. Naming y arquitectura

El servidor usa entidades de `PosDomain.Entities` en `CentralDbContext`, mientras `PosServer/Models` contiene modelos duplicados para varios conceptos (`Order`, `Product`, `Payment`, etc.).

Riesgo: duplicación conceptual entre dominio y servidor puede provocar drift de contratos.

Recomendación:

- Mantener `PosDomain.Entities` como modelo persistente oficial.
- Usar `PosServer/Models` solo como DTO si corresponde.
- Documentar la diferencia para evitar mezclar persistencia y contratos HTTP.

## Decisión arquitectónica

Dado que Supabase actual no contiene datos importantes, la decisión correcta es:

```text
RESET CONTROLADO DEL ESQUEMA
+
NUEVA MIGRACIÓN BASE
+
VALIDACIÓN AUTOMÁTICA
```

No se recomienda reparar manualmente `__EFMigrationsHistory` porque la base es desechable y existe evidencia de drift parcial.

## Criterio de salida para MACROFASE 12

La macrofase queda cerrada cuando:

```text
1. Supabase schema public fue reinicializado.
2. Existe una única migración base: InitialProductionBaseline.
3. Railway aplica la migración sin errores.
4. La API inicia y queda escuchando.
5. GET /health responde OK.
6. No hay migraciones pendientes.
7. DATABASE_SCHEMA.md y MIGRATION_BASELINE.md quedan actualizados.
```
