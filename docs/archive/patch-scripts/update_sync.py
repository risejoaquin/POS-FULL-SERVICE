import sys

with open('PosCore/Services/SyncService.cs', 'r') as f:
    content = f.read()

content = content.replace("private readonly System.Timers.Timer _timer;", "")
content = content.replace("private readonly System.Timers.Timer _pingTimer;", "")
content = content.replace("""    public SyncService(IServiceProvider serviceProvider, ILogger<SyncService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        
        // Configurar timer para ejecutar cada 10 segundos
        _timer = new System.Timers.Timer(10000);
        _timer.Elapsed += async (sender, e) => { try { await SyncDataAsync(); } catch (Exception ex) { _logger.LogError(ex, "Sync failed"); } };
        
        _pingTimer = new System.Timers.Timer(300000); // 5 minutes
        _pingTimer.Elapsed += async (sender, e) => { try { await SendPingAsync(); } catch { } };
    }""", """    private readonly System.Threading.SemaphoreSlim _syncLock = new System.Threading.SemaphoreSlim(1, 1);
    
    public SyncService(IServiceProvider serviceProvider, ILogger<SyncService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    
    public async Task SyncNowAsync()
    {
        await SyncDataAsync();
    }""")

content = content.replace("""    public void Start()
    {
        _timer.Start();
        _pingTimer.Start();
        Task.Run(async () => await SyncDataAsync());
    }

    public void Stop()
    {
        _timer.Stop();
        _pingTimer.Stop();
    }""", "")

with open('PosCore/Services/SyncService.cs', 'w') as f:
    f.write(content)
