using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PosDomain.Entities;
using PosInfrastructure.Data.Local;

namespace PosInfrastructure.Services.Local
{
    public class ShiftManagementService
    {
        private readonly PosDbContext _dbContext;

        public ShiftManagementService(PosDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<CashRegisterShift> OpenShiftAsync(decimal startingCash, string openedBy, string tenantId)
        {
            var activeShift = await _dbContext.CashRegisterShifts
                .Where(s => s.TenantId == tenantId && !s.IsClosed)
                .FirstOrDefaultAsync();

            if (activeShift != null)
                throw new InvalidOperationException("Ya existe un turno abierto.");

            var shift = new CashRegisterShift
            {
                StartingCash = startingCash,
                OpenedBy = openedBy,
                OpenedAt = DateTime.Now,
                TenantId = tenantId,
                IsClosed = false
            };

            _dbContext.CashRegisterShifts.Add(shift);
            await _dbContext.SaveChangesAsync();

            return shift;
        }

        public async Task<CashMovement> RegisterMovementAsync(int shiftId, string type, decimal amount, string reason, string createdBy, string tenantId)
        {
            var shift = await _dbContext.CashRegisterShifts
                .Where(s => s.Id == shiftId && s.TenantId == tenantId)
                .FirstOrDefaultAsync();

            if (shift == null)
                throw new InvalidOperationException("Turno no encontrado.");

            if (shift.IsClosed)
                throw new InvalidOperationException("No se pueden registrar movimientos en un turno cerrado.");

            var movement = new CashMovement
            {
                ShiftId = shiftId,
                Type = type,
                Amount = amount,
                Reason = reason,
                CreatedBy = createdBy,
                CreatedAt = DateTime.Now,
                TenantId = tenantId
            };

            _dbContext.CashMovements.Add(movement);
            await _dbContext.SaveChangesAsync();

            return movement;
        }

        public async Task CloseShiftAsync(int shiftId, decimal actualEndingCash, string closedBy, string tenantId)
        {
            var shift = await _dbContext.CashRegisterShifts
                .Where(s => s.Id == shiftId && s.TenantId == tenantId)
                .FirstOrDefaultAsync();

            if (shift == null)
                throw new InvalidOperationException("Turno no encontrado.");

            if (shift.IsClosed)
                throw new InvalidOperationException("El turno ya está cerrado.");

            var movements = await _dbContext.CashMovements
                .Where(m => m.ShiftId == shiftId && m.TenantId == tenantId)
                .ToListAsync();

            decimal totalIn = movements.Where(m => m.Type == "Entrada").Sum(m => m.Amount);
            decimal totalOut = movements.Where(m => m.Type == "Salida").Sum(m => m.Amount);
            
            shift.ExpectedEndingCash = shift.StartingCash + totalIn - totalOut;
            shift.ActualEndingCash = actualEndingCash;
            shift.Difference = actualEndingCash - shift.ExpectedEndingCash;
            shift.ClosedAt = DateTime.Now;
            shift.ClosedBy = closedBy;
            shift.IsClosed = true;

            await _dbContext.SaveChangesAsync();
        }
    }
}
