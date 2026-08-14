using Xunit;
using PosDomain.Entities;

namespace PosDomain.Tests.Entities
{
    public class OrderItemTests
    {
        [Fact]
        public void SubTotal_CalculatesCorrectly()
        {
            var item = new OrderItem
            {
                Quantity = 2,
                UnitPrice = 100m,
                Discount = 20m
            };

            Assert.Equal(180m, item.SubTotal);
        }

        [Fact]
        public void HasDiscount_WhenDiscountGreaterThanZero_ReturnsTrue()
        {
            var item = new OrderItem { Discount = 10m };
            Assert.True(item.HasDiscount);
        }

        [Fact]
        public void HasDiscount_WhenDiscountIsZero_ReturnsFalse()
        {
            var item = new OrderItem { Discount = 0m };
            Assert.False(item.HasDiscount);
        }
        
        [Fact]
        public void HasNotes_WhenNotesArePresent_ReturnsTrue()
        {
            var item = new OrderItem { Notes = "Sin cebolla" };
            Assert.True(item.HasNotes);
        }
        
        [Fact]
        public void HasNotes_WhenNotesAreEmpty_ReturnsFalse()
        {
            var item = new OrderItem { Notes = "" };
            Assert.False(item.HasNotes);
        }
    }
}
