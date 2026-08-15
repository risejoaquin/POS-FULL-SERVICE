using PosDomain.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PosInfrastructure.Data.Server;
using PosDomain.Entities;

using PosApplication.Interfaces.Server;

namespace PosInfrastructure.Services.Server
{
    public class SyncService : ISyncService
    {
        private readonly CentralDbContext _context;

        public SyncService(CentralDbContext context)
        {
            _context = context;
        }

        public async Task<object> GetChangesAsync(string tenantId, DateTime sinceDateTime)
        {
            var products = await _context.Products.AsNoTracking().Where(p => p.TenantId == tenantId && p.LastUpdated >= sinceDateTime).ToListAsync();
            var users = await _context.Users.AsNoTracking().Where(u => u.TenantId == tenantId && u.LastUpdated >= sinceDateTime).ToListAsync();
            
            var shifts = await _context.CashRegisterShifts.Include(s => s.Movements).AsNoTracking().Where(s => s.TenantId == tenantId && s.LastUpdated >= sinceDateTime).ToListAsync();
            var orders = await _context.Orders.Include(o => o.Items).AsNoTracking().Where(o => o.TenantId == tenantId && o.LastUpdated >= sinceDateTime).ToListAsync();
            var supplies = await _context.Supplies.AsNoTracking().Where(s => s.TenantId == tenantId).ToListAsync();
            var productModifiers = await _context.ProductModifiers.Include(m => m.Options).AsNoTracking().Where(m => m.TenantId == tenantId).ToListAsync();
            
            var outboxMessages = await _context.OutboxMessages
                .AsNoTracking()
                .Where(m => m.TenantId == tenantId && m.CreatedAt >= sinceDateTime && m.ProcessedAt == null)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();

            return new {
                products,
                users,
                shifts,
                orders,
                supplies,
                productModifiers,
                outboxMessages
            };
        }

        public async Task<bool> ApplyChangesAsync(string tenantId, SyncPayload payload)
        {
            // Conflict Resolution Strategy: Last Write Wins (using LastUpdated / RowVersion comparison)
            
            if (payload.Products != null) {
                foreach (var product in payload.Products) {
                    var existing = await _context.Products.FirstOrDefaultAsync(p => p.Barcode == product.Barcode && p.TenantId == tenantId);
                    if (existing == null) {
                        product.Id = 0;
                        product.TenantId = tenantId;
                        _context.Products.Add(product);
                    } else if (product.LastUpdated > existing.LastUpdated) {
                        // Resolve conflict by updating existing
                        existing.Name = product.Name;
                        existing.Price = product.Price;
                        existing.Category = product.Category;
                        existing.LastUpdated = product.LastUpdated;
                    }
                }
            }

            // Similarly apply for orders
            if (payload.Orders != null) {
                foreach (var order in payload.Orders) {
                    var existing = await _context.Orders.FirstOrDefaultAsync(o => o.ClientSideId == order.ClientSideId && o.TenantId == tenantId);
                    if (existing == null) {
                        order.Id = 0;
                        order.TenantId = tenantId;
                        if (order.Items != null) foreach (var i in order.Items) { i.Id = 0; i.OrderId = 0; i.TenantId = tenantId; }
                        _context.Orders.Add(order);
                    } else if (order.LastUpdated > existing.LastUpdated) {
                        existing.Status = order.Status;
                        existing.IsReturned = order.IsReturned;
                        existing.ReturnReason = order.ReturnReason;
                        existing.TotalAmount = order.TotalAmount;
                        existing.LastUpdated = order.LastUpdated;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
