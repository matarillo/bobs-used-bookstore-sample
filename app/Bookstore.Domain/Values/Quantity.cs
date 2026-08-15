namespace Bookstore.Domain
{
    // A number of copies of a book, rather than a bare int. Two rules apply to it: a count of
    // copies is never negative, and a line of a cart or an order refers to at least one copy.
    //
    // Both rules are settled at creation. Which of the two applies depends on what is being
    // counted, so there are two factories rather than one: stock can legitimately be none, a
    // line of an order cannot.
    public readonly record struct Quantity : IComparable<Quantity>
    {
        public static readonly Quantity None = default;

        public static readonly Quantity One = new(1);

        private Quantity(int value)
        {
            Value = value;
        }

        public int Value { get; }

        public bool IsNone => Value == 0;

        // A count of copies, which may be none — an empty shelf is a real stock level.
        public static Quantity Of(int value)
        {
            if (value < 0)
            {
                throw new DomainException($"A quantity cannot be negative; was {value}.");
            }

            return new Quantity(value);
        }

        // A line that refers to no copies is not a line.
        public static Quantity OfAtLeastOne(int value)
        {
            if (value < 1)
            {
                throw new DomainException($"A quantity of at least one is required; was {value}.");
            }

            return new Quantity(value);
        }

        public static Quantity operator +(Quantity left, Quantity right) => new(left.Value + right.Value);

        // Of, not the private constructor: taking more than there is would go below zero, and
        // that must fail rather than happen silently.
        public static Quantity operator -(Quantity left, Quantity right) => Of(left.Value - right.Value);

        public static bool operator <(Quantity left, Quantity right) => left.Value < right.Value;

        public static bool operator >(Quantity left, Quantity right) => left.Value > right.Value;

        public static bool operator <=(Quantity left, Quantity right) => left.Value <= right.Value;

        public static bool operator >=(Quantity left, Quantity right) => left.Value >= right.Value;

        public int CompareTo(Quantity other) => Value.CompareTo(other.Value);

        public override string ToString() => Value.ToString();
    }
}
