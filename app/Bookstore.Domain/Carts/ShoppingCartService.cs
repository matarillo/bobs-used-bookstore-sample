namespace Bookstore.Domain.Carts
{
    public interface IShoppingCartService
    {
        Task<ShoppingCart> GetShoppingCartAsync(string correlationId);

        Task AddToShoppingCartAsync(AddToShoppingCartDto addToShoppingCartDto);

        Task AddToWishlistAsync(AddToWishlistDto addToWishlistDto);

        Task MoveWishlistItemToShoppingCartAsync(MoveWishlistItemToShoppingCartDto moveWishlistItemToShoppingCartDto);

        Task MoveAllWishlistItemsToShoppingCartAsync(MoveAllWishlistItemsToShoppingCartDto moveAllWishlistItemsToShoppingCartDto);

        Task DeleteShoppingCartItemAsync(DeleteShoppingCartItemDto deleteShoppingCartItemDto);
    }

    public class ShoppingCartService : IShoppingCartService
    {
        private readonly IShoppingCartRepository shoppingCartRepository;

        public ShoppingCartService(IShoppingCartRepository shoppingCartRepository)
        {
            this.shoppingCartRepository = shoppingCartRepository;
        }

        // ISSUE-13: a query is tolerant of a missing cart — it is a valid "nothing here yet"
        // result, not a failure. Callers that render a cart (or a wish list) already treat null
        // as empty.
        public async Task<ShoppingCart> GetShoppingCartAsync(string shoppingCartCorrelationId)
        {
            return await shoppingCartRepository.GetAsync(shoppingCartCorrelationId);
        }

        public async Task AddToShoppingCartAsync(AddToShoppingCartDto dto)
        {
            await AddToShoppingCartAsync(dto.CorrelationId, dto.BookId, dto.Quantity, true);
        }

        public async Task AddToWishlistAsync(AddToWishlistDto dto)
        {
            // INV-CART-03 keeps a wish list line at one copy.
            await AddToShoppingCartAsync(dto.CorrelationId, dto.BookId, Quantity.One, false);
        }

        private async Task AddToShoppingCartAsync(string correlationId, int bookId, Quantity quantity, bool wantToBuy)
        {
            var shoppingCart = await shoppingCartRepository.GetAsync(correlationId);

            if (shoppingCart == null)
            {
                shoppingCart = new ShoppingCart(correlationId);

                await shoppingCartRepository.AddAsync(shoppingCart);
            }

            if (wantToBuy)
            {
                shoppingCart.AddItemToShoppingCart(bookId, quantity);
            }
            else
            {
                shoppingCart.AddItemToWishlist(bookId);
            }

            await shoppingCartRepository.SaveChangesAsync();
        }

        // ISSUE-13: moving one specifically-identified item is a strict update — a cart that
        // does not exist is a failure, matching the item-not-found case ShoppingCart itself now
        // enforces.
        public async Task MoveWishlistItemToShoppingCartAsync(MoveWishlistItemToShoppingCartDto dto)
        {
            var shoppingCart = await shoppingCartRepository.GetAsync(dto.CorrelationId);

            if (shoppingCart == null)
            {
                throw new DomainException($"Shopping cart \"{dto.CorrelationId}\" was not found.");
            }

            shoppingCart.MoveWishListItemToShoppingCart(dto.ShoppingCartItemId);

            await shoppingCartRepository.SaveChangesAsync();
        }

        // ISSUE-13: deliberately kept tolerant, unlike the single-item move above. "Move
        // everything" targets the whole wish list rather than one identified item, and a cart
        // that does not exist simply has nothing to move — the same vacuous-success reasoning
        // the design doc applies to order cancellation.
        public async Task MoveAllWishlistItemsToShoppingCartAsync(MoveAllWishlistItemsToShoppingCartDto dto)
        {
            var shoppingCart = await shoppingCartRepository.GetAsync(dto.CorrelationId);

            if (shoppingCart == null) return;

            // Materialised because moving an item can remove it from the underlying collection.
            foreach (var wishListItem in shoppingCart.GetWishListItems().ToList())
            {
                shoppingCart.MoveWishListItemToShoppingCart(wishListItem.Id);
            }

            await shoppingCartRepository.SaveChangesAsync();
        }

        // ISSUE-13: deleting a specifically-identified item is a strict update.
        public async Task DeleteShoppingCartItemAsync(DeleteShoppingCartItemDto dto)
        {
            var shoppingCart = await shoppingCartRepository.GetAsync(dto.CorrelationId);

            if (shoppingCart == null)
            {
                throw new DomainException($"Shopping cart \"{dto.CorrelationId}\" was not found.");
            }

            shoppingCart.RemoveShoppingCartItemById(dto.ShoppingCartItemId);

            await shoppingCartRepository.SaveChangesAsync();
        }
    }
}