using Bookstore.Domain.Customers;
using Bookstore.Domain.ReferenceData;

namespace Bookstore.Domain.Offers
{
    public class Offer : Entity
    {
        // An empty constructor is required by EF Core, which can no longer bind the constructor
        // below now that the buying price is a value rather than the mapped column.
#pragma warning disable CS8618 // Non-nullable property must contain a non-null value when exiting constructor.
        private Offer() { }
#pragma warning restore CS8618

        public Offer(
            int customerId,
            string bookName,
            string author,
            string ISBN,
            BookClassification classification,
            Money bookPrice)
        {
            CustomerId = customerId;
            BookName = bookName;
            Author = author;
            this.ISBN = ISBN;
            Classification = classification;
            BookPrice = bookPrice;
        }

        public string Author { get; set; }

        public string ISBN { get; set; }

        public string BookName { get; set; }

        public string? FrontUrl { get; set; }

        // Navigation properties populated by EF Core when an offer is loaded; the private
        // constructor above intentionally leaves them for EF to fix up.
        public ReferenceDataItem Genre { get; set; } = null!;
        public int GenreId { get; private set; }

        public ReferenceDataItem Condition { get; set; } = null!;
        public int ConditionId { get; private set; }

        public ReferenceDataItem Publisher { get; set; } = null!;
        public int PublisherId { get; private set; }

        public ReferenceDataItem BookType { get; set; } = null!;
        public int BookTypeId { get; private set; }

        // ISSUE-05: as on Book — the four identifiers are only ever set through a classification
        // checked against the reference data, so INV-OFFER-07 is now enforced rather than assumed.
        public BookClassification Classification
        {
            get => BookClassification.AlreadyChecked(PublisherId, BookTypeId, GenreId, ConditionId);
            private set
            {
                PublisherId = value.PublisherId;
                BookTypeId = value.BookTypeId;
                GenreId = value.GenreId;
                ConditionId = value.ConditionId;
            }
        }

        public string? Summary { get; set; }

        // INV-OFFER-02/03/04, revised by ISSUE-15: the status only moves through the behaviours
        // below, which each check the current state before transitioning.
        public OfferStatus OfferStatus { get; private set; } = OfferStatus.PendingApproval;

        public string? Comment { get; set; }

        public Customer Customer { get; set; } = null!;
        public int CustomerId { get; set; }

        public Money BookPrice { get; set; }

        // ISSUE-07: the column behind BookPrice — see Book for why it exists.
        internal decimal BookPriceAmount
        {
            get => BookPrice.Amount;
            private set => BookPrice = Money.Of(value);
        }

        // When the store paid the customer, and so when the money left the business. The buying
        // side of the monetary indicators (12 §2.4) is dated by this, not by when the offer was
        // made — an offer made in one month and paid in the next is spending in the second.
        public DateTime? PaidOn { get; private set; }

        // ISSUE-06: whether the bought book has been put on the shelf as a Book. A paid offer is
        // stocked exactly once; Book.CreateFromOffer is the only way this becomes true.
        public bool IsStocked { get; private set; }

        // The store approves a pending offer and awaits the shipment from the customer.
        public void Approve()
        {
            RequireStatus(OfferStatus.PendingApproval, "be approved");

            OfferStatus = OfferStatus.Approved;
        }

        // The store rejects an offer, either up front or after finding the shipped book does
        // not match what was declared.
        public void Reject()
        {
            if (OfferStatus != OfferStatus.PendingApproval && OfferStatus != OfferStatus.Received)
            {
                throw new DomainException($"Offer {Id} cannot be rejected from the \"{OfferStatus}\" state.");
            }

            OfferStatus = OfferStatus.Rejected;
        }

        // The store confirms the customer's shipment has arrived.
        public void ConfirmReceipt()
        {
            RequireStatus(OfferStatus.Approved, "be marked as received");

            OfferStatus = OfferStatus.Received;
        }

        // INV-OFFER-04: the store pays the customer only after receipt has been confirmed.
        // The date is passed in rather than read here so that what the store spent in a period is
        // a fact about the offer, not about when a report happens to run.
        public void RecordPayment(DateTime paidOnUtc)
        {
            RequireStatus(OfferStatus.Received, "be paid");

            OfferStatus = OfferStatus.Paid;
            PaidOn = paidOnUtc;
        }

        // ISSUE-06: the buying side of the business hands the book over to the selling side. Only
        // a paid offer may be stocked — the store does not sell what it has not yet bought — and
        // only once, so a single bought copy cannot become two books. Called by
        // Book.CreateFromOffer, which is what actually produces the stock.
        internal void MarkAsStocked()
        {
            RequireStatus(OfferStatus.Paid, "be added to inventory");

            if (IsStocked)
            {
                throw new DomainException($"Offer {Id} has already been added to inventory.");
            }

            IsStocked = true;
        }

        private void RequireStatus(OfferStatus required, string action)
        {
            if (OfferStatus != required)
            {
                throw new DomainException($"Offer {Id} cannot {action} from the \"{OfferStatus}\" state.");
            }
        }
    }
}