using Bookstore.Domain.Offers;
using Bookstore.Domain.ReferenceData;

namespace Bookstore.Domain.Books
{
    public class Book : Entity
    {
        public const int LowBookThreshold = 5;

        // ISSUE-06: an offer is one physical second-hand copy, so stocking it yields one book.
        private const int StockedFromOfferQuantity = 1;

        public Book(
            string name, 
            string author, 
            string ISBN, 
            int publisherId, 
            int bookTypeId, 
            int genreId,
            int conditionId,
            decimal price,
            int quantity, 
            int? year = null,
            string? summary = null,
            string? coverImageUrl = null)
        {
            Name = name;
            Author = author;
            this.ISBN = ISBN;
            PublisherId = publisherId;
            BookTypeId = bookTypeId;
            GenreId = genreId;
            ConditionId = conditionId;
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
        public static Book CreateFromOffer(Offer offer, decimal price, int? year = null, string? summary = null)
        {
            offer.MarkAsStocked();

            return new Book(
                offer.BookName,
                offer.Author,
                offer.ISBN,
                offer.PublisherId,
                offer.BookTypeId,
                offer.GenreId,
                offer.ConditionId,
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
        public int PublisherId { get; set; }

        public ReferenceDataItem BookType { get; set; }
        public int BookTypeId { get; set; }

        public ReferenceDataItem Genre { get; set; }
        public int GenreId { get; set; }

        public ReferenceDataItem Condition { get; set; }
        public int ConditionId { get; set; }

        public string? CoverImageUrl { get; set; }

        public string? Summary { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        // ISSUE-06: where this book came from. Null for a book the store entered by hand, whose
        // source is unknown to the domain.
        public int? SourceOfferId { get; private set; }

        // ISSUE-06: what the store paid for this book, copied from the offer when it was stocked.
        // Held as a value rather than read through the offer for the same reason OrderItem.Price
        // is (ISSUE-17): the cost of a book is settled once and must not follow later changes.
        // Null whenever SourceOfferId is null.
        public decimal? PurchaseCost { get; private set; }

        // ISSUE-06: what the store stands to make on a copy of this book — the point of buying
        // low and selling high. Null when the book was not sourced through an offer, because then
        // the domain does not know what it cost.
        public decimal? Margin => PurchaseCost.HasValue ? Price - PurchaseCost.Value : null;

        public bool IsInStock => Quantity > 0;

        public bool IsLowInStock => Quantity <= LowBookThreshold;

        // INV-BOOK-01, revised by ISSUE-03: a withdrawal that would take the stock level below
        // zero is rejected instead of being silently saturated at zero, so a shortage is never
        // hidden behind a stock level that only ever looks non-negative.
        public void ReduceStockLevel(int quantity)
        {
            if (quantity > Quantity)
            {
                throw new DomainException($"Cannot withdraw {quantity} of \"{Name}\"; only {Quantity} in stock.");
            }

            Quantity -= quantity;
        }

        // RULE-ORDER-05, revised by ISSUE-16: the counterpart of ReduceStockLevel, used to
        // return stock that a cancelled order had withdrawn.
        public void RestoreStockLevel(int quantity)
        {
            Quantity += quantity;
        }
    }
}