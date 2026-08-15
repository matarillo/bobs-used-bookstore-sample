namespace Bookstore.Domain
{
    // ISSUE-07: an amount of money, rather than a bare decimal. A price, a buying price, a
    // subtotal, a tax and a total are all the same kind of thing, and that kind of thing has
    // properties a decimal does not: one currency, a rounding rule, and no negative values.
    //
    // Validity is settled at creation (Of), so nothing downstream has to re-check it. Read-side
    // statistics stay in decimal: they are projections computed by the database (12 §2.4), not
    // part of the model whose invariants this type carries.
    public readonly record struct Money : IComparable<Money>
    {
        // The store trades in one currency. Naming it here is the point: an amount that does not
        // say what it is denominated in cannot be added to another one safely. Should the store
        // ever trade in two, this is where the second one has to be dealt with.
        public const string CurrencyCode = "USD";

        // The smallest unit the store can actually charge or pay. An amount is rounded to it on
        // the way in, so no amount can ever be a fraction of a cent.
        public const int DecimalPlaces = 2;

        public static readonly Money Zero = default;

        private Money(decimal amount)
        {
            Amount = amount;
        }

        public decimal Amount { get; }

        public bool IsZero => Amount == 0m;

        // INV-BOOK-06, INV-OFFER-06: money is never negative. A price of minus five is not a
        // cheap book, it is a mistake, and it used to be storable.
        public static Money Of(decimal amount)
        {
            if (amount < 0m)
            {
                throw new DomainException($"An amount of money cannot be negative; was {amount}.");
            }

            return new Money(Round(amount));
        }

        public static Money? OfNullable(decimal? amount) => amount.HasValue ? Of(amount.Value) : null;

        public static Money operator +(Money left, Money right) => new(left.Amount + right.Amount);

        // RULE-CART-03, RULE-ORDER-02: the multiplication ISSUE-02 was about. The type says what
        // the operation means — a price for one copy, taken so many times, is an amount — so the
        // two operands can no longer be swapped or forgotten unnoticed.
        public static Money operator *(Money price, Quantity quantity) => new(Round(price.Amount * quantity.Value));

        public static Money operator *(Quantity quantity, Money price) => price * quantity;

        // A share of an amount, such as tax. Rounded, so the share is itself chargeable.
        public Money Times(decimal factor)
        {
            if (factor < 0m)
            {
                throw new DomainException($"An amount of money cannot be scaled by a negative factor; was {factor}.");
            }

            return new Money(Round(Amount * factor));
        }

        // The difference between two amounts, which is deliberately not a Money: a margin can be
        // negative (the store sold for less than it paid), and a negative Money does not exist.
        public decimal Less(Money other) => Amount - other.Amount;

        public static bool operator <(Money left, Money right) => left.Amount < right.Amount;

        public static bool operator >(Money left, Money right) => left.Amount > right.Amount;

        public static bool operator <=(Money left, Money right) => left.Amount <= right.Amount;

        public static bool operator >=(Money left, Money right) => left.Amount >= right.Amount;

        public int CompareTo(Money other) => Amount.CompareTo(other.Amount);

        public override string ToString() => Amount.ToString("C");

        private static decimal Round(decimal amount) =>
            decimal.Round(amount, DecimalPlaces, MidpointRounding.AwayFromZero);
    }

    public static class MoneyEnumerableExtensions
    {
        // LINQ has no Sum for Money, and adding one here keeps the totals in the type rather than
        // dropping to decimal to add them up.
        public static Money Sum(this IEnumerable<Money> source) =>
            source.Aggregate(Money.Zero, static (running, next) => running + next);

        public static Money Sum<T>(this IEnumerable<T> source, Func<T, Money> selector) =>
            source.Aggregate(Money.Zero, (running, next) => running + selector(next));
    }
}
