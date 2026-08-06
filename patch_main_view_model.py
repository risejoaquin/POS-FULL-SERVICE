import re

with open('./PosCore/ViewModels/MainViewModel.cs', 'r', encoding='utf-8') as f:
    content = f.read()

# Add SubTotal and Taxes properties if they don't exist
if 'public decimal SubTotal' not in content:
    prop_total = """    [ObservableProperty]
    private decimal _total;"""
    prop_subtotal_taxes = """    [ObservableProperty]
    private decimal _total;

    [ObservableProperty]
    private decimal _subTotal;

    [ObservableProperty]
    private decimal _taxes;"""
    content = content.replace(prop_total, prop_subtotal_taxes)

# In UpdateTotals, update SubTotal and Taxes
update_totals_old = """        Total = Cart.Sum(i => i.SubTotal);"""
update_totals_new = """        Total = Cart.Sum(i => i.SubTotal);
        decimal taxRate = Settings?.Tax?.DefaultTaxRate ?? 0.16m;
        SubTotal = Total / (1 + taxRate);
        Taxes = Total - SubTotal;"""
content = content.replace(update_totals_old, update_totals_new)

with open('./PosCore/ViewModels/MainViewModel.cs', 'w', encoding='utf-8') as f:
    f.write(content)
