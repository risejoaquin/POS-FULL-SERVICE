# Railway Deployment Validation Evidence

## URL pública

```text
https://pos-full-service-production.up.railway.app/
```

## Respuestas observadas

```json
{
  "service": "POS-FULL-SERVICE API",
  "status": "running",
  "environment": "PRODUCTION"
}
```

```json
{
  "status": "Healthy",
  "service": "POS-FULL-SERVICE API"
}
```

```json
{
  "status": "Healthy",
  "database": "Connected"
}
```

## Endpoints de validación

```text
GET /              -> service running
GET /health        -> service healthy
GET /api/health    -> service healthy alias
GET /health/live   -> liveness healthy
GET /health/ready  -> readiness healthy + database connected
```

## Deploy log markers esperados

```text
RAILWAY RUNTIME PORT BINDING START
PORT=8080
ASPNETCORE_URLS=http://0.0.0.0:8080
Starting PosServer...
No migrations were applied. The database is already up to date.
```

## Observación menor

El navegador puede pedir `/favicon.ico`. Si responde `400`, no bloquea el backend ni el baseline. Puede corregirse después con un endpoint estático o respuesta 204.
