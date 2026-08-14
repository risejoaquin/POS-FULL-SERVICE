using PosDomain.Interfaces;
using PosDomain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PosApplication.Interfaces.Server
{
    public interface IProductService
    {
        Task<(List<Product> data, int page, int pageSize, int total)> GetProductsAsync(int page, int pageSize);
        Task<List<Product>> GetChangesAsync(DateTime since);
        Task<(bool isSuccess, string message, Product product)> CreateOrUpdateProductAsync(Product product);
        Task<bool> DeleteProductAsync(string barcode);
    }
}
