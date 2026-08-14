with open('PosCore/App.xaml.cs', 'r') as f:
    c = f.read()
c = c.replace("var syncService = ServiceProvider.GetRequiredService<SyncService>();\n            syncService.Start();", "var syncWorker = new SyncServiceWorker(ServiceProvider, ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<SyncServiceWorker>>());\n            _ = syncWorker.StartAsync(System.Threading.CancellationToken.None);")
with open('PosCore/App.xaml.cs', 'w') as f:
    f.write(c)
