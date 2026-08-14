using PosApplication.DTOs.Local;

namespace PosApplication.Interfaces.Local;

public interface IReportsService
{
    Task<ReportsDataResult> GetReportsDataAsync(DateTime startDate, DateTime endDate);
}
