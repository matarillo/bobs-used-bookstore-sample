using Bookstore.Domain.Carts;
using Bookstore.Domain.Tests.Builders;
using NSubstitute;

namespace Bookstore.Domain.Tests
{
    public class ShoppingCartServiceTests
    {
        private readonly IShoppingCartRepository shoppingCartRepository = Substitute.For<IShoppingCartRepository>();
        private readonly ShoppingCartService sut;

        public ShoppingCartServiceTests()
        {
            sut = new ShoppingCartService(shoppingCartRepository);
        }

        [Fact]
        public async Task AddToShoppingCartAsync_CreatesANewCart_When_NoneExistsForTheCorrelationId()
        {
            shoppingCartRepository.GetAsync("cart-1").Returns((ShoppingCart)null!);

            var dto = new AddToShoppingCartDto("cart-1", 1, Quantity.Of(2));

            await sut.AddToShoppingCartAsync(dto);

            await shoppingCartRepository.Received(1).AddAsync(Arg.Is<ShoppingCart>(c => c.CorrelationId == "cart-1"));
            await shoppingCartRepository.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task AddToShoppingCartAsync_AddsToTheExistingCart_When_OneAlreadyExists()
        {
            var book = new BookBuilder().Id(1).Build();
            var cart = new ShoppingCartBuilder().Build();
            shoppingCartRepository.GetAsync("cart-1").Returns(cart);

            var dto = new AddToShoppingCartDto("cart-1", book.Id, Quantity.Of(3));

            await sut.AddToShoppingCartAsync(dto);

            var item = Assert.Single(cart.GetShoppingCartItems(ShoppingCartItemFilter.IncludeOutOfStockItems));
            Assert.Equal(Quantity.Of(3), item.Quantity);
            await shoppingCartRepository.DidNotReceive().AddAsync(Arg.Any<ShoppingCart>());
        }

        [Fact]
        public async Task AddToWishlistAsync_AddsAWishlistItemWithAQuantityOfOne()
        {
            var cart = new ShoppingCartBuilder().Build();
            shoppingCartRepository.GetAsync("cart-1").Returns(cart);

            var dto = new AddToWishlistDto("cart-1", 1);

            await sut.AddToWishlistAsync(dto);

            var item = Assert.Single(cart.GetWishListItems());
            Assert.Equal(Quantity.Of(1), item.Quantity);
            await shoppingCartRepository.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task MoveWishlistItemToShoppingCartAsync_Throws_When_TheCartIsNotFound()
        {
            shoppingCartRepository.GetAsync("cart-1").Returns((ShoppingCart)null!);

            var dto = new MoveWishlistItemToShoppingCartDto("cart-1", 1);

            await Assert.ThrowsAsync<DomainException>(() => sut.MoveWishlistItemToShoppingCartAsync(dto));
        }

        [Fact]
        public async Task MoveWishlistItemToShoppingCartAsync_MovesTheItemAndSaves_When_TheCartIsFound()
        {
            var book = new BookBuilder().Id(1).Build();
            var cart = new ShoppingCartBuilder().WithWishListItem(book).Build();
            var wishListItemId = cart.GetWishListItems().Single().Id;
            shoppingCartRepository.GetAsync("cart-1").Returns(cart);

            var dto = new MoveWishlistItemToShoppingCartDto("cart-1", wishListItemId);

            await sut.MoveWishlistItemToShoppingCartAsync(dto);

            Assert.Single(cart.GetShoppingCartItems(ShoppingCartItemFilter.IncludeOutOfStockItems));
            Assert.Empty(cart.GetWishListItems());
            await shoppingCartRepository.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task MoveAllWishlistItemsToShoppingCartAsync_DoesNothing_When_TheCartIsNotFound()
        {
            shoppingCartRepository.GetAsync("cart-1").Returns((ShoppingCart)null!);

            var dto = new MoveAllWishlistItemsToShoppingCartDto("cart-1");

            await sut.MoveAllWishlistItemsToShoppingCartAsync(dto);

            await shoppingCartRepository.DidNotReceive().SaveChangesAsync();
        }

        [Fact]
        public async Task MoveAllWishlistItemsToShoppingCartAsync_MovesEveryWishlistItem_When_TheCartIsFound()
        {
            var firstBook = new BookBuilder().Id(1).Build();
            var secondBook = new BookBuilder().Id(2).Build();
            var cart = new ShoppingCartBuilder()
                .WithWishListItem(firstBook)
                .WithWishListItem(secondBook)
                .Build();
            shoppingCartRepository.GetAsync("cart-1").Returns(cart);

            var dto = new MoveAllWishlistItemsToShoppingCartDto("cart-1");

            await sut.MoveAllWishlistItemsToShoppingCartAsync(dto);

            Assert.Equal(2, cart.GetShoppingCartItems(ShoppingCartItemFilter.IncludeOutOfStockItems).Count());
            Assert.Empty(cart.GetWishListItems());
            await shoppingCartRepository.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task DeleteShoppingCartItemAsync_Throws_When_TheCartIsNotFound()
        {
            shoppingCartRepository.GetAsync("cart-1").Returns((ShoppingCart)null!);

            var dto = new DeleteShoppingCartItemDto("cart-1", 1);

            await Assert.ThrowsAsync<DomainException>(() => sut.DeleteShoppingCartItemAsync(dto));
        }

        [Fact]
        public async Task DeleteShoppingCartItemAsync_RemovesTheItemAndSaves_When_TheCartIsFound()
        {
            var book = new BookBuilder().Id(1).Build();
            var cart = new ShoppingCartBuilder().WithShoppingCartItem(book, 1).Build();
            var itemId = cart.GetShoppingCartItems(ShoppingCartItemFilter.IncludeOutOfStockItems).Single().Id;
            shoppingCartRepository.GetAsync("cart-1").Returns(cart);

            var dto = new DeleteShoppingCartItemDto("cart-1", itemId);

            await sut.DeleteShoppingCartItemAsync(dto);

            Assert.Empty(cart.GetShoppingCartItems(ShoppingCartItemFilter.IncludeOutOfStockItems));
            await shoppingCartRepository.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task GetShoppingCartAsync_DelegatesToTheRepository()
        {
            var cart = new ShoppingCartBuilder().Build();
            shoppingCartRepository.GetAsync("cart-1").Returns(cart);

            var result = await sut.GetShoppingCartAsync("cart-1");

            Assert.Same(cart, result);
        }
    }
}
