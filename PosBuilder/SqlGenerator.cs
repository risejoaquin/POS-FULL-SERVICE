using System;

namespace PosBuilder
{
    public static class SqlGenerator
    {
        public static string GenerateTenantSql(PosBuilder.Models.ConfigModel model)
        {
            
            var sb = new System.Text.StringBuilder();

            sb.AppendLine("-- 0. Función de Automatización para Concurrencia Optimista (RowVersion)");
            sb.AppendLine("CREATE OR REPLACE FUNCTION increment_row_version()");
            sb.AppendLine("RETURNS TRIGGER AS $$");
            sb.AppendLine("BEGIN");
            sb.AppendLine("    NEW.\"RowVersion\" = OLD.\"RowVersion\" + 1;");
            sb.AppendLine("    NEW.\"LastUpdated\" = CURRENT_TIMESTAMP;");
            sb.AppendLine("    RETURN NEW;");
            sb.AppendLine("END;");
            sb.AppendLine("$$ LANGUAGE plpgsql;");

            sb.AppendLine("-- 1. Tenants y Configuración");
            sb.AppendLine("CREATE TABLE IF NOT EXISTS \"Tenants\" (\"Id\" TEXT PRIMARY KEY, \"Name\" TEXT NOT NULL, \"IsActive\" BOOLEAN NOT NULL DEFAULT TRUE, \"CreatedAt\" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP, \"RowVersion\" INTEGER NOT NULL DEFAULT 1);");
            sb.AppendLine("CREATE TABLE IF NOT EXISTS \"TenantProfiles\" (\"Id\" SERIAL PRIMARY KEY, \"TenantId\" TEXT NOT NULL REFERENCES \"Tenants\"(\"Id\") ON DELETE CASCADE, \"BusinessType\" TEXT NOT NULL, \"ConfigJson\" JSONB NOT NULL DEFAULT '{}', \"CreatedAt\" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP, CONSTRAINT \"UQ_TenantProfiles_TenantId\" UNIQUE (\"TenantId\"));");

            sb.AppendLine("-- 2. Usuarios y Licencias");
            sb.AppendLine("CREATE TABLE IF NOT EXISTS \"Users\" (\"Id\" SERIAL PRIMARY KEY, \"Username\" TEXT NOT NULL, \"PasswordHash\" TEXT NOT NULL, \"TenantId\" TEXT NOT NULL REFERENCES \"Tenants\"(\"Id\") ON DELETE CASCADE, \"Role\" TEXT NOT NULL, \"IsActive\" BOOLEAN NOT NULL DEFAULT TRUE, \"CreatedBy\" TEXT, \"UpdatedBy\" TEXT, \"CreatedAt\" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP, \"LastUpdated\" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP, \"RowVersion\" INTEGER NOT NULL DEFAULT 1);");
            sb.AppendLine("CREATE TABLE IF NOT EXISTS \"Licenses\" (\"Id\" SERIAL PRIMARY KEY, \"LicenseKey\" TEXT NOT NULL, \"TenantId\" TEXT NOT NULL REFERENCES \"Tenants\"(\"Id\") ON DELETE CASCADE, \"Description\" TEXT NOT NULL, \"IsActive\" BOOLEAN NOT NULL DEFAULT TRUE, \"MaxTerminals\" INTEGER NOT NULL DEFAULT 1 CHECK (\"MaxTerminals\" > 0), \"ValidUntil\" TIMESTAMP NOT NULL, \"CreatedAt\" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP, \"RowVersion\" INTEGER NOT NULL DEFAULT 1);");

            sb.AppendLine("-- 3. Productos e Inventario");
            sb.AppendLine("CREATE TABLE IF NOT EXISTS \"Products\" (\"Id\" SERIAL PRIMARY KEY, \"Name\" TEXT NOT NULL, \"Barcode\" TEXT, \"Price\" NUMERIC(10,2) NOT NULL CHECK (\"Price\" >= 0.00), \"StockQuantity\" INTEGER NOT NULL DEFAULT 0, \"MinStockThreshold\" INTEGER NOT NULL DEFAULT 10 CHECK (\"MinStockThreshold\" >= 0), \"Category\" TEXT, \"UnitOfMeasure\" TEXT NOT NULL DEFAULT 'pza', \"IsService\" BOOLEAN NOT NULL DEFAULT FALSE, \"TenantId\" TEXT NOT NULL REFERENCES \"Tenants\"(\"Id\") ON DELETE CASCADE, \"CustomAttributes\" JSONB, \"CreatedBy\" TEXT, \"UpdatedBy\" TEXT, \"LastUpdated\" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP, \"RowVersion\" INTEGER NOT NULL DEFAULT 1, CONSTRAINT \"PK_Products_Tenant_Id\" UNIQUE (\"Id\", \"TenantId\"));");
            sb.AppendLine("CREATE TABLE IF NOT EXISTS \"StockMovements\" (\"Id\" SERIAL PRIMARY KEY, \"ProductId\" INTEGER NOT NULL, \"TenantId\" TEXT NOT NULL REFERENCES \"Tenants\"(\"Id\") ON DELETE CASCADE, \"ChangeAmount\" INTEGER NOT NULL CHECK (\"ChangeAmount\" <> 0), \"Reason\" TEXT NOT NULL, \"ReferenceId\" TEXT, \"CreatedBy\" TEXT, \"CreatedAt\" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP, CONSTRAINT \"FK_StockMovements_Product\" FOREIGN KEY (\"ProductId\", \"TenantId\") REFERENCES \"Products\"(\"Id\", \"TenantId\") ON DELETE CASCADE);");

            sb.AppendLine("-- 4. Modificadores y Opciones");
            sb.AppendLine("CREATE TABLE IF NOT EXISTS \"ProductModifiers\" (\"Id\" SERIAL PRIMARY KEY, \"Name\" TEXT NOT NULL, \"Description\" TEXT NOT NULL, \"IsRequired\" BOOLEAN NOT NULL DEFAULT FALSE, \"MinSelections\" INTEGER NOT NULL DEFAULT 0 CHECK (\"MinSelections\" >= 0), \"MaxSelections\" INTEGER NOT NULL DEFAULT 1 CHECK (\"MaxSelections\" >= \"MinSelections\"), \"TenantId\" TEXT NOT NULL REFERENCES \"Tenants\"(\"Id\") ON DELETE CASCADE, \"LastUpdated\" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP, CONSTRAINT \"PK_ProductModifiers_Tenant_Id\" UNIQUE (\"Id\", \"TenantId\"));");
            sb.AppendLine("CREATE TABLE IF NOT EXISTS \"ModifierOptions\" (\"Id\" SERIAL PRIMARY KEY, \"ProductModifierId\" INTEGER NOT NULL, \"Name\" TEXT NOT NULL, \"PriceAdjustment\" NUMERIC(10,2) NOT NULL DEFAULT 0.00 CHECK (\"PriceAdjustment\" >= 0.00), \"IsDefault\" BOOLEAN NOT NULL DEFAULT FALSE, \"SortOrder\" INTEGER NOT NULL DEFAULT 0, \"TenantId\" TEXT NOT NULL REFERENCES \"Tenants\"(\"Id\") ON DELETE CASCADE, CONSTRAINT \"PK_ModifierOptions_Tenant_Id\" UNIQUE (\"Id\", \"TenantId\"), CONSTRAINT \"FK_ModifierOptions_Modifier\" FOREIGN KEY (\"ProductModifierId\", \"TenantId\") REFERENCES \"ProductModifiers\"(\"Id\", \"TenantId\") ON DELETE CASCADE);");
            sb.AppendLine("CREATE TABLE IF NOT EXISTS \"ProductModifierLinks\" (\"Id\" SERIAL PRIMARY KEY, \"ProductId\" INTEGER NOT NULL, \"ProductModifierId\" INTEGER NOT NULL, \"SortOrder\" INTEGER NOT NULL DEFAULT 0, \"TenantId\" TEXT NOT NULL REFERENCES \"Tenants\"(\"Id\") ON DELETE CASCADE, CONSTRAINT \"FK_ProductModifierLinks_Product\" FOREIGN KEY (\"ProductId\", \"TenantId\") REFERENCES \"Products\"(\"Id\", \"TenantId\") ON DELETE CASCADE, CONSTRAINT \"FK_ProductModifierLinks_Modifier\" FOREIGN KEY (\"ProductModifierId\", \"TenantId\") REFERENCES \"ProductModifiers\"(\"Id\", \"TenantId\") ON DELETE CASCADE);");

            sb.AppendLine("-- 5. Órdenes, Ítems y Pagos");
            sb.AppendLine("CREATE TABLE IF NOT EXISTS \"Orders\" (\"Id\" SERIAL PRIMARY KEY, \"OrderDate\" TIMESTAMP NOT NULL, \"CustomerName\" TEXT, \"SubTotal\" NUMERIC(10,2) NOT NULL CHECK (\"SubTotal\" >= 0.00), \"TaxAmount\" NUMERIC(10,2) NOT NULL CHECK (\"TaxAmount\" >= 0.00), \"TotalAmount\" NUMERIC(10,2) NOT NULL CHECK (\"TotalAmount\" >= 0.00), \"IsSynced\" BOOLEAN NOT NULL DEFAULT FALSE, \"IsReturned\" BOOLEAN NOT NULL DEFAULT FALSE, \"ReturnReason\" TEXT, \"AuthorizedBy\" TEXT, \"TenantId\" TEXT NOT NULL REFERENCES \"Tenants\"(\"Id\") ON DELETE CASCADE, \"ClientSideId\" TEXT, \"TaxId\" TEXT, \"CfdiUse\" TEXT, \"PaymentForm\" TEXT, \"PaymentMethod\" TEXT, \"CustomAttributes\" JSONB, \"CreatedBy\" TEXT, \"LastUpdated\" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP, \"RowVersion\" INTEGER NOT NULL DEFAULT 1, CONSTRAINT \"PK_Orders_Tenant_Id\" UNIQUE (\"Id\", \"TenantId\"));");
            sb.AppendLine("CREATE TABLE IF NOT EXISTS \"OrderItems\" (\"Id\" SERIAL PRIMARY KEY, \"OrderId\" INTEGER NOT NULL, \"ProductId\" INTEGER NOT NULL, \"ProductBarcode\" TEXT, \"Quantity\" INTEGER NOT NULL CHECK (\"Quantity\" > 0), \"UnitPrice\" NUMERIC(10,2) NOT NULL CHECK (\"UnitPrice\" >= 0.00), \"Discount\" NUMERIC(10,2) NOT NULL DEFAULT 0.00 CHECK (\"Discount\" >= 0.00), \"Notes\" TEXT, \"TenantId\" TEXT NOT NULL REFERENCES \"Tenants\"(\"Id\") ON DELETE CASCADE, \"CustomAttributes\" JSONB, \"LastUpdated\" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP, CONSTRAINT \"PK_OrderItems_Tenant_Id\" UNIQUE (\"Id\", \"TenantId\"), CONSTRAINT \"FK_OrderItems_Order\" FOREIGN KEY (\"OrderId\", \"TenantId\") REFERENCES \"Orders\"(\"Id\", \"TenantId\") ON DELETE CASCADE, CONSTRAINT \"FK_OrderItems_Product\" FOREIGN KEY (\"ProductId\", \"TenantId\") REFERENCES \"Products\"(\"Id\", \"TenantId\"));");
            sb.AppendLine("CREATE TABLE IF NOT EXISTS \"OrderItemModifiers\" (\"Id\" SERIAL PRIMARY KEY, \"OrderItemId\" INTEGER NOT NULL, \"ModifierOptionId\" INTEGER NOT NULL, \"OptionName\" TEXT NOT NULL, \"PriceAdjustment\" NUMERIC(10,2) NOT NULL DEFAULT 0.00 CHECK (\"PriceAdjustment\" >= 0.00), \"TenantId\" TEXT NOT NULL REFERENCES \"Tenants\"(\"Id\") ON DELETE CASCADE, CONSTRAINT \"FK_OrderItemModifiers_Item\" FOREIGN KEY (\"OrderItemId\", \"TenantId\") REFERENCES \"OrderItems\"(\"Id\", \"TenantId\") ON DELETE CASCADE, CONSTRAINT \"FK_OrderItemModifiers_Option\" FOREIGN KEY (\"ModifierOptionId\", \"TenantId\") REFERENCES \"ModifierOptions\"(\"Id\", \"TenantId\"));");
            sb.AppendLine("CREATE TABLE IF NOT EXISTS \"Payments\" (\"Id\" SERIAL PRIMARY KEY, \"OrderId\" INTEGER NOT NULL, \"TenantId\" TEXT NOT NULL REFERENCES \"Tenants\"(\"Id\") ON DELETE CASCADE, \"PaymentMethod\" TEXT NOT NULL, \"Amount\" NUMERIC(10,2) NOT NULL CHECK (\"Amount\" > 0.00), \"ReferenceNumber\" TEXT, \"ChangeGiven\" NUMERIC(10,2) NOT NULL DEFAULT 0.00 CHECK (\"ChangeGiven\" >= 0.00 AND \"ChangeGiven\" <= \"Amount\"), \"PaymentDate\" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP, \"CreatedBy\" TEXT, CONSTRAINT \"FK_Payments_Order\" FOREIGN KEY (\"OrderId\", \"TenantId\") REFERENCES \"Orders\"(\"Id\", \"TenantId\") ON DELETE CASCADE);");

            sb.AppendLine("-- CashShifts (Turnos de Caja)");
            sb.AppendLine("CREATE TABLE IF NOT EXISTS \"CashShifts\" (\"Id\" SERIAL PRIMARY KEY, \"TenantId\" TEXT NOT NULL REFERENCES \"Tenants\"(\"Id\") ON DELETE CASCADE, \"UserId\" INTEGER NOT NULL REFERENCES \"Users\"(\"Id\"), \"InitialCash\" NUMERIC(10,2) NOT NULL CHECK (\"InitialCash\" >= 0.00), \"FinalCash\" NUMERIC(10,2), \"ExpectedCash\" NUMERIC(10,2), \"Difference\" NUMERIC(10,2), \"Status\" TEXT NOT NULL DEFAULT 'OPEN', \"OpenedAt\" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP, \"ClosedAt\" TIMESTAMP, CONSTRAINT \"PK_CashShifts_Tenant_Id\" UNIQUE (\"Id\", \"TenantId\"));");

            sb.AppendLine("-- 6. Triggers de Concurrencia Optimista");
            sb.AppendLine("CREATE OR REPLACE TRIGGER trg_products_version BEFORE UPDATE ON \"Products\" FOR EACH ROW EXECUTE FUNCTION increment_row_version();");
            sb.AppendLine("CREATE OR REPLACE TRIGGER trg_orders_version BEFORE UPDATE ON \"Orders\" FOR EACH ROW EXECUTE FUNCTION increment_row_version();");
            sb.AppendLine("CREATE OR REPLACE TRIGGER trg_users_version BEFORE UPDATE ON \"Users\" FOR EACH ROW EXECUTE FUNCTION increment_row_version();");

            sb.AppendLine("-- 7. Índices");
            sb.AppendLine("CREATE UNIQUE INDEX IF NOT EXISTS \"IX_Users_Username_TenantId\" ON \"Users\" (\"Username\", \"TenantId\");");
            sb.AppendLine("CREATE INDEX IF NOT EXISTS \"IX_Users_TenantId\" ON \"Users\" (\"TenantId\");");
            sb.AppendLine("CREATE UNIQUE INDEX IF NOT EXISTS \"IX_Products_TenantId_Barcode\" ON \"Products\" (\"TenantId\", \"Barcode\");");
            sb.AppendLine("CREATE INDEX IF NOT EXISTS \"IX_Products_TenantId\" ON \"Products\" (\"TenantId\");");
            sb.AppendLine("CREATE UNIQUE INDEX IF NOT EXISTS \"IX_Orders_TenantId_ClientSideId\" ON \"Orders\" (\"TenantId\", \"ClientSideId\") WHERE \"ClientSideId\" IS NOT NULL;");
            sb.AppendLine("CREATE INDEX IF NOT EXISTS \"IX_Orders_TenantId_OrderDate\" ON \"Orders\" (\"TenantId\", \"OrderDate\");");
            sb.AppendLine("CREATE INDEX IF NOT EXISTS \"IX_OrderItems_OrderId\" ON \"OrderItems\" (\"OrderId\");");
            sb.AppendLine("CREATE INDEX IF NOT EXISTS \"IX_OrderItems_TenantId\" ON \"OrderItems\" (\"TenantId\");");
            sb.AppendLine("CREATE INDEX IF NOT EXISTS \"IX_OrderItemModifiers_OrderItemId\" ON \"OrderItemModifiers\" (\"OrderItemId\");");
            sb.AppendLine("CREATE INDEX IF NOT EXISTS \"IX_StockMovements_TenantId_ProductId\" ON \"StockMovements\" (\"TenantId\", \"ProductId\");");
            sb.AppendLine("CREATE INDEX IF NOT EXISTS \"IX_Payments_TenantId_OrderId\" ON \"Payments\" (\"TenantId\", \"OrderId\");");
            sb.AppendLine("CREATE INDEX IF NOT EXISTS \"IX_Payments_OrderId\" ON \"Payments\" (\"OrderId\");");
            sb.AppendLine("CREATE INDEX IF NOT EXISTS \"IX_CashShifts_TenantId\" ON \"CashShifts\" (\"TenantId\");");

            sb.AppendLine("-- 8. Seed Inicial");
            sb.AppendLine($"INSERT INTO \"Tenants\" (\"Id\", \"Name\", \"IsActive\") VALUES ('{model.TenantId}', '{model.CompanyName}', true) ON CONFLICT DO NOTHING;");
            sb.AppendLine($"INSERT INTO \"TenantProfiles\" (\"TenantId\", \"BusinessType\", \"ConfigJson\") VALUES ('{model.TenantId}', '{model.BusinessType}', '{{}}'::jsonb) ON CONFLICT DO NOTHING;");
            
            sb.AppendLine($"INSERT INTO \"Users\" (\"Username\", \"PasswordHash\", \"TenantId\", \"Role\") VALUES ('{model.AdminUser}', '{model.AdminPassword}', '{model.TenantId}', 'Administrador'), ('{model.EmployeeUser}', '{model.EmployeePassword}', '{model.TenantId}', 'Empleado') ON CONFLICT DO NOTHING;");
            
            if (model.ExtraUsers != null)
            {
                foreach (var user in model.ExtraUsers)
                {
                    sb.AppendLine($"INSERT INTO \"Users\" (\"Username\", \"PasswordHash\", \"TenantId\", \"Role\") VALUES ('{user.Username}', '{user.Password}', '{model.TenantId}', '{user.Role}') ON CONFLICT DO NOTHING;");
                }
            }

            return sb.ToString();
        }
    }
}
