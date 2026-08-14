# Railway Production Environment Variables Baseline

## Variables mínimas actuales

| Variable | Obligatoria | Uso |
|---|---:|---|
| `ASPNETCORE_ENVIRONMENT` | Sí | Define `Production`, `Staging` o `Development`. |
| `PORT` | Sí, Railway | Puerto inyectado por Railway para HTTP runtime. |
| `DATABASE_URL` | Sí | Conexión PostgreSQL/Supabase si el código la usa como fuente principal. |
| `ConnectionStrings__DefaultConnection` | Sí | Cadena principal consumida por EF Core/Npgsql. |
| `JWT_KEY` | Sí | Firma/validación de tokens JWT. |
| `JWT_ISSUER` | Sí | Emisor esperado por JWT runtime validation. |
| `JWT_AUDIENCE` | Sí | Audiencia esperada por JWT runtime validation. |

## Compatibilidad transicional

Si existen estas variables, pueden conservarse temporalmente mientras se normaliza el contrato de configuración:

```text
Jwt__Issuer
Jwt__Audience
```

Pero el runtime actual validó variables estilo:

```text
JWT_KEY
JWT_ISSUER
JWT_AUDIENCE
```

## Política

- No guardar secretos en repositorio.
- No imprimir secretos en logs.
- No usar valores débiles en producción.
- No cambiar nombres de variables sin actualizar `ENVIRONMENT_VARIABLES` y scripts de validación.
