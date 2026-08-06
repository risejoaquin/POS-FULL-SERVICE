import json

sql = """
-- Cloud DB Schema for PosServer (PostgreSQL)

-- 1. Tablas Base
CREATE TABLE IF NOT EXISTS "Users" (
    "Id" SERIAL PRIMARY KEY,
    "Username" TEXT NOT NULL,
    "PasswordHash" TEXT NOT NULL,
    "TenantId" TEXT NOT NULL,
    "Role" TEXT NOT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS "Licenses" (
    "Id" SERIAL PRIMARY KEY,
    "LicenseKey" TEXT NOT NULL,
    "TenantId" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "MaxTerminals" INTEGER NOT NULL DEFAULT 1,
    "ValidUntil" TIMESTAMP NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "RowVersion" BYTEA
);

CREATE TABLE IF NOT EXISTS "Products" (
    "Id" SERIAL PRIMARY KEY,
    "Name" TEXT,
    "Barcode" TEXT,
    "Price" NUMERIC NOT NULL,
    "StockQuantity" INTEGER NOT NULL,
    "MinStockThreshold" INTEGER NOT NULL DEFAULT 10,
    "Category" TEXT,
    "LastUpdated" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "TenantId" TEXT,
    "CustomAttributes" JSONB
);

CREATE TABLE IF NOT EXISTS "ProductModifiers" (
    "Id" SERIAL PRIMARY KEY,
    "Name" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "IsRequired" BOOLEAN NOT NULL DEFAULT FALSE,
    "MinSelections" INTEGER NOT NULL DEFAULT 0,
    "MaxSelections" INTEGER NOT NULL DEFAULT 1,
    "TenantId" TEXT NOT NULL,
    "LastUpdated" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS "ModifierOptions" (
    "Id" SERIAL PRIMARY KEY,
    "ProductModifierId" INTEGER NOT NULL REFERENCES "ProductModifiers"("Id") ON DELETE CASCADE,
    "Name" TEXT NOT NULL,
    "PriceAdjustment" NUMERIC NOT NULL DEFAULT 0,
    "IsDefault" BOOLEAN NOT NULL DEFAULT FALSE,
    "SortOrder" INTEGER NOT NULL DEFAULT 0,
    "TenantId" TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS "ProductModifierLinks" (
    "Id" SERIAL PRIMARY KEY,
    "ProductId" INTEGER NOT NULL REFERENCES "Products"("Id") ON DELETE CASCADE,
    "ProductModifierId" INTEGER NOT NULL REFERENCES "ProductModifiers"("Id") ON DELETE CASCADE,
    "SortOrder" INTEGER NOT NULL DEFAULT 0,
    "TenantId" TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS "Orders" (
    "Id" SERIAL PRIMARY KEY,
    "OrderDate" TIMESTAMP NOT NULL,
    "CustomerName" TEXT,
    "SubTotal" NUMERIC NOT NULL,
    "TaxAmount" NUMERIC NOT NULL,
    "TotalAmount" NUMERIC NOT NULL,
    "IsSynced" BOOLEAN NOT NULL DEFAULT FALSE,
    "LastUpdated" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "IsReturned" BOOLEAN NOT NULL DEFAULT FALSE,
    "ReturnReason" TEXT,
    "AuthorizedBy" TEXT,
    "PaymentDetails" TEXT,
    "TenantId" TEXT,
    "ClientSideId" TEXT,
    "CustomAttributes" JSONB
);

CREATE TABLE IF NOT EXISTS "OrderItems" (
    "Id" SERIAL PRIMARY KEY,
    "OrderId" INTEGER NOT NULL REFERENCES "Orders"("Id") ON DELETE CASCADE,
    "ProductId" INTEGER NOT NULL,
    "ProductBarcode" TEXT,
    "Quantity" INTEGER NOT NULL,
    "UnitPrice" NUMERIC NOT NULL,
    "Discount" NUMERIC NOT NULL DEFAULT 0,
    "Notes" TEXT,
    "LastUpdated" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "TenantId" TEXT,
    "CustomAttributes" JSONB
);

-- Indices
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Users_Username_TenantId" ON "Users" ("Username", "TenantId");
CREATE INDEX IF NOT EXISTS "IX_Users_TenantId" ON "Users" ("TenantId");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Products_TenantId_Barcode" ON "Products" ("TenantId", "Barcode");
CREATE INDEX IF NOT EXISTS "IX_Products_TenantId" ON "Products" ("TenantId");
CREATE INDEX IF NOT EXISTS "IX_Orders_TenantId_OrderDate" ON "Orders" ("TenantId", "OrderDate");
CREATE INDEX IF NOT EXISTS "IX_OrderItems_OrderId" ON "OrderItems" ("OrderId");

-- Test Data (TenantId: 'TENANT_DEFAULT_123' - adjust if needed, but let's use 'TENANT-001' or similar depending on their config, wait, I'll just use 'TENANT-001')
-- We should clear existing test data to avoid conflicts, except Users.
DELETE FROM "ProductModifierLinks";
DELETE FROM "ModifierOptions";
DELETE FROM "ProductModifiers";
DELETE FROM "OrderItems";
DELETE FROM "Orders";
DELETE FROM "Products";

INSERT INTO "Products" ("Name", "Barcode", "Price", "StockQuantity", "MinStockThreshold", "Category", "TenantId", "CustomAttributes") VALUES
('Café Americano', '75010001', 35.00, 100, 10, 'Bebidas', 'TENANT-001', '{}'::jsonb),
('Capuchino', '75010002', 45.00, 50, 5, 'Bebidas', 'TENANT-001', '{}'::jsonb),
('Galleta de Chispas', '75010003', 15.00, 30, 10, 'Postres', 'TENANT-001', '{}'::jsonb),
('Taco al Pastor', '75010004', 20.00, 200, 20, 'Alimentos', 'TENANT-001', '{}'::jsonb),
('Refresco Cola 600ml', '75010005', 18.00, 100, 20, 'Bebidas', 'TENANT-001', '{}'::jsonb);

INSERT INTO "ProductModifiers" ("Name", "Description", "IsRequired", "MinSelections", "MaxSelections", "TenantId") VALUES
('Tipo de Leche', 'Selecciona el tipo de leche para tu bebida', true, 1, 1, 'TENANT-001'),
('Endulzante', 'Agrega endulzante', false, 0, 2, 'TENANT-001'),
('Extras Pastor', 'Con todo o sin algo', false, 0, 3, 'TENANT-001');

INSERT INTO "ModifierOptions" ("ProductModifierId", "Name", "PriceAdjustment", "IsDefault", "SortOrder", "TenantId") VALUES
(1, 'Entera', 0.00, true, 1, 'TENANT-001'),
(1, 'Deslactosada', 5.00, false, 2, 'TENANT-001'),
(1, 'Almendra', 10.00, false, 3, 'TENANT-001'),
(2, 'Azúcar', 0.00, true, 1, 'TENANT-001'),
(2, 'Splenda', 0.00, false, 2, 'TENANT-001'),
(3, 'Sin Cebolla', 0.00, false, 1, 'TENANT-001'),
(3, 'Sin Cilantro', 0.00, false, 2, 'TENANT-001'),
(3, 'Extra Queso', 10.00, false, 3, 'TENANT-001');

-- Linking Capuchino (ID 2) to Leche (ID 1) and Endulzante (ID 2)
INSERT INTO "ProductModifierLinks" ("ProductId", "ProductModifierId", "SortOrder", "TenantId") VALUES
(2, 1, 1, 'TENANT-001'),
(2, 2, 2, 'TENANT-001');

-- Linking Taco (ID 4) to Extras (ID 3)
INSERT INTO "ProductModifierLinks" ("ProductId", "ProductModifierId", "SortOrder", "TenantId") VALUES
(4, 3, 1, 'TENANT-001');

"""

with open('cloud_db_schema.sql', 'w') as f:
    f.write(sql)
