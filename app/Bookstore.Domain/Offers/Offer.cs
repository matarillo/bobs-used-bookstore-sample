using Bookstore.Domain.Customers;
using Bookstore.Domain.ReferenceData;

namespace Bookstore.Domain.Offers
{
    public class Offer : Entity
    {
        public Offer(
            int customerId,
            string bookName,
            string author,
            string ISBN,
            int bookTypeId,
            int conditionId,
            int genreId,
            int publisherId,
            decimal bookPrice)
        {
            CustomerId = customerId;
            BookName = bookName;
            Author = author;
            this.ISBN = ISBN;
            BookTypeId = bookTypeId;
            ConditionId = conditionId;
            GenreId = genreId;
            PublisherId = publisherId;
            BookPrice = bookPrice;
        }

        public string Author { get; set; }

        public string ISBN { get; set; }

        public string BookName { get; set; }

        public string? FrontUrl { get; set; }

        public ReferenceDataItem Genre { get; set; }
        public int GenreId { get; set; }

        public ReferenceDataItem Condition { get; set; }
        public int ConditionId { get; set; }

        public ReferenceDataItem Publisher { get; set; }
        public int PublisherId { get; set; }

        public ReferenceDataItem BookType { get; set; }
        public int BookTypeId { get; set; }

        public string? Summary { get; set; }

        // INV-OFFER-02/03/04, revised by ISSUE-15: the status only moves through the behaviours
        // below, which each check the current state before transitioning.
        public OfferStatus OfferStatus { get; private set; } = OfferStatus.PendingApproval;

        public string? Comment { get; set; }

        public Customer Customer { get; set; }
        public int CustomerId { get; set; }

        public decimal BookPrice { get; set; }

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
        public void RecordPayment()
        {
            RequireStatus(OfferStatus.Received, "be paid");

            OfferStatus = OfferStatus.Paid;
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