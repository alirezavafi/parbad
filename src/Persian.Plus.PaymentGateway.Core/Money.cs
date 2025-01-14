// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Globalization;

namespace Persian.Plus.PaymentGateway.Core
{
    /// <summary>
    /// Defines money unit.
    /// <para>
    /// Note: The official unit of currency in Iran is the Iranian rial (IR).
    /// It means the amount of the invoice will be sent to Iranian gateways automatically
    /// as <see cref="Int64"/> by Persian.Plus.PaymentGateway.Core.
    /// </para>
    /// <para>Examples:
    /// <para>decimal a = new Money(1000)</para>
    /// <para>long a = new Money(1000)</para>
    /// <para>Money m = 1000.55m</para>
    /// <para>Money m = 1000</para>
    /// </para>
    /// </summary>
    public readonly struct Money : IComparable<Money>, IEquatable<Money>, IEquatable<decimal>, IEquatable<long>
    {
        /// <summary>
        /// Defines money unit.
        /// <para>
        /// Note: The official unit of currency in Iran is the Iranian rial (IR).
        /// It means the amount of the invoice will be sent to Iranian gateways automatically
        /// as <see cref="Int64"/> by Persian.Plus.PaymentGateway.Core.
        /// </para>
        /// </summary>
        /// <param name="value">The amount of money.</param>
        public Money(decimal value)
        {
            Value = value;
        }

        public decimal Value { get; }

        public Money AddAmount(decimal amount)
        {
            return new Money(Value + amount);
        }

        public bool Equals(Money? other) => false;
        public bool Equals(Money other) => Value == other.Value;

        public bool Equals(long? other) => false;
        public bool Equals(long other) => (long)Value == other;

        public bool Equals(decimal? other) => false;
        public bool Equals(decimal other) => Value == other;

        public override bool Equals(object obj)
        {
            var equals =
                (obj is Money other && Equals(other)) ||
                (obj is int intNumber) && Equals((long)intNumber) ||
                (obj is long longNumber) && Equals(longNumber) ||
                (obj is decimal decimalNumber) && Equals(decimalNumber);

            return equals;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public int CompareTo(Money other)
        {
            return Value.CompareTo(other.Value);
        }

        public override string ToString()
        {
            return ToString(CultureInfo.InvariantCulture);
        }

        public string ToString(IFormatProvider format)
        {
            return Value.ToString(format);
        }

        public string ToString(string format)
        {
            return Value.ToString(format);
        }

        public static Money Parse(decimal amount) => new Money(amount);

        public static Money Parse(long amount) => new Money(amount);

        /// <exception cref="Exception"></exception>
        public static Money Parse(string amount)
        {
            if (!decimal.TryParse(amount, out var testValue))
            {
                throw new Exception($"Cannot parse {amount} to Money.");
            }

            return testValue;
        }

        public static bool TryParse(string value, out Money money)
        {
            try
            {
                money = Parse(value);
                return true;
            }
            catch
            {
                money = default;
                return false;
            }
        }

        public static implicit operator decimal(Money money)
        {
            return money.Value;
        }

        public static implicit operator long(Money money)
        {
            return (long)money.Value;
        }

        public static implicit operator Money(decimal amount) => Parse(amount);

        public static implicit operator Money(long amount) => Parse(amount);

        public static bool operator >(Money left, Money right)
        {
            return left.Value > right.Value;
        }

        public static bool operator <(Money left, Money right)
        {
            return !(left > right);
        }

        public static bool operator >=(Money left, Money right)
        {
            return left.Value >= right.Value;
        }

        public static bool operator <=(Money left, Money right)
        {
            return left.Value <= right.Value;
        }

        public static Money operator +(Money left, Money right)
        {
            return new Money(left.Value + right.Value);
        }

        public static Money operator -(Money left, Money right)
        {
            return new Money(left.Value - right.Value);
        }

        public static Money operator *(Money left, Money right)
        {
            return new Money(left.Value * right.Value);
        }

        public static Money operator /(Money left, Money right)
        {
            return new Money(left.Value / right.Value);
        }
    }
}
