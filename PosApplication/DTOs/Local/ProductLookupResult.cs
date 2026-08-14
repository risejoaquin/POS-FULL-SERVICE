using PosDomain.Entities;

namespace PosApplication.DTOs.Local
{
    public class ProductLookupResult
    {
        public bool Found { get; set; }
        public Product? Product { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
