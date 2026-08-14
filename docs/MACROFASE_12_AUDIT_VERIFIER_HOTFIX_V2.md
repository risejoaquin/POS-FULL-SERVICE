# MACROFASE 12 Audit Verifier Hotfix V2

## Motivo

El verificador anterior buscaba el marcador exacto:

```text
CentralDbContext expone 17 conjuntos persistentes
```

pero el documento de auditoría contiene el nombre de la clase con formato Markdown:

```text
`CentralDbContext` expone 17 conjuntos persistentes
```

PowerShell no encontraba la coincidencia literal y detenía la validación aunque la auditoría sí estaba presente.

## Corrección

El marcador se relajó a:

```text
expone 17 conjuntos persistentes
```

Esto conserva la intención de validación sin depender del formato Markdown del nombre de clase.

## Alcance

- Sin cambios de lógica de negocio.
- Sin cambios de migraciones.
- Sin cambios en Supabase.
- Sin cambios en Railway.
- Solo corrección del script de verificación.
