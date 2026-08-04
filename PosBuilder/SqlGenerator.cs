using System;

namespace PosBuilder
{
    public static class SqlGenerator
    {
        public static string GenerateTenantSql(string storeName, string tenantId, string adminUser, string adminPin, string empUser, string empPin)
        {
            return $@"-- Script generado de ejemplo
-- La base de datos ahora se aprovisiona mediante la API (PosServer).
-- Este archivo se mantiene por compatibilidad.
";
        }
    }
}
