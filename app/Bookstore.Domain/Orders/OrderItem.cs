using Bookstore.Domain.Books;

namespace Bookstore.Domain.Orders
{
    public class OrderItem : Entity
    {
        // This private constructor is required by EF Core
        private OrderItem() { }

        public OrderItem(Order order, Book book, Quantity quantity)
        {
            OrderId = order.Id;
            Order = order;
            BookId = book.Id;
            Book = book;
            Quantity = quantity;
            Price = book.Price;
            Cost = book.PurchaseCost;
        }

        public int OrderId { get; set; }
        public Order Order { get; set; }

        public int BookId { get; set; }
        public Book Book { get; set; }

        public Quantity Quantity { get; private set; }

        // RULE-ORDER-03, revised by ISSUE-17: the price agreed when the order was placed. An
        // order records an agreement, so it must not follow later changes to the book price.
        public Money Price { get; private set; }

        // What the store paid for the book, copied from Book.PurchaseCost (ISSUE-06) when the
        // order was placed. Recorded here for the same reason Price is: the profit on a sale is
        // settled at the moment of the sale and must not follow later changes. Null when the book
        // did not come from an offer, because then the domain never knew what it cost.
        public Money? Cost { get; private set; }

        // ISSUE-07: the columns behind the values above — see Book for why they exist.
        internal int QuantityValue
        {
            get => Quantity.Value;
            private set => Quantity = Quantity.OfAtLeastOne(value);
        }

        internal decimal PriceAmount
        {
            get => Price.Amount;
            private set => Price = Money.Of(value);
        }

        internal decimal? CostAmount
        {
            get => Cost?.Amount;
            private set => Cost = Money.OfNullable(value);
        }

        // RULE-ORDER-02, revised by ISSUE-02: the agreed price multiplied by the quantity.
        public Money SubTotal => Price * Quantity;

        // The measure the business runs on: sell for more than you paid. Null, rather than equal
        // to the subtotal, when the cost is unknown — an unknown cost is not a cost of zero. A
        // decimal because a profit can be a loss (see Money.Less).
        public decimal? GrossProfit => Cost.HasValue ? Price.Less(Cost.Value) * Quantity.Value : null;
    }
}
