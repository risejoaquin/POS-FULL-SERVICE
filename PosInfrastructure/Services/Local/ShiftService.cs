using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PosDomain.Entities;
using PosApplication.Interfaces.Local;
using PosApplication.DTOs.Local;
using PosInfrastructure.Data.Local;

namespace PosInfrastructure.Services.Local
{
    public class ShiftService : IShiftService
    {
        private readonly PosDbContext _dbContext;

        public ShiftService(PosDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<CashRegisterShift?> GetActiveShiftAsync()
        {
            return await _dbContext.CashRegisterShifts
                .Include(s => s.Movements)
                .FirstOrDefaultAsync(s => !s.IsClosed);
        }

        public async Task<CashRegisterShift> OpenShiftAsync(string openedBy, decimal startingCash)
        {
            var shift = new CashRegisterShift
            {
                OpenedAt = DateTime.Now,
                OpenedBy = openedBy,
                StartingCash = startingCash,
                IsClosed = false
            };

            _dbContext.CashRegisterShifts.Add(shift);
            await _dbContext.SaveChangesAsync();
            return shift;
        }

        public async Task<CashRegisterShift> CloseShiftAsync(int shiftId, string closedBy, decimal actualEndingCash, decimal expectedEndingCash, decimal difference)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var shift = await _dbContext.CashRegisterShifts
                    .Include(s => s.Movements)
                    .FirstOrDefaultAsync(s => s.Id == shiftId);

                if (shift == null)
                {
                    throw new KeyNotFoundException($"Turno con ID {shiftId} no encontrado.");
                }

                shift.ClosedAt = DateTime.Now;
                shift.ClosedBy = closedBy;
                shift.ExpectedEndingCash = expectedEndingCash;
                shift.ActualEndingCash = actualEndingCash;
                shift.Difference = difference;
                shift.IsClosed = true;

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return shift;
            }
            catch
            {
                await transaction.RollbackAsync();
                _dbContext.ChangeTracker.Clear();
                throw;
            }
        }

        public async Task<CashMovement> RegisterCashMovementAsync(int shiftId, decimal amount, string type, string reason, string createdBy, string tenantId)
        {
            var shift = await _dbContext.CashRegisterShifts.FindAsync(shiftId);
            if (shift == null)
            {
                throw new KeyNotFoundException($"Turno con ID {shiftId} no encontrado.");
            }

            var movement = new CashMovement
            {
                ShiftId = shiftId,
                Amount = amount,
                Type = type,
                Reason = reason,
                CreatedAt = DateTime.Now,
                CreatedBy = createdBy,
                TenantId = tenantId
            };

            _dbContext.CashMovements.Add(movement);
            
            shift.Movements ??= new List<CashMovement>();
            shift.Movements.Add(movement);

            await _dbContext.SaveChangesAsync();
            return movement;
        }

        public async Task<IEnumerable<CashMovement>> GetCashMovementsAsync(int shiftId)
        {
            return await _dbContext.CashMovements
                .Where(c => c.ShiftId == shiftId)
                .ToListAsync();
        }

        public async Task<ShiftSummaryResult> GetShiftSummaryAsync(int shiftId)
        {
             var shift = await _dbContext.CashRegisterShifts
                 .Include(s => s.Movements)
                 .FirstOrDefaultAsync(s => s.Id == shiftId);

             if (shift == null) return new ShiftSummaryResult();

             var endPeriod = shift.ClosedAt ?? DateTime.Now;
             
             var orders = await _dbContext.Orders
                 .Where(o => o.OrderDate >= shift.OpenedAt && o.OrderDate <= endPeriod)
                 .ToListAsync();

             decimal cashSales = 0;
             decimal cardSales = 0;
             decimal totalTaxes = 0;
             
             int annulledCount = orders.Count(o => o.IsReturned);
             decimal annulledTotal = orders.Where(o => o.IsReturned).Sum(o => o.TotalAmount);

             foreach (var o in orders.Where(o => !o.IsReturned))
             {
                 totalTaxes += o.TaxAmount;
                 var payments = o.PaymentDetails.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                 foreach (var p in payments)
                 {
                     if (p.StartsWith("Efectivo: "))
                     {
                         if (decimal.TryParse(p.Substring("Efectivo: ".Length).Split(' ')[0], System.Globalization.NumberStyles.Currency, null, out decimal amount))
                             cashSales += amount;
                     }
                     else if (p.StartsWith("Tarjeta: "))
                     {
                         if (decimal.TryParse(p.Substring("Tarjeta: ".Length).Split(' ')[0], System.Globalization.NumberStyles.Currency, null, out decimal amount))
                             cardSales += amount;
                     }
                 }
             }

             var movements = await _dbContext.CashMovements.Where(c => c.ShiftId == shift.Id).ToListAsync();
             decimal movesIn = movements.Where(m => m.Amount > 0).Sum(m => m.Amount);
             decimal movesOut = Math.Abs(movements.Where(m => m.Amount < 0).Sum(m => m.Amount));

             decimal expectedEndingCash = shift.StartingCash + cashSales + movements.Sum(m => m.Amount);

             return new ShiftSummaryResult
             {
                 ShiftId = shift.Id,
                 OpenedAt = shift.OpenedAt,
                 StartingCash = shift.StartingCash,
                 TotalSales = cashSales + cardSales,
                 TotalInflows = movesIn,
                 TotalOutflows = movesOut,
                 ExpectedCash = expectedEndingCash,
                 Movements = movements,

                 CashSales = cashSales,
                 CardSales = cardSales,
                 TotalTaxes = totalTaxes,
                 CashIn = movesIn,
                 CashOut = movesOut,
                 ExpectedEndingCash = expectedEndingCash,
                 AnnulledCount = annulledCount,
                 AnnulledTotal = annulledTotal,
                 ActiveShift = shift
             };
        }
    }
}
