import re

with open('./PosCore/ViewModels/MainViewModel.cs', 'r', encoding='utf-8') as f:
    content = f.read()

old_add = """    private void AddToCart(Product product)
    {
        var existingItem = Cart.FirstOrDefault(i => i.ProductId == product.Id);
        
        int currentQuantity = existingItem?.Quantity ?? 0;
        if (product.StockQuantity <= currentQuantity)
        {
            _ = ShowNotification($"Stock insuficiente. Solo hay {product.StockQuantity} disponibles.", true);
            return;
        }

        if (existingItem != null)
        {
            existingItem.Quantity++;
            var index = Cart.IndexOf(existingItem);
            if (index >= 0) {
                Cart.RemoveAt(index);
                Cart.Insert(index, existingItem);
            }
        }
        else
        {
            var newItem = new OrderItem
            {
                ProductId = product.Id,
                Product = product,
                ProductBarcode = product.Barcode,
                Quantity = 1,
                UnitPrice = product.Price
            };
            Cart.Add(newItem);
        }
        UpdateTotals();
    }"""

new_add = """    private async void AddToCart(Product product)
    {
        int currentQuantity = Cart.Where(i => i.ProductId == product.Id).Sum(i => i.Quantity);
        if (product.StockQuantity <= currentQuantity)
        {
            _ = ShowNotification($"Stock insuficiente. Solo hay {product.StockQuantity} disponibles.", true);
            return;
        }

        // Check if product has modifiers
        var modifiers = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(
            DbContext.ProductModifierLinks
            .Include(l => l.ProductModifier)
            .ThenInclude(pm => pm.Options)
            .Where(l => l.ProductId == product.Id)
            .Select(l => l.ProductModifier)
        );

        decimal finalPrice = product.Price;
        List<object> selectedModifiers = null;

        if (modifiers != null && modifiers.Any())
        {
            // We need to show the UI in the main thread
            var result = await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var window = new PosCore.Views.ModifierSelectionWindow(product, modifiers);
                if (window.ShowDialog() == true)
                {
                    return new { Success = true, Price = window.FinalPrice, Modifiers = window.SelectedModifiers };
                }
                return new { Success = false, Price = 0m, Modifiers = (List<object>)null };
            });

            if (!result.Success) return; // User cancelled
            finalPrice = result.Price;
            selectedModifiers = result.Modifiers;
        }

        // If it has modifiers, we generally treat it as a separate item so we can track the specific modifiers
        if (selectedModifiers != null && selectedModifiers.Any())
        {
            var newItem = new OrderItem
            {
                ProductId = product.Id,
                Product = product,
                ProductBarcode = product.Barcode,
                Quantity = 1,
                UnitPrice = finalPrice
            };
            
            newItem.CustomAttributes["Modifiers"] = selectedModifiers;
            
            Cart.Add(newItem);
        }
        else
        {
            var existingItem = Cart.FirstOrDefault(i => i.ProductId == product.Id && !i.CustomAttributes.ContainsKey("Modifiers"));
            if (existingItem != null)
            {
                existingItem.Quantity++;
                var index = Cart.IndexOf(existingItem);
                if (index >= 0) {
                    Cart.RemoveAt(index);
                    Cart.Insert(index, existingItem);
                }
            }
            else
            {
                var newItem = new OrderItem
                {
                    ProductId = product.Id,
                    Product = product,
                    ProductBarcode = product.Barcode,
                    Quantity = 1,
                    UnitPrice = product.Price
                };
                Cart.Add(newItem);
            }
        }
        
        UpdateTotals();
    }"""

content = content.replace(old_add, new_add)

with open('./PosCore/ViewModels/MainViewModel.cs', 'w', encoding='utf-8') as f:
    f.write(content)
