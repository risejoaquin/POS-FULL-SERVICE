# MACROFASE 12 Audit Verifier Hotfix

Este hotfix corrige el error de parsing de PowerShell en `VERIFY_MACROFASE_12_DATABASE_AUDIT_UPDATED.ps1`.

## Causa

El verificador usaba comillas escapadas con `\"` dentro de una cadena de PowerShell. En PowerShell el escape correcto para comillas dobles dentro de una cadena doble no es `\"`; por eso el parser detenía el script antes de validar los artefactos.

## Corrección

Los marcadores del verificador fueron convertidos a cadenas con comillas simples, permitiendo validar textos que contienen comillas dobles, por ejemplo:

```text
42P07: relation "CashRegisterShifts" already exists
```

## Alcance

- No cambia lógica de negocio.
- No cambia migraciones.
- No cambia esquema.
- No toca Railway.
- No toca Supabase.
- Solo corrige el verificador local de auditoría.
