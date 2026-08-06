import re

with open('./PosCore/ViewModels/InventoryViewModel.cs', 'r', encoding='utf-8') as f:
    content = f.read()

# Add properties
props = """    [ObservableProperty]
    private string _variantes = string.Empty;

    [ObservableProperty]
    private string _notasCocina = string.Empty;

    private void MapCustomAttributesToUI()
    {
        if (EditingProduct != null && EditingProduct.CustomAttributes != null)
        {
            if (EditingProduct.CustomAttributes.TryGetValue("Variantes", out var val1) && val1 != null)
                Variantes = val1.ToString();
            else
                Variantes = "";

            if (EditingProduct.CustomAttributes.TryGetValue("NotasCocina", out var val2) && val2 != null)
                NotasCocina = val2.ToString();
            else
                NotasCocina = "";
        }
        else
        {
            Variantes = "";
            NotasCocina = "";
        }
    }
"""
content = content.replace("    [ObservableProperty]\n    private string _errorMessage = string.Empty;", "    [ObservableProperty]\n    private string _errorMessage = string.Empty;\n" + props)

# Call mapping when EditingProduct changes
content = content.replace(
    "    private void EditProduct(Product product)\n    {\n        if (product == null) return;\n        IsEditing = true;\n        EditingProduct = product;\n    }",
    "    private void EditProduct(Product product)\n    {\n        if (product == null) return;\n        IsEditing = true;\n        EditingProduct = product;\n        MapCustomAttributesToUI();\n    }"
)

# And clear them on Add new
content = content.replace(
    "    private void AddProduct()\n    {\n        IsEditing = true;\n        EditingProduct = new Product();\n    }",
    "    private void AddProduct()\n    {\n        IsEditing = true;\n        EditingProduct = new Product();\n        MapCustomAttributesToUI();\n    }"
)

# Save to CustomAttributes in SaveProduct
save_logic = """        try
        {
            if (EditingProduct.CustomAttributes == null) EditingProduct.CustomAttributes = new System.Collections.Generic.Dictionary<string, object>();
            EditingProduct.CustomAttributes["Variantes"] = Variantes;
            EditingProduct.CustomAttributes["NotasCocina"] = NotasCocina;
            
            if (EditingProduct.Id == 0)"""
content = content.replace(
    "        try\n        {\n            if (EditingProduct.Id == 0)",
    save_logic
)

with open('./PosCore/ViewModels/InventoryViewModel.cs', 'w', encoding='utf-8') as f:
    f.write(content)
