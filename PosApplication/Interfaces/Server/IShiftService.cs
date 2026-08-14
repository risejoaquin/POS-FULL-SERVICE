using PosDomain.Interfaces;
using PosDomain.Entities;
using System.Threading.Tasks;

namespace PosApplication.Interfaces.Server
{
    public interface IShiftService
    {
        Task<(bool isSuccess, string message, CashRegisterShift shift)> SyncShiftAsync(CashRegisterShift shift);
    }
}
