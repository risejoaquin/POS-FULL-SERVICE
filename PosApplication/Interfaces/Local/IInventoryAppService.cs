using System.Collections.Generic;
using System.Threading.Tasks;
using PosDomain.Entities;

namespace PosApplication.Interfaces.Local
{
    public interface IInventoryAppService
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<IEnumerable<Supply>> GetAllSuppliesAsync();
        Task<Product> CreateProductAsync(Product product);
        Task UpdateProductAsync(Product product);
        Task DeleteProductAsync(int productId);
        Task<int> ImportProductsAsync(IEnumerable<Product> products);
        Task<IEnumerable<RecipeItem>> GetProductRecipeItemsAsync(int productId);
        Task<RecipeItem> AddRecipeItemAsync(int productId, int supplyId, decimal quantity);
        Task DeleteRecipeItemAsync(int recipeItemId);
        Task<IEnumerable<ProductModifier>> GetAllProductModifiersAsync();
        Task<IEnumerable<ProductModifier>> GetProductModifiersAsync(int productId);
        Task UpdateProductModifiersAsync(int productId, IEnumerable<int> modifierIds);
        Task<Supply> CreateSupplyAsync(Supply supply);
        Task UpdateSupplyAsync(Supply supply);
        Task DeleteSupplyAsync(int supplyId);
        Task AdjustStockAsync(int productId, decimal quantity, string reason);
        Task UpdateProductRecipeAsync(int productId, List<RecipeItem> recipeItems);
    }
}
