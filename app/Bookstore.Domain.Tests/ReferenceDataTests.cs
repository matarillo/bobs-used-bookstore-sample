using Bookstore.Domain.ReferenceData;

namespace Bookstore.Domain.Tests
{
    // ISSUE-05: four classification axes are represented by one concept, so nothing checks that
    // the item put in the genre position is a genre, and an item's type can be changed after
    // books and offers are already pointing at it (INV-BOOK-05, INV-OFFER-07, INV-REFDATA-03).
    // Pinned here before that changes.
    public class ReferenceDataTests
    {
        [Fact]
        public void TheTypeOfAnItemCanBeChangedAfterItIsCreated()
        {
            var item = new ReferenceDataItem(ReferenceDataType.Genre, "History");

            item.DataType = ReferenceDataType.Publisher;

            Assert.Equal(ReferenceDataType.Publisher, item.DataType);
        }
    }
}
