DO $$ 
DECLARE
    t_name text;
BEGIN
    FOR t_name IN (SELECT tablename FROM pg_tables WHERE schemaname = 'public' AND tablename IN ('Users', 'Products', 'Orders', 'OrderItems', 'Payments', 'InventoryMovements', 'Supplies', 'RecipeItems', 'CashRegisterShifts', 'CashMovements', 'Licenses', 'AuditLogs', 'OutboxMessages')) 
    LOOP
        EXECUTE format('ALTER TABLE "%I" ENABLE ROW LEVEL SECURITY;', t_name);
        
        -- Create policy for tenant isolation
        EXECUTE format('
            DO $policy$
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM pg_policies WHERE schemaname = ''public'' AND tablename = ''%I'' AND policyname = ''tenant_isolation_policy'') THEN
                    CREATE POLICY tenant_isolation_policy ON "%I"
                    AS RESTRICTIVE
                    FOR ALL
                    TO public
                    USING ("TenantId" = current_setting(''app.current_tenant'', true));
                END IF;
            END
            $policy$;
        ', t_name, t_name);
        
        -- Ensure RLS is active even for the table owner in typical application connections
        EXECUTE format('ALTER TABLE "%I" FORCE ROW LEVEL SECURITY;', t_name);
    END LOOP;
END $$;
