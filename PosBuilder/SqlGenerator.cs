using System;

namespace PosBuilder
{
    public static class SqlGenerator
    {
        private static string EscapeSql(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return input.Replace("'", "''");
        }

        public static string GenerateTenantSql(string storeName, string tenantId, string adminUser, string adminPin, string empUser, string empPin)
        {
            string safeStoreName = EscapeSql(storeName);
            string safeTenantId = EscapeSql(tenantId);
            string safeAdminUser = EscapeSql(adminUser);
            string safeAdminPin = EscapeSql(adminPin);
            string safeEmpUser = EscapeSql(empUser);
            string safeEmpPin = EscapeSql(empPin);
            string safeLicenseKey = $"VAL-{safeTenantId}-123";

            return $@"-- Configuración inicial para {safeStoreName} ({safeTenantId})

-- Asegurar que pgcrypto esté disponible
CREATE EXTENSION IF NOT EXISTS pgcrypto;

INSERT INTO ""Users"" (""Username"", ""PasswordHash"", ""Role"", ""TenantId"", ""IsActive"", ""CreatedAt"") 
VALUES 
('{safeAdminUser}', crypt('{safeAdminPin}', gen_salt('bf')), 'Admin', '{safeTenantId}', true, CURRENT_TIMESTAMP),
('{safeEmpUser}', crypt('{safeEmpPin}', gen_salt('bf')), 'Cajero', '{safeTenantId}', true, CURRENT_TIMESTAMP)
ON CONFLICT (""Username"", ""TenantId"") DO NOTHING;

-- Agregar Licencia (válida por 1 año por defecto)
INSERT INTO ""Licenses"" (""LicenseKey"", ""TenantId"", ""Description"", ""IsActive"", ""MaxTerminals"", ""ValidUntil"")
VALUES 
('{safeLicenseKey}', '{safeTenantId}', 'Licencia Inicial {safeStoreName}', true, 3, CURRENT_TIMESTAMP + interval '1 year');

-- Datos de Prueba (Productos y Modificadores)
INSERT INTO ""Products"" (""Name"", ""Barcode"", ""Price"", ""StockQuantity"", ""MinStockThreshold"", ""Category"", ""TenantId"", ""CustomAttributes"") VALUES
('Café Americano', '75010001', 35.00, 100, 10, 'Bebidas', '{safeTenantId}', '{{}}'::jsonb),
('Capuchino', '75010002', 45.00, 50, 5, 'Bebidas', '{safeTenantId}', '{{}}'::jsonb),
('Galleta de Chispas', '75010003', 15.00, 30, 10, 'Postres', '{safeTenantId}', '{{}}'::jsonb),
('Taco al Pastor', '75010004', 20.00, 200, 20, 'Alimentos', '{safeTenantId}', '{{}}'::jsonb),
('Refresco Cola 600ml', '75010005', 18.00, 100, 20, 'Bebidas', '{safeTenantId}', '{{}}'::jsonb);

INSERT INTO ""ProductModifiers"" (""Name"", ""Description"", ""IsRequired"", ""MinSelections"", ""MaxSelections"", ""TenantId"") VALUES
('Tipo de Leche', 'Selecciona el tipo de leche para tu bebida', true, 1, 1, '{safeTenantId}'),
('Endulzante', 'Agrega endulzante', false, 0, 2, '{safeTenantId}'),
('Extras Pastor', 'Con todo o sin algo', false, 0, 3, '{safeTenantId}');

-- Insert options
WITH pm_leche AS (SELECT ""Id"" FROM ""ProductModifiers"" WHERE ""Name"" = 'Tipo de Leche' AND ""TenantId"" = '{safeTenantId}' LIMIT 1),
     pm_endulzante AS (SELECT ""Id"" FROM ""ProductModifiers"" WHERE ""Name"" = 'Endulzante' AND ""TenantId"" = '{safeTenantId}' LIMIT 1),
     pm_extras AS (SELECT ""Id"" FROM ""ProductModifiers"" WHERE ""Name"" = 'Extras Pastor' AND ""TenantId"" = '{safeTenantId}' LIMIT 1)
INSERT INTO ""ModifierOptions"" (""ProductModifierId"", ""Name"", ""PriceAdjustment"", ""IsDefault"", ""SortOrder"", ""TenantId"") VALUES
((SELECT ""Id"" FROM pm_leche), 'Entera', 0.00, true, 1, '{safeTenantId}'),
((SELECT ""Id"" FROM pm_leche), 'Deslactosada', 5.00, false, 2, '{safeTenantId}'),
((SELECT ""Id"" FROM pm_leche), 'Almendra', 10.00, false, 3, '{safeTenantId}'),
((SELECT ""Id"" FROM pm_endulzante), 'Azúcar', 0.00, true, 1, '{safeTenantId}'),
((SELECT ""Id"" FROM pm_endulzante), 'Splenda', 0.00, false, 2, '{safeTenantId}'),
((SELECT ""Id"" FROM pm_extras), 'Sin Cebolla', 0.00, false, 1, '{safeTenantId}'),
((SELECT ""Id"" FROM pm_extras), 'Sin Cilantro', 0.00, false, 2, '{safeTenantId}'),
((SELECT ""Id"" FROM pm_extras), 'Extra Queso', 10.00, false, 3, '{safeTenantId}');

-- Linking
WITH p_capuchino AS (SELECT ""Id"" FROM ""Products"" WHERE ""Barcode"" = '75010002' AND ""TenantId"" = '{safeTenantId}' LIMIT 1),
     p_taco AS (SELECT ""Id"" FROM ""Products"" WHERE ""Barcode"" = '75010004' AND ""TenantId"" = '{safeTenantId}' LIMIT 1),
     pm_leche AS (SELECT ""Id"" FROM ""ProductModifiers"" WHERE ""Name"" = 'Tipo de Leche' AND ""TenantId"" = '{safeTenantId}' LIMIT 1),
     pm_endulzante AS (SELECT ""Id"" FROM ""ProductModifiers"" WHERE ""Name"" = 'Endulzante' AND ""TenantId"" = '{safeTenantId}' LIMIT 1),
     pm_extras AS (SELECT ""Id"" FROM ""ProductModifiers"" WHERE ""Name"" = 'Extras Pastor' AND ""TenantId"" = '{safeTenantId}' LIMIT 1)
INSERT INTO ""ProductModifierLinks"" (""ProductId"", ""ProductModifierId"", ""SortOrder"", ""TenantId"") VALUES
((SELECT ""Id"" FROM p_capuchino), (SELECT ""Id"" FROM pm_leche), 1, '{safeTenantId}'),
((SELECT ""Id"" FROM p_capuchino), (SELECT ""Id"" FROM pm_endulzante), 2, '{safeTenantId}'),
((SELECT ""Id"" FROM p_taco), (SELECT ""Id"" FROM pm_extras), 1, '{safeTenantId}');
";
        }
    }
}
