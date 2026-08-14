# MACROFASE 12E - Production Database Baseline Closure

## Estado

**CLOSED PENDING LOCAL VERIFICATION**

Esta macrofase cierra formalmente el baseline de base de datos y despliegue productivo controlado del POS.

## Evidencia esperada

- `InitialProductionBaseline` existe en `PosInfrastructure/Migrations`.
- Supabase `public` schema fue reseteado intencionalmente porque la base era desechable.
- Railway build compila correctamente.
- Railway deploy inicia el contenedor.
- PosServer escucha usando el `PORT` inyectado por Railway.
- EF Core confirma que la base está actualizada.
- Endpoints públicos responden correctamente.

## Endpoints validados

```text
/             -> 200 OK
/health       -> 200 OK
/api/health   -> 200 OK
/health/live  -> 200 OK
/health/ready -> 200 OK + database Connected
```

## Estado de base de datos

```text
Database provider: PostgreSQL / Supabase
Migration baseline: InitialProductionBaseline
EF migrations: up to date
Schema drift anterior: resuelto
Error anterior 42P07 CashRegisterShifts already exists: resuelto
```

## Decisión de arquitectura

La línea base de producción queda congelada desde `InitialProductionBaseline`. A partir de este punto:

- No se modifican migraciones antiguas.
- No se regeneran migraciones base sin una decisión explícita.
- Cualquier cambio de esquema debe hacerse mediante migración incremental.
- No se ejecutan resets destructivos en ambientes con datos reales.

## Guardrails

```text
sin borrar datos reales
sin reset automático de Supabase
sin migraciones destructivas automáticas
sin cambiar lógica de negocio POS
sin cambiar checkout
sin cambiar inventario
sin cambiar API pública protegida
sin exponer secretos
sin tocar variables de Railway desde código
```

## Criterio de cierre

MACROFASE 12E se puede cerrar cuando:

```text
VERIFY_MACROFASE_12E_PRODUCTION_DATABASE_BASELINE_CLOSURE.ps1 -> PASS
dotnet test -> 643 passed, 0 failed
dotnet build -c Release Pos.sln -> 0 warnings, 0 errors
Railway endpoints -> 200 OK
/health/ready -> database Connected
```
