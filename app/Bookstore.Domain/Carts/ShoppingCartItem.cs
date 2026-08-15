using Bookstore.Domain.Books;

namespace Bookstore.Domain.Carts
{
    public class ShoppingCartItem : Entity
    {
        // An empty constructor is required by EF Core
        private ShoppingCartItem() { }

        public ShoppingCartItem(ShoppingCart shoppingCart, int bookId, Quantity quantity, bool wantToBuy)
        {
            ShoppingCartId = shoppingCart.Id;
            ShoppingCart = shoppingCart;
            BookId = bookId;
            Quantity = quantity;
            WantToBuy = wantToBuy;
        }

        public int ShoppingCartId { get; set; }

        // Navigation properties populated by EF Core when the item is loaded; the empty
        // constructor above intentionally leaves them for EF to fix up.
        public ShoppingCart ShoppingCart { get; set; } = null!;

        public int BookId { get; set; }
        public Book Book { get; set; } = null!;

        public Quantity Quantity { get; set; }

        public bool WantToBuy { get; set; }

        // The column behind Quantity — see Book for why it exists. The "at least one copy" rule
        // applies on the way back out of the database too: a stored line of zero copies is not a
        // line.
        internal int QuantityValue
        {
            get => Quantity.Value;
            private set => Quantity = Quantity.OfAtLeastOne(value);
        }

        // The price of the book multiplied by the quantity.
        public Money SubTotal => Book.Price * Quantity;
    }
}