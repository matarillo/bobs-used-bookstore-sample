using Bookstore.Domain.Carts;
using Bookstore.Domain.Tests.Builders;

namespace Bookstore.Domain.Tests
{
    public class ShoppingCartTests
    {
        [Fact]
        public void AddItemToShoppingCart_AddsASecondLine_When_TheSameBookIsAddedTwice()
        {
            var book = new BookBuilder().Id(1).Build();

            var shoppingCart = new ShoppingCartBuilder()
                .WithShoppingCartItem(book, 2)
                .WithShoppingCartItem(book, 3)
                .Build();

            Assert.Equal(2, shoppingCart.GetShoppingCartItems(ShoppingCartItemFilter.IncludeOutOfStockItems).Count());
        }

        [Fact]
        public void AddItemToWishlist_AddsASecondLine_When_TheSameBookIsAddedTwice()
        {
            var book = new BookBuilder().Id(1).Build();

            var shoppingCart = new ShoppingCartBuilder()
                .WithWishListItem(book)
                .WithWishListItem(book)
                .Build();

            Assert.Equal(2, shoppingCart.GetWishListItems().Count());
        }

        [Fact]
        public void GetSubTotal_IgnoresTheQuantity_When_Executed()
        {
            var book = new BookBuilder().Id(1).Price(10m).Quantity(100).Build();

            var shoppingCart = new ShoppingCartBuilder()
                .WithShoppingCartItem(book, 3)
                .Build();

            Assert.Equal(10m, shoppingCart.GetSubTotal(ShoppingCartItemFilter.ExcludeOutOfStockItems));
        }

        [Fact]
        public void GetSubTotal_ExcludesOutOfStockItems_When_TheFilterExcludesThem()
        {
            var inStockBook = new BookBuilder().Id(1).Price(10m).Quantity(100).Build();
            var outOfStockBook = new BookBuilder().Id(2).Price(20m).Quantity(0).Build();

            var shoppingCart = new ShoppingCartBuilder()
                .WithShoppingCartItem(inStockBook, 1)
                .WithShoppingCartItem(outOfStockBook, 1)
                .Build();

            Assert.Equal(10m, shoppingCart.GetSubTotal(ShoppingCartItemFilter.ExcludeOutOfStockItems));
            Assert.Equal(30m, shoppingCart.GetSubTotal(ShoppingCartItemFilter.IncludeOutOfStockItems));
        }

        [Fact]
        public void GetSubTotal_IgnoresWishListItems_When_Executed()
        {
            var shoppingCartBook = new BookBuilder().Id(1).Price(10m).Quantity(100).Build();
            var wishListBook = new BookBuilder().Id(2).Price(20m).Quantity(100).Build();

            var shoppingCart = new ShoppingCartBuilder()
                .WithShoppingCartItem(shoppingCartBook, 1)
                .WithWishListItem(wishListBook)
                .Build();

            Assert.Equal(10m, shoppingCart.GetSubTotal(ShoppingCartItemFilter.IncludeOutOfStockItems));
        }

        [Fact]
        public void AddItemToWishlist_CreatesAnItemWithAQuantityOfOne_When_Executed()
        {
            var book = new BookBuilder().Id(1).Build();

            var shoppingCart = new ShoppingCartBuilder()
                .WithWishListItem(book)
                .Build();

            Assert.Equal(1, shoppingCart.GetWishListItems().Single().Quantity);
        }
    }
}
