using Bookstore.Domain.Books;
using Bookstore.Domain.Offers;
using Bookstore.Domain.Tests.Builders;

namespace Bookstore.Domain.Tests
{
    public class BookTests
    {
        [Theory]
        [InlineData(1, true)]
        [InlineData(0, false)]
        public void IsInStock_ReturnsTrue_When_QuantityIsGreaterThanZero(int quantity, bool expectedResult)
        {
            var book = new BookBuilder()
                .Quantity(quantity)
                .Build();
            
            Assert.Equal(expectedResult, book.IsInStock);
        }

        [Theory]
        [InlineData(Book.LowBookThreshold - 1, true)]
        [InlineData(Book.LowBookThreshold, true)]
        [InlineData(Book.LowBookThreshold + 1, false)]
        public void IsLowInStock_ReturnsTrue_When_QuantityIsLessThanOrEqualToThreshold(int quantity, bool expectedResult)
        {
            var book = new BookBuilder()
                .Quantity(quantity)
                .Build();
            
            Assert.Equal(expectedResult, book.IsLowInStock);
        }

        [Fact]
        public void ReduceStockLevel_ReducesQuantityBySpecifiedAmount_When_Executed()
        {
            var book = new BookBuilder()
                .Quantity(100)
                .Build();
            
            const int amountToReduce = 50;
            var expectedQuantity = book.Quantity - Quantity.Of(amountToReduce);

            book.ReduceStockLevel(Quantity.Of(amountToReduce));

            Assert.Equal(expectedQuantity, book.Quantity);
        }

        [Fact]
        public void ReduceStockLevel_Throws_When_QuantityExceedsStock()
        {
            var book = new BookBuilder()
                .Quantity(50)
                .Build();

            const int amountToReduce = 60;

            Assert.Throws<DomainException>(() => book.ReduceStockLevel(Quantity.Of(amountToReduce)));
            Assert.Equal(Quantity.Of(50), book.Quantity);
        }

        [Fact]
        public void RestoreStockLevel_IncreasesQuantityBySpecifiedAmount_When_Executed()
        {
            var book = new BookBuilder()
                .Quantity(50)
                .Build();

            const int amountToRestore = 3;

            book.RestoreStockLevel(Quantity.Of(amountToRestore));

            Assert.Equal(Quantity.Of(53), book.Quantity);
        }

        // The offer describes the book, so stocking it must carry that description over
        // unchanged. What the store sells it for is a separate decision.
        [Fact]
        public void CreateFromOffer_CopiesTheOfferDescriptionOfTheBook_When_TheOfferIsPaid()
        {
            var offer = PaidOffer();

            var book = Book.CreateFromOffer(offer, price: Money.Of(20m));

            Assert.Equal(offer.BookName, book.Name);
            Assert.Equal(offer.Author, book.Author);
            Assert.Equal(offer.ISBN, book.ISBN);
            Assert.Equal(offer.BookTypeId, book.BookTypeId);
            Assert.Equal(offer.ConditionId, book.ConditionId);
            Assert.Equal(offer.GenreId, book.GenreId);
            Assert.Equal(offer.PublisherId, book.PublisherId);
        }

        [Fact]
        public void CreateFromOffer_StocksOneCopyAtThePriceTheStoreChose()
        {
            var offer = PaidOffer(bookPrice: 5m);

            var book = Book.CreateFromOffer(offer, price: Money.Of(20m));

            Assert.Equal(Money.Of(20m), book.Price);
            Assert.Equal(Quantity.Of(1), book.Quantity);
        }

        // The source of the stock and what it cost, which is what makes a margin calculable at
        // all.
        [Fact]
        public void CreateFromOffer_RecordsTheOfferAsTheSourceAndItsPriceAsTheCost()
        {
            var offer = PaidOffer(bookPrice: 5m, id: 7);

            var book = Book.CreateFromOffer(offer, price: Money.Of(20m));

            Assert.Equal(7, book.SourceOfferId);
            Assert.Equal(Money.Of(5m), book.PurchaseCost);
            Assert.Equal(15m, book.Margin);
        }

        [Fact]
        public void CreateFromOffer_MarksTheOfferAsStocked()
        {
            var offer = PaidOffer();

            Assert.False(offer.IsStocked);

            Book.CreateFromOffer(offer, price: Money.Of(20m));

            Assert.True(offer.IsStocked);
        }

        // The store does not shelve what it has not yet bought and paid for.
        [Theory]
        [InlineData(OfferStatus.PendingApproval)]
        [InlineData(OfferStatus.Approved)]
        [InlineData(OfferStatus.Received)]
        [InlineData(OfferStatus.Rejected)]
        public void CreateFromOffer_Throws_When_TheOfferHasNotBeenPaid(OfferStatus status)
        {
            var offer = new OfferBuilder().Status(status).Build();

            Assert.Throws<DomainException>(() => Book.CreateFromOffer(offer, price: Money.Of(20m)));
            Assert.False(offer.IsStocked);
        }

        // One bought copy is one book. Stocking the same offer twice would invent stock.
        [Fact]
        public void CreateFromOffer_Throws_When_TheOfferHasAlreadyBeenStocked()
        {
            var offer = PaidOffer();

            Book.CreateFromOffer(offer, price: Money.Of(20m));

            Assert.Throws<DomainException>(() => Book.CreateFromOffer(offer, price: Money.Of(20m)));
        }

        [Fact]
        public void Margin_IsNull_When_TheBookWasNotSourcedFromAnOffer()
        {
            var book = new BookBuilder().Build();

            Assert.Null(book.SourceOfferId);
            Assert.Null(book.PurchaseCost);
            Assert.Null(book.Margin);
        }

        private static Offer PaidOffer(decimal bookPrice = 5m, int id = 1) =>
            new OfferBuilder().Id(id).BookPrice(bookPrice).Status(OfferStatus.Paid).Build();
    }
}
