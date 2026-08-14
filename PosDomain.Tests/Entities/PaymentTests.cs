using PosDomain.Entities;
using Xunit;

namespace PosDomain.Tests.Entities;

public class PaymentTests
{
    [Fact]
    public void IsCash_WhenMethodIsEfectivo_ReturnsTrue()
    {
        var payment = new Payment { Method = "Efectivo" };

        Assert.True(payment.IsCash);
        Assert.False(payment.IsCard);
    }

    [Fact]
    public void Validate_WhenAmountIsZero_ReturnsFailure()
    {
        var payment = new Payment { OrderId = 1, Method = "Efectivo", Amount = 0m };

        var result = payment.Validate();

        Assert.False(result.IsSuccess);
        Assert.Equal("Payment amount must be greater than zero.", result.Error);
    }

    [Fact]
    public void Validate_WhenMethodIsEmpty_ReturnsFailure()
    {
        var payment = new Payment { OrderId = 1, Amount = 10m, Method = "" };

        var result = payment.Validate();

        Assert.False(result.IsSuccess);
        Assert.Equal("Payment method is required.", result.Error);
    }
}

public class PaymentAlignmentTests
{
    [Fact]
    public void SignedAmount_WhenPaymentIsCompleted_ReturnsPositiveAmount()
    {
        var payment = new Payment { Amount = 125m, Status = PaymentStatus.Completed };

        Assert.Equal(125m, payment.SignedAmount);
        Assert.True(payment.IsCompleted);
    }

    [Fact]
    public void SignedAmount_WhenPaymentIsRefunded_ReturnsNegativeAmount()
    {
        var payment = new Payment { Amount = 125m, Status = PaymentStatus.Refunded };

        Assert.Equal(-125m, payment.SignedAmount);
        Assert.True(payment.IsRefunded);
    }

    [Fact]
    public void MarkRefunded_WhenPaymentIsCompleted_ChangesStatus()
    {
        var payment = new Payment { Amount = 50m, Method = Payment.CashMethod, Status = PaymentStatus.Completed };

        var result = payment.MarkRefunded();

        Assert.True(result.IsSuccess);
        Assert.Equal(PaymentStatus.Refunded, payment.Status);
    }

    [Fact]
    public void MarkRefunded_WhenPaymentAlreadyRefunded_ReturnsFailure()
    {
        var payment = new Payment { Amount = 50m, Method = Payment.CashMethod, Status = PaymentStatus.Refunded };

        var result = payment.MarkRefunded();

        Assert.False(result.IsSuccess);
        Assert.Equal("Payment is already refunded.", result.Error);
    }
}
