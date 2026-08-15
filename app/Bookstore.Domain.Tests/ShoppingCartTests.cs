using Bookstore.Domain.Carts;
using Bookstore.Domain.Tests.Builders;

namespace Bookstore.Domain.Tests
{
    public class ShoppingCartTests
    {
        [Fact]
        public void AddItemToShoppingCart_IncreasesTheQuantityOfTheExistingLine_When_TheSameBookIsAddedTwice()
        {
            var book = new BookBuilder().Id(1).Build();

            var shoppingCart = new ShoppingCartBuilder()
                .WithShoppingCartItem(book, 2)
                .WithShoppingCartItem(book, 3)
                .Build();

            var shoppingCartItem = shoppingCart.GetShoppingCartItems(ShoppingCartItemFilter.IncludeOutOfStockItems).Single();

            Assert.Equal(5, shoppingCartItem.Quantity);
        }

        [Fact]
        public void AddItemToShoppingCart_AddsASeparateLine_When_ADifferentBookIsAdded()
        {
            var firstBook = new BookBuilder().Id(1).Build();
            var secondBook = new BookBuilder().Id(2).Build();

            var shoppingCart = new ShoppingCartBuilder()
                .WithShoppingCartItem(firstBook, 1)
                .WithShoppingCartItem(secondBook, 1)
                .Build();

            Assert.Equal(2, shoppingCart.GetShoppingCartItems(ShoppingCartItemFilter.IncludeOutOfStockItems).Count());
        }

        [Fact]
        public void AddItemToShoppingCart_LeavesTheWishListItemUntouched_When_TheSameBookIsOnTheWishList()
        {
            var book = new BookBuilder().Id(1).Build();

            var shoppingCart = new ShoppingCartBuilder()
                .WithWishListItem(book)
                .WithShoppingCartItem(book, 2)
                .Build();

            Assert.Equal(2, shoppingCart.GetShoppingCartItems(ShoppingCartItemFilter.IncludeOutOfStockItems).Single().Quantity);
            Assert.Single(shoppingCart.GetWishListItems());
        }

        [Fact]
        public void AddItemToWishlist_DoesNotAddASecondLine_When_TheSameBookIsAddedTwice()
        {
            var book = new BookBuilder().Id(1).Build();

            var shoppingCart = new ShoppingCartBuilder()
                .WithWishListItem(book)
                .WithWishListItem(book)
                .Build();

            Assert.Single(shoppingCart.GetWishListItems());
        }

        [Fact]
        public void MoveWishListItemToShoppingCart_MergesIntoTheExistingLine_When_TheBookIsAlreadyInTheShoppingCart()
        {
            var book = new BookBuilder().Id(1).Build();

            var shoppingCart = new ShoppingCartBuilder()
                .WithShoppingCartItem(book, 2)
                .WithWishListItem(book)
                .Build();

            var wishListItemId = shoppingCart.GetWishListItems().Single().Id;

            shoppingCart.MoveWishListItemToShoppingCart(wishListItemId);

            Assert.Equal(3, shoppingCart.GetShoppingCartItems(ShoppingCartItemFilter.IncludeOutOfStockItems).Single().Quantity);
            Assert.Empty(shoppingCart.GetWishListItems());
        }

        [Fact]
        public void MoveWishListItemToShoppingCart_ConvertsTheItem_When_TheBookIsNotInTheShoppingCart()
        {
            var book = new BookBuilder().Id(1).Build();

            var shoppingCart = new ShoppingCartBuilder()
                .WithWishListItem(book)
                .Build();

            var wishListItemId = shoppingCart.GetWishListItems().Single().Id;

            shoppingCart.MoveWishListItemToShoppingCart(wishListItemId);

            Assert.Single(shoppingCart.GetShoppingCartItems(ShoppingCartItemFilter.IncludeOutOfStockItems));
            Assert.Empty(shoppingCart.GetWishListItems());
        }

        [Fact]
        public void MoveWishListItemToShoppingCart_Throws_When_TheItemDoesNotExist()
        {
            var shoppingCart = new ShoppingCartBuilder().Build();

            Assert.Throws<DomainException>(() => shoppingCart.MoveWishListItemToShoppingCart(999));
        }

        [Fact]
        public void RemoveShoppingCartItemById_Throws_When_TheItemDoesNotExist()
        {
            var shoppingCart = new ShoppingCartBuilder().Build();

            Assert.Throws<DomainException>(() => shoppingCart.RemoveShoppingCartItemById(999));
        }

        [Fact]
        public void GetSubTotal_MultipliesThePriceByTheQuantity_When_Executed()
        {
            var book = new BookBuilder().Id(1).Price(10m).Quantity(100).Build();

            var shoppingCart = new ShoppingCartBuilder()
                .WithShoppingCartItem(book, 3)
                .Build();

            Assert.Equal(30m, shoppingCart.GetSubTotal(ShoppingCartItemFilter.ExcludeOutOfStockItems));
        }

        [Fact]
        public void GetSubTotal_CountsAMergedLineOnce_When_TheSameBookIsAddedTwice()
        {
            var book = new BookBuilder().Id(1).Price(10m).Quantity(100).Build();

            var shoppingCart = new ShoppingCartBuilder()
                .WithShoppingCartItem(book, 2)
                .WithShoppingCartItem(book, 3)
                .Build();

            Assert.Equal(50m, shoppingCart.GetSubTotal(ShoppingCartItemFilter.ExcludeOutOfStockItems));
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

        [Fact]
        public void GetItemsForNewOrder_ReturnsOnlyInStockItems_When_TheCartHasBoth()
        {
            var inStockBook = new BookBuilder().Id(1).Quantity(5).Build();
            var outOfStockBook = new BookBuilder().Id(2).Quantity(0).Build();

            var shoppingCart = new ShoppingCartBuilder()
                .WithShoppingCartItem(inStockBook, 1)
                .WithShoppingCartItem(outOfStockBook, 1)
                .Build();

            var items = shoppingCart.GetItemsForNewOrder();

            Assert.Equal(inStockBook.Id, Assert.Single(items).BookId);
        }

        [Fact]
        public void GetItemsForNewOrder_Throws_When_TheCartIsEmpty()
        {
            var shoppingCart = new ShoppingCartBuilder().Build();

            Assert.Throws<DomainException>(() => shoppingCart.GetItemsForNewOrder());
        }

        [Fact]
        public void GetItemsForNewOrder_Throws_When_EveryWantedItemIsOutOfStock()
        {
            var outOfStockBook = new BookBuilder().Id(1).Quantity(0).Build();

            var shoppingCart = new ShoppingCartBuilder()
                .WithShoppingCartItem(outOfStockBook, 1)
                .Build();

            Assert.Throws<DomainException>(() => shoppingCart.GetItemsForNewOrder());
        }

        [Fact]
        public void GetItemsForNewOrder_IgnoresWishListItems_When_DecidingWhetherTheCartIsEmpty()
        {
            var wishListBook = new BookBuilder().Id(1).Quantity(5).Build();

            var shoppingCart = new ShoppingCartBuilder()
                .WithWishListItem(wishListBook)
                .Build();

            Assert.Throws<DomainException>(() => shoppingCart.GetItemsForNewOrder());
        }

        [Fact]
        public void GetOutOfStockWantedItems_ReturnsOnlyOutOfStockItems_When_TheCartHasBoth()
        {
            var inStockBook = new BookBuilder().Id(1).Quantity(5).Build();
            var outOfStockBook = new BookBuilder().Id(2).Quantity(0).Build();

            var shoppingCart = new ShoppingCartBuilder()
                .WithShoppingCartItem(inStockBook, 1)
                .WithShoppingCartItem(outOfStockBook, 1)
                .Build();

            var skipped = shoppingCart.GetOutOfStockWantedItems();

            Assert.Equal(outOfStockBook.Id, Assert.Single(skipped).BookId);
        }

        [Fact]
        public void GetOutOfStockWantedItems_IgnoresWishListItems_When_Executed()
        {
            var outOfStockWishListBook = new BookBuilder().Id(1).Quantity(0).Build();

            var shoppingCart = new ShoppingCartBuilder()
                .WithWishListItem(outOfStockWishListBook)
                .Build();

            Assert.Empty(shoppingCart.GetOutOfStockWantedItems());
        }
    }
}
