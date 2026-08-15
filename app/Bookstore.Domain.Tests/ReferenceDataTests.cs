using Bookstore.Domain.ReferenceData;
using Bookstore.Domain.Tests.Builders;

namespace Bookstore.Domain.Tests
{
    // ISSUE-05: four classification axes were represented by one concept, so nothing checked that
    // the item put in the genre position was a genre, and an item's type could be changed after
    // books and offers were already filed under it. INV-BOOK-05, INV-OFFER-07 and INV-REFDATA-03
    // are now enforced rather than assumed.
    public class ReferenceDataTests
    {
        [Fact]
        public void TheTypeOfAnItemIsSettledWhenItIsCreated()
        {
            var item = new ReferenceDataItem(ReferenceDataType.Genre, "History");

            item.Rename("Modern History");

            Assert.Equal(ReferenceDataType.Genre, item.DataType);
            Assert.Equal("Modern History", item.Text);
        }

        [Fact]
        public void AClassificationAcceptsAnItemOfTheTypeItsPositionRequires()
        {
            var classification = TestReferenceData.Classification();

            Assert.Equal(TestReferenceData.PublisherId, classification.PublisherId);
            Assert.Equal(TestReferenceData.BookTypeId, classification.BookTypeId);
            Assert.Equal(TestReferenceData.GenreId, classification.GenreId);
            Assert.Equal(TestReferenceData.ConditionId, classification.ConditionId);
        }

        // A publisher in the genre position used to be stored without complaint.
        [Fact]
        public void AClassificationRejectsAnItemOfTheWrongType()
        {
            var exception = Assert.Throws<DomainException>(
                () => TestReferenceData.Classification(genreId: TestReferenceData.PublisherId));

            Assert.Contains("Publisher", exception.Message);
            Assert.Contains("Genre", exception.Message);
        }

        [Fact]
        public void AClassificationRejectsAnItemThatDoesNotExist()
        {
            Assert.Throws<DomainException>(() => TestReferenceData.Classification(conditionId: 99));
        }

        // ISSUE-06: an offer that is stocked as a book carries its classification across, and it
        // was checked when the offer was made.
        [Fact]
        public void AStockedOfferKeepsTheClassificationItWasMadeWith()
        {
            var offer = new OfferBuilder().Status(Offers.OfferStatus.Paid).Build();

            var book = Books.Book.CreateFromOffer(offer, Money.Of(10m));

            Assert.Equal(offer.Classification, book.Classification);
        }
    }
}
