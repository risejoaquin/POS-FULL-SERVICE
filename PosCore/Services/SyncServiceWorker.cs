using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using PosCore.Services;

namespace PosCore.Services
{
    public class SyncServiceWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SyncServiceWorker> _logger;

        public SyncServiceWorker(IServiceProvider serviceProvider, ILogger<SyncServiceWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SyncServiceWorker started.");
            
            using var periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(30));
            
            // Trigger an initial sync shortly after startup
            await Task.Delay(5000, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var syncService = scope.ServiceProvider.GetRequiredService<SyncService>();
                        await syncService.SyncNowAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in SyncWorker loop");
                }

                await periodicTimer.WaitForNextTickAsync(stoppingToken);
            }
        }
    }
}
