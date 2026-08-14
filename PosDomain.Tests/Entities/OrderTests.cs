using System;
using Xunit;
using PosDomain.Entities;
using PosDomain;

namespace PosDomain.Tests.Entities
{
    public class OrderTests
    {
        [Fact]
        public void AddItem_WhenOrderIsDraft_AddsItemAndReturnsSuccess()
        {
            var order = new Order { Status = OrderStatus.Draft, TenantId = "tenant1" };
            var product = new Product { Id = 1, Barcode = "123", Price = 100m, TenantId = "tenant1" };

            var result = order.AddItem(product, 2);

            Assert.True(result.IsSuccess);
            Assert.Single(order.Items);
            Assert.Equal(1, order.Items[0].ProductId);
            Assert.Equal(2, order.Items[0].Quantity);
        }

        [Fact]
        public void AddItem_WhenOrderIsClosed_ReturnsFailure()
        {
            var order = new Order { Status = OrderStatus.Closed };
            var product = new Product { Id = 1, Price = 100m };

            var result = order.AddItem(product, 1);

            Assert.False(result.IsSuccess);
            Assert.Empty(order.Items);
            Assert.Equal("Cannot add items to an order that is not open.", result.Error);
        }

        [Fact]
        public void AddItem_WhenQuantityIsZeroOrLess_ReturnsFailure()
        {
            var order = new Order { Status = OrderStatus.Draft };
            var product = new Product { Id = 1, Price = 100m };

            var result = order.AddItem(product, 0);

            Assert.False(result.IsSuccess);
            Assert.Equal("Quantity must be greater than zero.", result.Error);
        }

        [Fact]
        public void CalculateTotal_UpdatesSubTotalAndTotalAmount()
        {
            var order = new Order { Status = OrderStatus.Draft };
            var product1 = new Product { Id = 1, Price = 100m };
            var product2 = new Product { Id = 2, Price = 50m };

            order.AddItem(product1, 2);
            order.AddItem(product2, 1);

            Assert.Equal(250m, order.SubTotal);
            Assert.Equal(40m, order.TaxAmount); // Assuming 16% tax
            Assert.Equal(290m, order.TotalAmount);
        }

        [Fact]
        public void Complete_WhenItemsExist_ChangesStatusToClosed()
        {
            var order = new Order { Status = OrderStatus.Draft };
            var product = new Product { Id = 1, Price = 100m };
            order.AddItem(product, 1);

            var result = order.Complete("user1");

            Assert.True(result.IsSuccess);
            Assert.Equal(OrderStatus.Closed, order.Status);
            Assert.Equal("user1", order.AuthorizedBy);
        }

        [Fact]
        public void Complete_WhenNoItems_ReturnsFailure()
        {
            var order = new Order { Status = OrderStatus.Draft };

            var result = order.Complete("user1");

            Assert.False(result.IsSuccess);
            Assert.Equal(OrderStatus.Draft, order.Status); // Should remain draft
            Assert.Equal("Order must contain at least one item.", result.Error);
        }


        [Fact]
        public void AddPayment_WhenPaymentIsValid_AddsPaymentAndCopiesTenantId()
        {
            var order = new Order { TenantId = "tenant1" };
            var payment = new Payment { Amount = 100m, Method = "Efectivo" };

            var result = order.AddPayment(payment);

            Assert.True(result.IsSuccess);
            Assert.Single(order.Payments);
            Assert.Equal("tenant1", payment.TenantId);
        }

        [Fact]
        public void AddPayment_WhenAmountIsZero_ReturnsFailure()
        {
            var order = new Order();
            var payment = new Payment { Amount = 0m, Method = "Efectivo" };

            var result = order.AddPayment(payment);

            Assert.False(result.IsSuccess);
            Assert.Equal("Payment amount must be greater than zero.", result.Error);
            Assert.Empty(order.Payments);
        }

        [Fact]
        public void MarkRefunded_WhenReasonAndAuthorizedByAreValid_MarksOrderAsRefunded()
        {
            var order = new Order();

            var result = order.MarkRefunded("Cliente solicitó devolución", "manager");

            Assert.True(result.IsSuccess);
            Assert.True(order.IsReturned);
            Assert.True(order.IsRefunded);
            Assert.Equal(OrderStatus.Refunded, order.Status);
            Assert.Equal("Cliente solicitó devolución", order.ReturnReason);
            Assert.Equal("manager", order.AuthorizedBy);
        }

        [Fact]
        public void MarkRefunded_WhenReasonIsEmpty_ReturnsFailure()
        {
            var order = new Order();

            var result = order.MarkRefunded("", "manager");

            Assert.False(result.IsSuccess);
            Assert.Equal("Return reason is required.", result.Error);
        }
    }
}

public class OrderPaymentCashAlignmentTests
{
    [Fact]
    public void PaymentTotals_WhenPaymentsAreAdded_CalculatesCashCardAndBalance()
    {
        var order = new Order { TenantId = "tenant1", TotalAmount = 150m };

        order.AddPayment(new Payment { Amount = 100m, Method = Payment.CashMethod, Status = PaymentStatus.Completed });
        order.AddPayment(new Payment { Amount = 25m, Method = Payment.CardMethod, Status = PaymentStatus.Completed });

        Assert.Equal(125m, order.TotalPaid);
        Assert.Equal(100m, order.CashPaid);
        Assert.Equal(25m, order.CardPaid);
        Assert.Equal(25m, order.BalanceDue);
        Assert.False(order.IsFullyPaid);
    }

    [Fact]
    public void IsFullyPaid_WhenTotalPaidCoversTotal_ReturnsTrue()
    {
        var order = new Order { TenantId = "tenant1", TotalAmount = 150m };

        order.AddPayment(new Payment { Amount = 150m, Method = Payment.CashMethod, Status = PaymentStatus.Completed });

        Assert.True(order.IsFullyPaid);
        Assert.Equal(0m, order.BalanceDue);
    }

    [Fact]
    public void AddPayment_WhenPaymentFailed_ReturnsFailure()
    {
        var order = new Order { TenantId = "tenant1" };
        var payment = new Payment { Amount = 100m, Method = Payment.CashMethod, Status = PaymentStatus.Failed };

        var result = order.AddPayment(payment);

        Assert.False(result.IsSuccess);
        Assert.Equal("Failed payments cannot be added to an order.", result.Error);
        Assert.Empty(order.Payments);
    }

    [Fact]
    public void MarkRefunded_WhenAlreadyRefunded_ReturnsFailure()
    {
        var order = new Order { Status = OrderStatus.Refunded, IsReturned = true };

        var result = order.MarkRefunded("Duplicate refund", "manager");

        Assert.False(result.IsSuccess);
        Assert.Equal("Order is already refunded.", result.Error);
    }
}
