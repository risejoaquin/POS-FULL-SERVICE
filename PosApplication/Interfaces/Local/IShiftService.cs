using System.Collections.Generic;
using System.Threading.Tasks;
using PosDomain.Entities;
using PosApplication.DTOs.Local;

namespace PosApplication.Interfaces.Local
{
    public interface IShiftService
    {
        Task<CashRegisterShift?> GetActiveShiftAsync();
        Task<CashRegisterShift> OpenShiftAsync(string openedBy, decimal startingCash);
        Task<CashRegisterShift> CloseShiftAsync(int shiftId, string closedBy, decimal actualEndingCash, decimal expectedEndingCash, decimal difference);
        Task<CashMovement> RegisterCashMovementAsync(int shiftId, decimal amount, string type, string reason, string createdBy, string tenantId);
        Task<IEnumerable<CashMovement>> GetCashMovementsAsync(int shiftId);
        Task<ShiftSummaryResult> GetShiftSummaryAsync(int shiftId);
    }
}
