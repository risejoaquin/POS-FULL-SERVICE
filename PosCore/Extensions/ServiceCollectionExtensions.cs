using Microsoft.Extensions.DependencyInjection;
using PosApplication.Interfaces.Local;
using PosInfrastructure.Services.Local;

namespace PosCore.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddInventoryServices(this IServiceCollection services)
        {
            services.AddScoped<IInventoryService, InventoryService>();
            services.AddScoped<IInventoryAppService, InventoryAppService>();
            services.AddScoped<IInventoryDriftReportingService, InventoryDriftReportingService>();
            services.AddScoped<IAuditService, AuditService>();
            services.AddScoped<IProductLookupService, ProductLookupService>();
            services.AddScoped<IShiftService, ShiftService>();
            services.AddScoped<ILocalAuthService, LocalAuthService>();
            services.AddScoped<IUsersService, UsersService>();
            services.AddScoped<IReportsService, ReportsService>();
            services.AddScoped<IReturnsService, ReturnsService>();
            services.AddScoped<ILocalOrderService, LocalOrderService>();
        }
    }
}
