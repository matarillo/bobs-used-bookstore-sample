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
        public ShoppingCart ShoppingCart { get; set; }

        public int BookId { get; set; }
        public Book Book { get; set; }

        public Quantity Quantity { get; set; }

        public bool WantToBuy { get; set; }

        // ISSUE-07: the column behind Quantity — see Book for why it exists. INV-CART-05 applies
        // on the way back out of the database too: a stored line of zero copies is not a line.
        internal int QuantityValue
        {
            get => Quantity.Value;
            private set => Quantity = Quantity.OfAtLeastOne(value);
        }

        // RULE-CART-03, revised by ISSUE-02: the price of the book multiplied by the quantity.
        public Money SubTotal => Book.Price * Quantity;
    }
}