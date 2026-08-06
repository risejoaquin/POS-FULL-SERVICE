import re

with open('./PosCore/ViewModels/MainViewModel.cs', 'r', encoding='utf-8') as f:
    content = f.read()

# Add _taxAmount
if "private decimal _taxAmount;" not in content:
    content = content.replace(
        "private decimal _total = 0m;\n",
        "private decimal _total = 0m;\n\n    [ObservableProperty]\n    private decimal _taxAmount = 0m;\n"
    )

# Fix CalculateTotals
old_calc = """    private void CalculateTotals()
    {
        SubTotal = Cart.Sum(i => i.SubTotal);
        if (IsDiscountApplied)
        {
            DiscountAmount = SubTotal * 0.10m;
        }
        else
        {
            DiscountAmount = 0;
        }
        Total = SubTotal - DiscountAmount;
    }"""

new_calc = """    private void CalculateTotals()
    {
        decimal rawTotal = Cart.Sum(i => i.SubTotal);
        if (IsDiscountApplied)
        {
            DiscountAmount = rawTotal * 0.10m;
        }
        else
        {
            DiscountAmount = 0;
        }
        Total = rawTotal - DiscountAmount;
        
        decimal taxRate = Settings?.Tax?.DefaultTaxRate ?? 0.16m;
        SubTotal = Total / (1 + taxRate);
        TaxAmount = Total - SubTotal;
    }"""

content = content.replace(old_calc, new_calc)

with open('./PosCore/ViewModels/MainViewModel.cs', 'w', encoding='utf-8') as f:
    f.write(content)

with open('./PosCore/Views/Controls/CartPanelControl.xaml', 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace(
    '<TextBlock Text="$0.00" FontSize="16" Foreground="{StaticResource TextPrimaryBrush}" HorizontalAlignment="Right" FontWeight="SemiBold"/>',
    '<TextBlock Text="{Binding TaxAmount, StringFormat=C}" FontSize="16" Foreground="{StaticResource TextPrimaryBrush}" HorizontalAlignment="Right" FontWeight="SemiBold"/>'
)

with open('./PosCore/Views/Controls/CartPanelControl.xaml', 'w', encoding='utf-8') as f:
    f.write(content)
