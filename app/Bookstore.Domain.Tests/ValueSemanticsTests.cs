using Bookstore.Domain.Books;
using Bookstore.Domain.Carts;
using Bookstore.Domain.Offers;
using Bookstore.Domain.Tests.Builders;

namespace Bookstore.Domain.Tests
{
    // ISSUE-07: money and quantity are plain numbers today, so nothing stops a negative price,
    // a negative stock level or a quantity of zero, and no rounding rule applies to an amount.
    // These tests pin that behaviour before it is replaced by Money and Quantity, so the change
    // of specification is visible in the diff rather than implied by it.
    public class ValueSemanticsTests
    {
        [Fact]
        public void ABookCanBeCreatedWithANegativePrice()
        {
            var book = new BookBuilder().Price(-5m).Build();

            Assert.Equal(-5m, book.Price);
        }

        [Fact]
        public void ABookCanBeCreatedWithANegativeStockLevel()
        {
            var book = new BookBuilder().Quantity(-3).Build();

            Assert.Equal(-3, book.Quantity);
            Assert.False(book.IsInStock);
        }

        [Fact]
        public void AnOfferCanBeMadeAtANegativePrice()
        {
            var offer = new OfferBuilder().BookPrice(-5m).Build();

            Assert.Equal(-5m, offer.BookPrice);
        }

        [Fact]
        public void ACartLineCanBeAddedWithAQuantityOfZero()
        {
            var book = new BookBuilder().Id(1).Price(10m).Build();
            var cart = new ShoppingCartBuilder().WithShoppingCartItem(book, 0).Build();

            Assert.Equal(0, cart.ShoppingCartItems.Single().Quantity);
            Assert.Equal(0m, cart.GetSubTotal(ShoppingCartItemFilter.IncludeOutOfStockItems));
        }

        [Fact]
        public void ACartLineCanBeAddedWithANegativeQuantity()
        {
            var book = new BookBuilder().Id(1).Price(10m).Build();
            var cart = new ShoppingCartBuilder().WithShoppingCartItem(book, -2).Build();

            Assert.Equal(-20m, cart.GetSubTotal(ShoppingCartItemFilter.IncludeOutOfStockItems));
        }

        // No rounding rule applies, so a price carries as many decimal places as it was given and
        // the subtotal carries the product of them.
        [Fact]
        public void AnAmountKeepsMoreDecimalPlacesThanACurrencyHas()
        {
            var book = new BookBuilder().Id(1).Price(0.005m).Build();
            var cart = new ShoppingCartBuilder().WithShoppingCartItem(book, 3).Build();

            Assert.Equal(0.015m, cart.GetSubTotal(ShoppingCartItemFilter.IncludeOutOfStockItems));
        }

        // Tax is a tenth of the subtotal with no rounding, so an order can be charged a fraction
        // of a cent.
        [Fact]
        public void TaxIsNotRoundedToTheCurrency()
        {
            var book = new BookBuilder().Id(1).Price(0.99m).Build();
            var order = new Orders.Order(customerId: 1, addressId: 1);

            order.AddOrderItem(book, 1);

            Assert.Equal(0.099m, order.Tax);
            Assert.Equal(1.089m, order.Total);
        }
    }
}
