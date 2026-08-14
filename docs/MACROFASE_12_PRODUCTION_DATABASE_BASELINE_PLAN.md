# MACROFASE 12 — Production Database Baseline Plan

## Objetivo

Reconstruir la línea base de datos del POS para que Railway + Supabase puedan desplegar de forma repetible, limpia y sin drift.

## Bloques de trabajo

### 12.A — Configuration Freeze

- Confirmar variables Railway:
  - `ASPNETCORE_ENVIRONMENT=Production`
  - `PORT`
  - `DATABASE_URL`
  - `JWT_KEY`
  - `JWT_ISSUER`
  - `JWT_AUDIENCE`
  - `ALLOWED_ORIGINS`
- Confirmar `railway.json` en raíz.
- Confirmar `Root Directory` vacío o `/`.
- Confirmar `Dockerfile Path` por `railway.json`: `PosServer/Dockerfile`.

### 12.B — Model Hardening

Antes de regenerar migraciones:

- Hacer `TenantId` requerido para entidades multi-tenant.
- Definir precisión decimal.
- Definir FK explícitas faltantes.
- Revisar delete behavior.
- Agregar índices operativos faltantes.

### 12.C — Migration Reset

- Respaldar carpeta `PosInfrastructure/Migrations` por seguridad.
- Eliminar migración anterior `InitialServer` y snapshot asociado.
- Generar nueva migración:

```powershell
dotnet ef migrations add InitialProductionBaseline `
  --project PosInfrastructure `
  --startup-project PosServer `
  --context CentralDbContext `
  --output-dir Migrations
```

### 12.D — Supabase Schema Reset

Ejecutar en Supabase SQL Editor:

```sql
DROP SCHEMA IF EXISTS public CASCADE;
CREATE SCHEMA public;
GRANT ALL ON SCHEMA public TO postgres;
GRANT ALL ON SCHEMA public TO public;
```

Después redeploy en Railway para que `Database.Migrate()` aplique la nueva línea base.

### 12.E — Deployment Validation

Validar en Railway Deploy Logs:

```text
Applying migration 'InitialProductionBaseline'
Now listening on: http://[::]:PORT
```

Validar endpoints:

```powershell
curl https://TU-APP.railway.app/health
curl https://TU-APP.railway.app/
```

### 12.F — Baseline Freeze

Después de pasar:

- No editar `InitialProductionBaseline`.
- Cualquier cambio futuro será migración incremental.
- Documentar schema final en `DATABASE_SCHEMA.md`.

## Guardrails

```text
sin producción real
sin datos de clientes reales
sin borrar una base con información importante
sin editar datos de negocio manualmente
sin reparar __EFMigrationsHistory a mano si la base es desechable
sin cambiar lógica de checkout
sin cambiar comportamiento público de API salvo configuración/migración
```
