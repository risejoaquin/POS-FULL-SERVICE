using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosDomain.Entities;
using PosApplication.Interfaces.Server;
using PosInfrastructure.Services.Server;
using PosDomain.Interfaces;

// PHASE 7D duplicate using cleanup applied: analyzer hygiene without business logic changes.
namespace PosServer.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 100)
        {
            var result = await _productService.GetProductsAsync(page, pageSize);
            return Ok(new { data = result.data, page = result.page, pageSize = result.pageSize, total = result.total });
        }

        [HttpGet("changes")]
        public async Task<IActionResult> GetChanges([FromQuery] string? since)
        {
            DateTime sinceDateTime = DateTime.MinValue;

            if (!string.IsNullOrWhiteSpace(since))
            {
                if (!DateTime.TryParse(since, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out sinceDateTime))
                {
                    sinceDateTime = DateTime.MinValue;
                }
            }

            var products = await _productService.GetChangesAsync(sinceDateTime);
            return Ok(products);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateOrUpdateProduct([FromBody] Product product)
        {
            var (isSuccess, message, resultProduct) = await _productService.CreateOrUpdateProductAsync(product);
            if (isSuccess)
            {
                return Ok(resultProduct);
            }
            else
            {
                if (message.StartsWith("Conflicto"))
                    return Conflict(new { Message = message, ServerVersion = resultProduct });
                return BadRequest(new { Message = message });
            }
        }

        [HttpDelete("{barcode}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(string barcode)
        {
            await _productService.DeleteProductAsync(barcode);
            return Ok();
        }
    }
}
