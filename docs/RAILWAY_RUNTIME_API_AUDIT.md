# Railway Runtime API Audit — MACROFASE 12D

## Diagnóstico

Railway ya compila y despliega la imagen, y el contenedor ejecuta `PosServer.dll`. Los logs muestran:

- `RAILWAY RUNTIME PORT BINDING START`
- `PORT=8080`
- `ASPNETCORE_URLS=http://0.0.0.0:8080`
- EF Core conecta a PostgreSQL/Supabase.
- `No migrations were applied. The database is already up to date.`

La causa probable de que el dominio público responda `502 Bad Gateway` o no responda rutas HTTP es que `Program.cs` finalizaba después de ejecutar migraciones y no entraba en `app.Run()`.

## Hallazgos de código

### H1 — Bloqueante: faltaba `app.Run()`

`Program.cs` configuraba endpoints, middlewares y migraciones, pero terminaba después de `dbContext.Database.Migrate();`. Sin `app.Run()`, ASP.NET Core no mantiene el servidor escuchando solicitudes HTTP. Railway puede marcar el deployment como exitoso porque el proceso terminó sin una excepción, pero el proxy público no encuentra ningún servidor activo y puede responder `502`.

### H2 — Health endpoints no estaban normalizados

Existía `HealthController` con rutas:

- `/health/live`
- `/health/ready`
- `/health/metrics`
- `/metrics`

Pero no existían alias simples:

- `/health`
- `/api/health`

Eso hacía confusa la validación manual desde Railway.

### H3 — Tenant middleware bloqueaba health endpoints sin tenant

`TenantMiddleware` eximía `/`, Swagger, licencia y auth, pero no eximía `/health`, `/api/health` ni `/health/*`. Después de que `app.Run()` funcione, esos endpoints podrían responder `400` por falta de `TenantId` si pasan por el middleware.

### H4 — HTTPS redirection puede ser riesgosa detrás de Railway

Railway termina TLS en el edge y se comunica con el contenedor por HTTP. Se agregó `UseForwardedHeaders` y se evita `UseHttpsRedirection` cuando el runtime detecta variables de Railway. Esto evita redirecciones problemáticas detrás del proxy.

### H5 — Docker runtime port binding ya está corregido

El arranque debe hacerse mediante `scripts/railway/start-posserver.sh`, que define `ASPNETCORE_URLS` en runtime usando `PORT`.

## Cambios aplicados

- Agregado `app.Run();` al final de `Program.cs`.
- Agregados logs de auditoría runtime antes del arranque.
- Agregados endpoints públicos mínimos:
  - `/`
  - `/health`
  - `/api/health`
- Eximidos health endpoints en `TenantMiddleware`.
- Agregado `UseForwardedHeaders` para Railway/proxy.
- Saltado `UseHttpsRedirection` en runtime Railway.
- Conservado `HealthController` existente.
- Conservado `Database.Migrate()` por ahora para validar `InitialProductionBaseline` en esta macrofase.

## Resultado esperado

Después del redeploy, los logs deben mostrar:

```text
POS Server runtime audit: startup completed; entering app.Run().
```

El dominio debe responder:

```text
GET /          -> 200 OK
GET /health    -> 200 OK
GET /api/health -> 200 OK
GET /health/live -> 200 OK
GET /health/ready -> 200 OK si DB conecta
```

## Guardrails

- Sin cambios de lógica de negocio.
- Sin cambios de checkout.
- Sin cambios de inventario.
- Sin schema change.
- Sin nuevas migraciones.
- Sin reset de Supabase.
- Sin cambios a secrets.
- Sin cambio de endpoints productivos existentes.
