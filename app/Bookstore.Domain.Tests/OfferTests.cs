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
        public void Approve_TransitionsToApproved_When_TheOfferIsPendingApproval()
        {
            var offer = new OfferBuilder().Build();

            offer.Approve();

            Assert.Equal(OfferStatus.Approved, offer.OfferStatus);
        }

        [Theory]
        [InlineData(OfferStatus.Approved)]
        [InlineData(OfferStatus.Received)]
        [InlineData(OfferStatus.Paid)]
        [InlineData(OfferStatus.Rejected)]
        public void Approve_Throws_When_TheOfferIsNotPendingApproval(OfferStatus status)
        {
            var offer = OfferInState(status);

            Assert.Throws<DomainException>(() => offer.Approve());
        }

        [Theory]
        [InlineData(OfferStatus.PendingApproval)]
        [InlineData(OfferStatus.Received)]
        public void Reject_TransitionsToRejected_When_TheOfferIsPendingApprovalOrReceived(OfferStatus status)
        {
            var offer = OfferInState(status);

            offer.Reject();

            Assert.Equal(OfferStatus.Rejected, offer.OfferStatus);
        }

        [Theory]
        [InlineData(OfferStatus.Approved)]
        [InlineData(OfferStatus.Paid)]
        [InlineData(OfferStatus.Rejected)]
        public void Reject_Throws_When_TheOfferIsApprovedPaidOrAlreadyRejected(OfferStatus status)
        {
            var offer = OfferInState(status);

            Assert.Throws<DomainException>(() => offer.Reject());
        }

        [Fact]
        public void ConfirmReceipt_TransitionsToReceived_When_TheOfferIsApproved()
        {
            var offer = OfferInState(OfferStatus.Approved);

            offer.ConfirmReceipt();

            Assert.Equal(OfferStatus.Received, offer.OfferStatus);
        }

        [Theory]
        [InlineData(OfferStatus.PendingApproval)]
        [InlineData(OfferStatus.Received)]
        [InlineData(OfferStatus.Paid)]
        [InlineData(OfferStatus.Rejected)]
        public void ConfirmReceipt_Throws_When_TheOfferIsNotApproved(OfferStatus status)
        {
            var offer = OfferInState(status);

            Assert.Throws<DomainException>(() => offer.ConfirmReceipt());
        }

        [Fact]
        public void RecordPayment_TransitionsToPaid_When_TheOfferIsReceived()
        {
            var offer = OfferInState(OfferStatus.Received);

            offer.RecordPayment(OfferBuilder.PaidOn);

            Assert.Equal(OfferStatus.Paid, offer.OfferStatus);
        }

        // The buying half of the monetary indicators is dated by this, so the offer has to keep it.
        [Fact]
        public void RecordPayment_RecordsWhenTheStorePaid_When_TheOfferIsReceived()
        {
            var offer = OfferInState(OfferStatus.Received);

            Assert.Null(offer.PaidOn);

            offer.RecordPayment(OfferBuilder.PaidOn);

            Assert.Equal(OfferBuilder.PaidOn, offer.PaidOn);
        }

        [Theory]
        [InlineData(OfferStatus.PendingApproval)]
        [InlineData(OfferStatus.Approved)]
        [InlineData(OfferStatus.Paid)]
        [InlineData(OfferStatus.Rejected)]
        public void RecordPayment_Throws_When_TheOfferIsNotReceived(OfferStatus status)
        {
            var offer = OfferInState(status);

            Assert.Throws<DomainException>(() => offer.RecordPayment(OfferBuilder.PaidOn));
        }

        private static Offer OfferInState(OfferStatus status) => new OfferBuilder().Status(status).Build();
    }
}
