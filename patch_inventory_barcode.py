import re

with open('./PosCore/ViewModels/InventoryViewModel.cs', 'r', encoding='utf-8') as f:
    content = f.read()

old_barcode = """    [RelayCommand]
    private void GenerateBarcode()
    {
        if (EditingProduct != null)
        {
            EditingProduct.Barcode = "GEN-" + DateTime.Now.ToString("yyyyMMddHHmmss");
            OnPropertyChanged(nameof(EditingProduct));
        }
    }"""

new_barcode = """    [RelayCommand]
    private void GenerateBarcode()
    {
        if (EditingProduct != null)
        {
            var random = new Random();
            EditingProduct.Barcode = "750" + random.Next(100000000, 999999999).ToString();
            OnPropertyChanged(nameof(EditingProduct));
        }
    }"""

content = content.replace(old_barcode, new_barcode)

with open('./PosCore/ViewModels/InventoryViewModel.cs', 'w', encoding='utf-8') as f:
    f.write(content)
