# Production Environment Variables

## Variables obligatorias actuales para Railway

| Variable | Obligatoria | Uso en código | Notas |
|---|---:|---|---|
| `ASPNETCORE_ENVIRONMENT` | Sí | .NET runtime / `builder.Environment` | Usar `Production` en Railway. |
| `PORT` | Sí | `ASPNETCORE_URLS=http://+:${PORT}` en Dockerfile | Railway normalmente la inyecta. |
| `DATABASE_URL` | Sí | `Environment.GetEnvironmentVariable("DATABASE_URL")` en `Program.cs` | Tiene prioridad sobre `ConnectionStrings__DefaultConnection`. |
| `ConnectionStrings__DefaultConnection` | Alternativa | `builder.Configuration.GetConnectionString("DefaultConnection")` | Puede quedar como fallback, pero Railway usa `DATABASE_URL`. |
| `JWT_KEY` | Sí | `Environment.GetEnvironmentVariable("JWT_KEY")` | Clave de firma JWT. Mínimo 32 bytes; recomendado 64 caracteres aleatorios. |
| `JWT_ISSUER` | Sí | `Environment.GetEnvironmentVariable("JWT_ISSUER")` en producción | Ejemplo: `PosServer`. |
| `JWT_AUDIENCE` | Sí | `Environment.GetEnvironmentVariable("JWT_AUDIENCE")` en producción | Ejemplo: `PosClient`. |
| `ALLOWED_ORIGINS` | Recomendado | CORS en `Program.cs` | Lista separada por comas. |

## Variables que NO reemplazan a las obligatorias actuales

`Jwt__Issuer` y `Jwt__Audience` pueden existir, pero el código actual exige `JWT_ISSUER` y `JWT_AUDIENCE` cuando `ASPNETCORE_ENVIRONMENT=Production`.

## Recomendación futura

Unificar la configuración con `IConfiguration` y options tipadas, pero no bloquear MACROFASE 12 por esto.
