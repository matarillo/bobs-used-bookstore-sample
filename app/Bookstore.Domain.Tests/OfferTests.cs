using Bookstore.Domain.Offers;
using Bookstore.Domain.Tests.Builders;

namespace Bookstore.Domain.Tests
{
    public class OfferTests
    {
        [Fact]
        public void OfferStatus_IsPendingApproval_When_TheOfferIsCreated()
        {
            var offer = new OfferBuilder().Build();

            Assert.Equal(OfferStatus.PendingApproval, offer.OfferStatus);
        }

        [Fact]
        public void OfferStatus_CanBeSetToAnyValue_When_AssignedDirectly()
        {
            // INV-OFFER-02/03/04 are not enforced: payment can be recorded without ever
            // confirming receipt of the book. Recorded here so the change is visible when
            // ISSUE-15 replaces the setter with guarded behaviours.
            var offer = new OfferBuilder().Build();

            offer.OfferStatus = OfferStatus.Paid;
            offer.OfferStatus = OfferStatus.PendingApproval;

            Assert.Equal(OfferStatus.PendingApproval, offer.OfferStatus);
        }
    }
}
