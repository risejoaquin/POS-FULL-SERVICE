using PosDomain.Entities;
using Xunit;

namespace PosDomain.Tests.Entities;

public class CashRegisterShiftTests
{
    [Fact]
    public void IsOpen_WhenNotClosedAndClosedAtIsNull_ReturnsTrue()
    {
        var shift = new CashRegisterShift { IsClosed = false, ClosedAt = null };

        Assert.True(shift.IsOpen);
    }

    [Fact]
    public void Close_WhenShiftIsOpen_SetsClosingValues()
    {
        var shift = new CashRegisterShift { IsClosed = false, ClosedAt = null };

        var result = shift.Close(100m, 95m, "manager");

        Assert.True(result.IsSuccess);
        Assert.True(shift.IsClosed);
        Assert.NotNull(shift.ClosedAt);
        Assert.Equal(100m, shift.ExpectedEndingCash);
        Assert.Equal(95m, shift.ActualEndingCash);
        Assert.Equal(-5m, shift.Difference);
        Assert.Equal("manager", shift.ClosedBy);
    }

    [Fact]
    public void Close_WhenShiftIsClosed_ReturnsFailure()
    {
        var shift = new CashRegisterShift { IsClosed = true, ClosedAt = DateTime.UtcNow };

        var result = shift.Close(100m, 100m, "manager");

        Assert.False(result.IsSuccess);
        Assert.Equal("Shift is already closed.", result.Error);
    }

    [Fact]
    public void AddMovement_WhenShiftIsClosed_ReturnsFailure()
    {
        var shift = new CashRegisterShift { IsClosed = true, ClosedAt = DateTime.UtcNow };
        var movement = new CashMovement { ShiftId = 1, Type = CashMovement.InType, Amount = 10m };

        var result = shift.AddMovement(movement);

        Assert.False(result.IsSuccess);
        Assert.Equal("Cannot add cash movements to a closed shift.", result.Error);
    }
}
