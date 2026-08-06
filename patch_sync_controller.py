import re

with open('./PosServer/Controllers/SyncController.cs', 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace(
'''        [HttpGet("changes")]
        public async Task<IActionResult> GetChanges([FromQuery] string? since)
        {
            var tenantId = _tenantService.GetTenantId();
            DateTime sinceDateTime = DateTime.MinValue;
            if (!string.IsNullOrEmpty(since) && DateTime.TryParse(since, out var parsed))
            {
                sinceDateTime = parsed.ToUniversalTime(); // assuming UTC in db
            }

            var products = await _context.Products.AsNoTracking().Where(p => p.TenantId == tenantId && p.LastUpdated >= sinceDateTime).ToListAsync();
            var users = await _context.Users.AsNoTracking().Where(u => u.TenantId == tenantId && u.LastUpdated >= sinceDateTime).ToListAsync();
            var shifts = await _context.CashRegisterShifts.Include(s => s.Movements).AsNoTracking().Where(s => s.TenantId == tenantId && s.LastUpdated >= sinceDateTime).ToListAsync();
            var orders = await _context.Orders.Include(o => o.Items).AsNoTracking().Where(o => o.TenantId == tenantId && o.LastUpdated >= sinceDateTime).ToListAsync();

            return Ok(new {
                products,
                users,
                shifts,
                orders
            });
        }''',
'''        [HttpGet("changes")]
        public async Task<IActionResult> GetChanges([FromQuery] string? since)
        {
            try
            {
                var tenantId = _tenantService.GetTenantId();
                DateTime sinceDateTime = DateTime.MinValue;
                if (!string.IsNullOrEmpty(since) && DateTime.TryParse(since, out var parsed))
                {
                    sinceDateTime = parsed.ToUniversalTime(); // assuming UTC in db
                }

                var products = await _context.Products.AsNoTracking().Where(p => p.TenantId == tenantId && p.LastUpdated >= sinceDateTime).ToListAsync();
                var users = await _context.Users.AsNoTracking().Where(u => u.TenantId == tenantId && u.LastUpdated >= sinceDateTime).ToListAsync();
                var shifts = await _context.CashRegisterShifts.Include(s => s.Movements).AsNoTracking().Where(s => s.TenantId == tenantId && s.LastUpdated >= sinceDateTime).ToListAsync();
                var orders = await _context.Orders.Include(o => o.Items).AsNoTracking().Where(o => o.TenantId == tenantId && o.LastUpdated >= sinceDateTime).ToListAsync();

                return Ok(new {
                    products,
                    users,
                    shifts,
                    orders
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, inner = ex.InnerException?.Message, stack = ex.StackTrace });
            }
        }''')

with open('./PosServer/Controllers/SyncController.cs', 'w', encoding='utf-8') as f:
    f.write(content)
