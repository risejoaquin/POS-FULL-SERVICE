with open('./PosCore/ViewModels/InventoryViewModel.cs', 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace(
"""    private void GenerateBarcode()
    {
        if (EditingProduct != null)
        {
            EditingProduct.Barcode = "GEN-" + DateTime.Now.ToString("yyyyMMddHHmmss");
        }
    }""",
"""    private void GenerateBarcode()
    {
        if (EditingProduct != null)
        {
            EditingProduct.Barcode = "GEN-" + DateTime.Now.ToString("yyyyMMddHHmmss");
            OnPropertyChanged(nameof(EditingProduct));
        }
    }"""
)

with open('./PosCore/ViewModels/InventoryViewModel.cs', 'w', encoding='utf-8') as f:
    f.write(content)
