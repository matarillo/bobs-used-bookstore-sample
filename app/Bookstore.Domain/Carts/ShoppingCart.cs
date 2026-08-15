namespace Bookstore.Domain.Carts
{
    public class ShoppingCart : Entity
    {
        public List<ShoppingCartItem> ShoppingCartItems { get; private set; } = new();

        public string CorrelationId { get; set; }

        public ShoppingCart(string correlationId)
        {
            CorrelationId = correlationId;
        }

        public IEnumerable<ShoppingCartItem> GetShoppingCartItems(ShoppingCartItemFilter filter)
        {
            return filter == ShoppingCartItemFilter.IncludeOutOfStockItems ?
                ShoppingCartItems.Where(x => x.WantToBuy) :
                ShoppingCartItems.Where(x => x.WantToBuy && x.Book.Quantity > 0);
        }

        public IEnumerable<ShoppingCartItem> GetWishListItems()
        {
            return ShoppingCartItems.Where(x => x.WantToBuy == false);
        }

        // INV-CART-04: a book occupies a single line of the shopping cart. Adding it again
        // increases the quantity of the existing line rather than creating a second one.
        public void AddItemToShoppingCart(int bookId, int quantity)
        {
            var existingItem = ShoppingCartItems.FirstOrDefault(x => x.BookId == bookId && x.WantToBuy);

            if (existingItem == null)
            {
                ShoppingCartItems.Add(new ShoppingCartItem(this, bookId, quantity, true));
            }
            else
            {
                existingItem.Quantity += quantity;
            }
        }

        // INV-CART-03 keeps the quantity of a wish list item at 1, so a duplicate is simply not added.
        public void AddItemToWishlist(int bookId)
        {
            if (ShoppingCartItems.Any(x => x.BookId == bookId && x.WantToBuy == false)) return;

            ShoppingCartItems.Add(new ShoppingCartItem(this, bookId, 1, false));
        }

        public void MoveWishListItemToShoppingCart(int shoppingCartItemId)
        {
            var wishListItem = ShoppingCartItems.SingleOrDefault(x => x.Id == shoppingCartItemId);

            if (wishListItem == null) return;

            var existingItem = ShoppingCartItems.FirstOrDefault(x => x.BookId == wishListItem.BookId && x.WantToBuy);

            if (existingItem == null)
            {
                wishListItem.WantToBuy = true;
            }
            else
            {
                // Merging into the existing line keeps INV-CART-04 intact.
                existingItem.Quantity += wishListItem.Quantity;

                ShoppingCartItems.Remove(wishListItem);
            }
        }

        public void RemoveShoppingCartItemById(int shoppingCartItemId)
        {
            var shoppingCartItem = ShoppingCartItems.Single(x => x.Id == shoppingCartItemId);

            ShoppingCartItems.Remove(shoppingCartItem);
        }

        public decimal GetSubTotal(ShoppingCartItemFilter filter)
        {
            return GetShoppingCartItems(filter).Sum(x => x.SubTotal);
        }
    }

    public enum ShoppingCartItemFilter
    {
        IncludeOutOfStockItems,
        ExcludeOutOfStockItems
    }
}