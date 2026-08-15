using Bookstore.Domain.Books;
using Bookstore.Domain.Carts;
using Bookstore.Domain.Offers;
using Bookstore.Domain.Tests.Builders;

namespace Bookstore.Domain.Tests
{
    // ISSUE-07: money and quantity used to be plain numbers, so nothing stopped a negative price,
    // a negative stock level or a line of zero copies, and no rounding rule applied to an amount.
    // Every test here previously pinned the opposite of what it now asserts — this is a change of
    // specification, not a fix to a broken implementation.
    public class ValueSemanticsTests
    {
        [Fact]
        public void ABookCannotBeCreatedWithANegativePrice()
        {
            Assert.Throws<DomainException>(() => new BookBuilder().Price(-5m).Build());
        }

        [Fact]
        public void ABookCannotBeCreatedWithANegativeStockLevel()
        {
            Assert.Throws<DomainException>(() => new BookBuilder().Quantity(-3).Build());
        }

        [Fact]
        public void ABookCanBeCreatedWithNoStock()
        {
            var book = new BookBuilder().Quantity(0).Build();

            Assert.Equal(Quantity.None, book.Quantity);
            Assert.False(book.IsInStock);
        }

        [Fact]
        public void AnOfferCannotBeMadeAtANegativePrice()
        {
            Assert.Throws<DomainException>(() => new OfferBuilder().BookPrice(-5m).Build());
        }

        // INV-CART-05: the value rules out a negative count, the cart rules out an empty line.
        [Fact]
        public void ACartLineCannotBeAddedWithAQuantityOfZero()
        {
            var cart = new ShoppingCart("correlation-id");

            Assert.Throws<DomainException>(() => cart.AddItemToShoppingCart(bookId: 1, Quantity.None));
        }

        [Fact]
        public void AQuantityCannotBeNegative()
        {
            Assert.Throws<DomainException>(() => Quantity.Of(-2));
        }

        // The boundary between the outside world and the model. A form field that arrives as zero
        // is rejected where it is turned into a value, not several layers later.
        [Fact]
        public void AQuantityForALineMustBeAtLeastOne()
        {
            Assert.Throws<DomainException>(() => Quantity.OfAtLeastOne(0));
            Assert.Equal(1, Quantity.OfAtLeastOne(1).Value);
        }

        // An amount is rounded to the currency as it is created, so no amount can be a fraction
        // of a cent and no total can accumulate one.
        [Fact]
        public void AnAmountIsRoundedToTheCurrency()
        {
            var book = new BookBuilder().Id(1).Price(0.005m).Build();
            var cart = new ShoppingCartBuilder().WithShoppingCartItem(book, 3).Build();

            Assert.Equal(Money.Of(0.01m), book.Price);
            Assert.Equal(Money.Of(0.03m), cart.GetSubTotal(ShoppingCartItemFilter.IncludeOutOfStockItems));
        }

        [Fact]
        public void TaxIsRoundedToTheCurrency()
        {
            var book = new BookBuilder().Id(1).Price(0.99m).Build();
            var order = new Orders.Order(customerId: 1, addressId: 1);

            order.AddOrderItem(book, Quantity.One);

            Assert.Equal(Money.Of(0.10m), order.Tax);
            Assert.Equal(Money.Of(1.09m), order.Total);
        }

        // A margin is a difference of two amounts, not an amount: the store can sell for less
        // than it paid, and that has to be expressible.
        [Fact]
        public void AMarginCanBeNegative()
        {
            var offer = new OfferBuilder().Id(1).BookPrice(20m).Status(OfferStatus.Paid).Build();

            var book = Book.CreateFromOffer(offer, price: Money.Of(5m));

            Assert.Equal(-15m, book.Margin);
        }
    }
}
