import re

with open('./PosCore/ViewModels/InventoryViewModel.cs', 'r', encoding='utf-8') as f:
    content = f.read()

new_command = """
    [RelayCommand]
    private void ConfigureModifiers()
    {
        if (EditingProduct == null || EditingProduct.Id == 0)
        {
            _ = _notificationService.ShowWarning("Guarde el producto primero antes de configurar modificadores.");
            return;
        }
        var window = new Views.ProductModifiersConfigWindow(EditingProduct, _dbContext);
        window.ShowDialog();
    }
"""

if 'ConfigureModifiers' not in content:
    content = content.replace('private void GenerateBarcode()', new_command + '\n    [RelayCommand]\n    private void GenerateBarcode()')

with open('./PosCore/ViewModels/InventoryViewModel.cs', 'w', encoding='utf-8') as f:
    f.write(content)
