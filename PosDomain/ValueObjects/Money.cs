using System;
using System.Globalization;

namespace PosDomain.ValueObjects
{
    /// <summary>
    /// Represents a monetary amount using integer minor units internally.
    /// For MXN, minor units are cents.
    /// </summary>
    public sealed class Money : IEquatable<Money>, IComparable<Money>
    {
        private const int MinorUnitsPerMajorUnit = 100;

        public long MinorUnits { get; }
        public string Currency { get; }

        public decimal Amount => MinorUnits / (decimal)MinorUnitsPerMajorUnit;

        public Money(decimal amount, string currency = "MXN")
            : this(ToMinorUnits(amount), currency, alreadyMinorUnits: true)
        {
        }

        private Money(long minorUnits, string currency, bool alreadyMinorUnits)
        {
            if (string.IsNullOrWhiteSpace(currency))
            {
                throw new ArgumentException("Currency is required.", nameof(currency));
            }

            MinorUnits = alreadyMinorUnits ? minorUnits : ToMinorUnits(minorUnits);
            Currency = currency.Trim().ToUpperInvariant();
        }

        public static Money Zero(string currency = "MXN") => FromMinorUnits(0, currency);

        public static Money FromMinorUnits(long minorUnits, string currency = "MXN")
            => new Money(minorUnits, currency, alreadyMinorUnits: true);

        public static Money FromDecimal(decimal amount, string currency = "MXN")
            => new Money(amount, currency);

        public Money Add(Money other) => this + other;

        public Money Subtract(Money other) => this - other;

        public static Money operator +(Money left, Money right)
        {
            EnsureSameCurrency(left, right);
            return FromMinorUnits(left.MinorUnits + right.MinorUnits, left.Currency);
        }

        public static Money operator -(Money left, Money right)
        {
            EnsureSameCurrency(left, right);
            return FromMinorUnits(left.MinorUnits - right.MinorUnits, left.Currency);
        }

        public static Money operator *(Money money, decimal multiplier)
        {
            var amount = money.Amount * multiplier;
            return new Money(amount, money.Currency);
        }

        public static bool operator ==(Money? left, Money? right) => Equals(left, right);
        public static bool operator !=(Money? left, Money? right) => !Equals(left, right);
        public static bool operator >(Money left, Money right)
        {
            EnsureSameCurrency(left, right);
            return left.MinorUnits > right.MinorUnits;
        }
        public static bool operator <(Money left, Money right)
        {
            EnsureSameCurrency(left, right);
            return left.MinorUnits < right.MinorUnits;
        }
        public static bool operator >=(Money left, Money right)
        {
            EnsureSameCurrency(left, right);
            return left.MinorUnits >= right.MinorUnits;
        }
        public static bool operator <=(Money left, Money right)
        {
            EnsureSameCurrency(left, right);
            return left.MinorUnits <= right.MinorUnits;
        }

        public int CompareTo(Money? other)
        {
            if (other is null) return 1;
            EnsureSameCurrency(this, other);
            return MinorUnits.CompareTo(other.MinorUnits);
        }

        public bool Equals(Money? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return MinorUnits == other.MinorUnits && Currency == other.Currency;
        }

        public override bool Equals(object? obj) => Equals(obj as Money);

        public override int GetHashCode() => HashCode.Combine(MinorUnits, Currency);

        public override string ToString() => $"{Currency} {Amount.ToString("0.00", CultureInfo.InvariantCulture)}";

        private static long ToMinorUnits(decimal amount)
            => decimal.ToInt64(decimal.Round(amount * MinorUnitsPerMajorUnit, 0, MidpointRounding.AwayFromZero));

        private static void EnsureSameCurrency(Money left, Money right)
        {
            if (left.Currency != right.Currency)
            {
                throw new InvalidOperationException("Cannot operate on money with different currencies.");
            }
        }
    }
}
