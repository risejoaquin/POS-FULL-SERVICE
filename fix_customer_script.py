import re

def fix_customer_leak():
    filepath = './PosCore/ViewModels/MainViewModel.cs'
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()
        
    # In SuspendOrder
    target = r'Cart\.Clear\(\);\s*IsDiscountApplied = false;\s*UpdateTotal\(\);'
    replacement = 'CustomerName = string.Empty;\n        Cart.Clear();\n        IsDiscountApplied = false;\n        UpdateTotal();'
    # We only want to replace it in SuspendOrder, but since CheckoutAsync already has CustomerName = string.Empty, let's be careful.
    
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)

# Actually, I'll just write a script that specifically patches SuspendOrder
