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
                ShoppingCartItems.Where(x => x.WantToBuy && x.Book.IsInStock);
        }

        public IEnumerable<ShoppingCartItem> GetWishListItems()
        {
            return ShoppingCartItems.Where(x => x.WantToBuy == false);
        }

        // A book occupies a single line of the shopping cart. Adding it again increases the
        // quantity of the existing line rather than creating a second one.
        public void AddItemToShoppingCart(int bookId, Quantity quantity)
        {
            // Quantity rules out a negative count; that a line is for at least one copy is the
            // cart's rule rather than the value's.
            if (quantity.IsNone)
            {
                throw new DomainException("A shopping cart line must be for at least one copy.");
            }

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

        // A wish list line is always for one copy, so a duplicate is simply not added.
        public void AddItemToWishlist(int bookId)
        {
            if (ShoppingCartItems.Any(x => x.BookId == bookId && x.WantToBuy == false)) return;

            ShoppingCartItems.Add(new ShoppingCartItem(this, bookId, Quantity.One, false));
        }

        // A specifically-identified item that is not there is a failure, the same policy applied
        // to RemoveShoppingCartItemById below.
        public void MoveWishListItemToShoppingCart(int shoppingCartItemId)
        {
            var wishListItem = ShoppingCartItems.SingleOrDefault(x => x.Id == shoppingCartItemId);

            if (wishListItem == null)
            {
                throw new DomainException($"Wish list item {shoppingCartItemId} was not found.");
            }

            var existingItem = ShoppingCartItems.FirstOrDefault(x => x.BookId == wishListItem.BookId && x.WantToBuy);

            if (existingItem == null)
            {
                wishListItem.WantToBuy = true;
            }
            else
            {
                // Merging into the existing line keeps one line per book.
                existingItem.Quantity += wishListItem.Quantity;

                ShoppingCartItems.Remove(wishListItem);
            }
        }

        // Deleting a specifically-identified item that is not there fails, rather than silently
        // doing nothing — the policy for single-item updates generally.
        public void RemoveShoppingCartItemById(int shoppingCartItemId)
        {
            var shoppingCartItem = ShoppingCartItems.SingleOrDefault(x => x.Id == shoppingCartItemId);

            if (shoppingCartItem == null)
            {
                throw new DomainException($"Shopping cart item {shoppingCartItemId} was not found.");
            }

            ShoppingCartItems.Remove(shoppingCartItem);
        }

        public Money GetSubTotal(ShoppingCartItemFilter filter)
        {
            return GetShoppingCartItems(filter).Sum(x => x.SubTotal);
        }

        // The items eligible for a new order are those the customer wants to buy and that are
        // currently in stock. An order needs at least one item, so this throws instead of handing
        // back an empty list — whether the cart itself is empty or everything in it is out of
        // stock amounts to the same "nothing to order".
        public IReadOnlyList<ShoppingCartItem> GetItemsForNewOrder()
        {
            var items = GetShoppingCartItems(ShoppingCartItemFilter.ExcludeOutOfStockItems).ToList();

            if (items.Count == 0)
            {
                throw new DomainException("There are no items in stock to place an order with.");
            }

            return items;
        }

        // The items the customer wants to buy that were left out of a new order because they are
        // out of stock, so the caller can tell the customer which items were skipped. They stay
        // in the cart rather than being removed — the customer may still want them once the book
        // is back in stock.
        public IReadOnlyList<ShoppingCartItem> GetOutOfStockWantedItems()
        {
            return ShoppingCartItems.Where(x => x.WantToBuy && !x.Book.IsInStock).ToList();
        }
    }

    public enum ShoppingCartItemFilter
    {
        IncludeOutOfStockItems,
        ExcludeOutOfStockItems
    }
}