using PosDomain.Entities;
using Xunit;

namespace PosDomain.Tests.Entities;

public class CashMovementTests
{
    [Fact]
    public void SignedAmount_WhenMovementIsEntrada_ReturnsPositiveAmount()
    {
        var movement = new CashMovement { Type = CashMovement.InType, Amount = 100m };

        Assert.True(movement.IsCashIn);
        Assert.Equal(100m, movement.SignedAmount);
    }

    [Fact]
    public void SignedAmount_WhenMovementIsSalida_ReturnsNegativeAmount()
    {
        var movement = new CashMovement { Type = CashMovement.OutType, Amount = 100m };

        Assert.True(movement.IsCashOut);
        Assert.Equal(-100m, movement.SignedAmount);
    }

    [Fact]
    public void Validate_WhenAmountIsZero_ReturnsFailure()
    {
        var movement = new CashMovement { ShiftId = 1, Type = CashMovement.InType, Amount = 0m };

        var result = movement.Validate();

        Assert.False(result.IsSuccess);
        Assert.Equal("Amount must be greater than zero.", result.Error);
    }

    [Fact]
    public void Validate_WhenTypeIsInvalid_ReturnsFailure()
    {
        var movement = new CashMovement { ShiftId = 1, Type = "Unknown", Amount = 10m };

        var result = movement.Validate();

        Assert.False(result.IsSuccess);
        Assert.Equal("Cash movement type must be Entrada or Salida.", result.Error);
    }
}

public class CashMovementAlignmentTests
{
    [Fact]
    public void CashIn_Factory_CreatesValidEntradaMovement()
    {
        var movement = CashMovement.CashIn(1, 25m, "Fondo adicional", "cashier", "tenant1");

        Assert.True(movement.IsCashIn);
        Assert.Equal(25m, movement.SignedAmount);
        Assert.Equal("tenant1", movement.TenantId);
        Assert.True(movement.Validate().IsSuccess);
    }

    [Fact]
    public void CashOut_Factory_CreatesValidSalidaMovement()
    {
        var movement = CashMovement.CashOut(1, 10m, "Retiro", "cashier");

        Assert.True(movement.IsCashOut);
        Assert.Equal(-10m, movement.SignedAmount);
        Assert.True(movement.Validate().IsSuccess);
    }

    [Fact]
    public void Validate_WhenReasonIsEmpty_ReturnsFailure()
    {
        var movement = new CashMovement { ShiftId = 1, Type = CashMovement.InType, Amount = 10m, Reason = "" };

        var result = movement.Validate();

        Assert.False(result.IsSuccess);
        Assert.Equal("Reason is required.", result.Error);
    }
}
