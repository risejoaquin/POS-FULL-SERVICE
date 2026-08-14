with open("PosCore/Services/ApiService.cs", "r") as f:
    text = f.read()

text = text.replace('        catch\n        {\n            return false;\n        }\n\n    public async Task<bool> SyncInventoryMovementAsync', '        catch\n        {\n            return false;\n        }\n    }\n\n    public async Task<bool> SyncInventoryMovementAsync')

text = text.replace('        catch\n        {\n            return false;\n        }\n    public async Task<bool> SyncInventoryMovementAsync', '        catch\n        {\n            return false;\n        }\n    }\n\n    public async Task<bool> SyncInventoryMovementAsync')


with open("PosCore/Services/ApiService.cs", "w") as f:
    f.write(text)
