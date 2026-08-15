using Bookstore.Domain.Offers;
using Bookstore.Domain.ReferenceData;

namespace Bookstore.Domain.Books
{
    public class Book : Entity
    {
        public const int LowBookThreshold = 5;

        // ISSUE-06: an offer is one physical second-hand copy, so stocking it yields one book.
        private static readonly Quantity StockedFromOfferQuantity = Quantity.One;

        // An empty constructor is required by EF Core, which can no longer bind the constructor
        // below now that its price and quantity are values rather than the mapped columns.
#pragma warning disable CS8618 // Non-nullable property must contain a non-null value when exiting constructor.
        private Book() { }
#pragma warning restore CS8618

        public Book(
            string name,
            string author,
            string ISBN,
            BookClassification classification,
            Money price,
            Quantity quantity,
            int? year = null,
            string? summary = null,
            string? coverImageUrl = null)
        {
            Name = name;
            Author = author;
            this.ISBN = ISBN;
            Classification = classification;
            Price = price;
            Quantity = quantity;
            Year = year;
            Summary = summary;
            CoverImageUrl = coverImageUrl;
        }

        // ISSUE-06: the route from buying to selling, which the domain previously lacked. The
        // offer's description of the book carries over unchanged; the sale price is the store's
        // decision and is not derived from what the store paid. Marking the offer as stocked is
        // part of this operation, so a book can only ever come from a paid, not-yet-stocked offer.
        public static Book CreateFromOffer(Offer offer, Money price, int? year = null, string? summary = null)
        {
            offer.MarkAsStocked();

            return new Book(
                offer.BookName,
                offer.Author,
                offer.ISBN,
                offer.Classification,
                price,
                StockedFromOfferQuantity,
                year,
                summary)
            {
                SourceOfferId = offer.Id,
                PurchaseCost = offer.BookPrice
            };
        }

        public string Name { get; set; }

        public string Author { get; set; }

        public int? Year { get; set; }

        public string ISBN { get; set; }

        public ReferenceDataItem Publisher { get; set; }
        public int PublisherId { get; private set; }

        public ReferenceDataItem BookType { get; set; }
        public int BookTypeId { get; private set; }

        public ReferenceDataItem Genre { get; set; }
        public int GenreId { get; private set; }

        public ReferenceDataItem Condition { get; set; }
        public int ConditionId { get; private set; }

        // ISSUE-05: the four identifiers above are the foreign keys, and they are only ever set
        // through a classification that has been checked against the reference data. INV-BOOK-05
        // used to hold by convention alone.
        public BookClassification Classification
        {
            get => BookClassification.AlreadyChecked(PublisherId, BookTypeId, GenreId, ConditionId);
            set
            {
                PublisherId = value.PublisherId;
                BookTypeId = value.BookTypeId;
                GenreId = value.GenreId;
                ConditionId = value.ConditionId;
            }
        }

        public string? CoverImageUrl { get; set; }

        public string? Summary { get; set; }

        public Money Price { get; set; }

        public Quantity Quantity { get; set; }

        // ISSUE-06: where this book came from. Null for a book the store entered by hand, whose
        // source is unknown to the domain.
        public int? SourceOfferId { get; private set; }

        // ISSUE-06: what the store paid for this book, copied from the offer when it was stocked.
        // Held as a value rather than read through the offer for the same reason OrderItem.Price
        // is (ISSUE-17): the cost of a book is settled once and must not follow later changes.
        // Null whenever SourceOfferId is null.
        public Money? PurchaseCost { get; private set; }

        // ISSUE-07: the columns behind the values above. The write model works in Money and
        // Quantity; the read side (sorting, filters and the statistics of 12 §2) needs plain
        // numbers it can hand to the database, so those live here and stay internal to the
        // domain and its persistence.
        internal decimal PriceAmount
        {
            get => Price.Amount;
            private set => Price = Money.Of(value);
        }

        internal int StockQuantity
        {
            get => Quantity.Value;
            private set => Quantity = Quantity.Of(value);
        }

        internal decimal? PurchaseCostAmount
        {
            get => PurchaseCost?.Amount;
            private set => PurchaseCost = Money.OfNullable(value);
        }

        // ISSUE-06: what the store stands to make on a copy of this book — the point of buying
        // low and selling high. Null when the book was not sourced through an offer, because then
        // the domain does not know what it cost. A decimal rather than a Money because it is a
        // difference of two amounts, and a difference can be negative (see Money.Less).
        public decimal? Margin => PurchaseCost.HasValue ? Price.Less(PurchaseCost.Value) : null;

        public bool IsInStock => !Quantity.IsNone;

        public bool IsLowInStock => Quantity.Value <= LowBookThreshold;

        // INV-BOOK-01, revised by ISSUE-03: a withdrawal that would take the stock level below
        // zero is rejected instead of being silently saturated at zero, so a shortage is never
        // hidden behind a stock level that only ever looks non-negative.
        public void ReduceStockLevel(Quantity quantity)
        {
            if (quantity > Quantity)
            {
                throw new DomainException($"Cannot withdraw {quantity} of \"{Name}\"; only {Quantity} in stock.");
            }

            Quantity -= quantity;
        }

        // RULE-ORDER-05, revised by ISSUE-16: the counterpart of ReduceStockLevel, used to
        // return stock that a cancelled order had withdrawn.
        public void RestoreStockLevel(Quantity quantity)
        {
            Quantity += quantity;
        }
    }
}