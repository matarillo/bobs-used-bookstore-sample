using Bookstore.Domain.Books;

namespace Bookstore.Domain.Orders
{
    public class OrderItem : Entity
    {
        // This private constructor is required by EF Core
        private OrderItem() { }

        public OrderItem(Order order, Book book, int quantity)
        {
            OrderId = order.Id;
            Order = order;
            BookId = book.Id;
            Book = book;
            Quantity = quantity;
            Price = book.Price;
        }

        public int OrderId { get; set; }
        public Order Order { get; set; }

        public int BookId { get; set; }
        public Book Book { get; set; }

        public int Quantity { get; set; }

        // RULE-ORDER-03: the price agreed when the order was placed. An order is the record
        // of an agreement, so it must not follow later changes to the price of the book.
        public decimal Price { get; private set; }

        // RULE-ORDER-02: the agreed price multiplied by the quantity ordered.
        public decimal SubTotal => Price * Quantity;
    }
}