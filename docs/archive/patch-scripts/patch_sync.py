with open('PosCore/Services/SyncService.cs', 'r') as f:
    c = f.read()
c = c.replace("if (_isSyncing) return;\n        _isSyncing = true;\n\n        try", "if (!await _syncLock.WaitAsync(0)) return;\n\n        try")
c = c.replace("_isSyncing = false;", "_syncLock.Release();")
with open('PosCore/Services/SyncService.cs', 'w') as f:
    f.write(c)
