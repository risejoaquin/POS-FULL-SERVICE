using System;
using PosDomain.ValueObjects;
using Xunit;

namespace PosDomain.Tests.ValueObjects
{
    public class MoneyTests
    {
        [Fact]
        public void Constructor_StoresAmountAsMinorUnits()
        {
            var money = new Money(123.45m, "mxn");

            Assert.Equal(12345, money.MinorUnits);
            Assert.Equal(123.45m, money.Amount);
            Assert.Equal("MXN", money.Currency);
        }

        [Fact]
        public void Constructor_RoundsToNearestCentAwayFromZero()
        {
            var money = new Money(10.005m);

            Assert.Equal(1001, money.MinorUnits);
            Assert.Equal(10.01m, money.Amount);
        }

        [Fact]
        public void FromMinorUnits_CreatesMoneyWithoutDecimalDrift()
        {
            var money = Money.FromMinorUnits(999, "MXN");

            Assert.Equal(999, money.MinorUnits);
            Assert.Equal(9.99m, money.Amount);
        }

        [Fact]
        public void Addition_WithSameCurrency_AddsMinorUnits()
        {
            var left = new Money(10.25m);
            var right = new Money(5.10m);

            var result = left + right;

            Assert.Equal(1535, result.MinorUnits);
            Assert.Equal(15.35m, result.Amount);
        }

        [Fact]
        public void Subtraction_WithSameCurrency_SubtractsMinorUnits()
        {
            var left = new Money(10.25m);
            var right = new Money(5.10m);

            var result = left - right;

            Assert.Equal(515, result.MinorUnits);
            Assert.Equal(5.15m, result.Amount);
        }

        [Fact]
        public void Multiplication_RoundsResultToMinorUnits()
        {
            var money = new Money(19.99m);

            var result = money * 0.16m;

            Assert.Equal(320, result.MinorUnits);
            Assert.Equal(3.20m, result.Amount);
        }

        [Fact]
        public void Operations_WithDifferentCurrencies_Throw()
        {
            var mxn = new Money(10m, "MXN");
            var usd = new Money(10m, "USD");

            Assert.Throws<InvalidOperationException>(() => mxn + usd);
            Assert.Throws<InvalidOperationException>(() => mxn - usd);
            Assert.Throws<InvalidOperationException>(() => mxn > usd);
        }

        [Fact]
        public void Constructor_WithBlankCurrency_Throws()
        {
            Assert.Throws<ArgumentException>(() => new Money(10m, " "));
        }
    }
}
