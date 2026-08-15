namespace Bookstore.Domain.ReferenceData
{
    // ISSUE-05: the four axes a book (or an offer of one) is classified along, checked to be what
    // they claim to be.
    //
    // All four are drawn from one pool of reference data items, so a publisher's identifier fits
    // the genre position perfectly well as far as the compiler and the database are concerned.
    // Nothing rejected it, and INV-BOOK-05 and INV-OFFER-07 were invariants in name only.
    //
    // The aggregates still store the four identifiers — they are the foreign keys — but they can
    // only be handed a classification, and a classification can only be made by looking the items
    // up and finding each is of the type its position requires.
    public readonly record struct BookClassification
    {
        private BookClassification(int publisherId, int bookTypeId, int genreId, int conditionId)
        {
            PublisherId = publisherId;
            BookTypeId = bookTypeId;
            GenreId = genreId;
            ConditionId = conditionId;
        }

        public int PublisherId { get; }

        public int BookTypeId { get; }

        public int GenreId { get; }

        public int ConditionId { get; }

        // The way in from the outside: four identifiers chosen on a form, checked against the
        // reference data they were chosen from.
        public static BookClassification Of(
            IEnumerable<ReferenceDataItem> referenceData,
            int publisherId,
            int bookTypeId,
            int genreId,
            int conditionId)
        {
            var items = referenceData.ToDictionary(x => x.Id);

            Require(items, publisherId, ReferenceDataType.Publisher);
            Require(items, bookTypeId, ReferenceDataType.BookType);
            Require(items, genreId, ReferenceDataType.Genre);
            Require(items, conditionId, ReferenceDataType.Condition);

            return new BookClassification(publisherId, bookTypeId, genreId, conditionId);
        }

        // The way across from something already classified — an offer being stocked as a book
        // (ISSUE-06). Its classification was checked when the offer was made, and the book that
        // comes out of it is the same book.
        internal static BookClassification AlreadyChecked(int publisherId, int bookTypeId, int genreId, int conditionId) =>
            new(publisherId, bookTypeId, genreId, conditionId);

        private static void Require(IReadOnlyDictionary<int, ReferenceDataItem> items, int id, ReferenceDataType expected)
        {
            if (!items.TryGetValue(id, out var item))
            {
                throw new DomainException($"Reference data item {id} was not found; a {expected} is required.");
            }

            if (item.DataType != expected)
            {
                throw new DomainException($"\"{item.Text}\" is a {item.DataType}, not a {expected}.");
            }
        }
    }
}
