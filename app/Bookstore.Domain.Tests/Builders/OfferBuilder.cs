using Bookstore.Domain.Offers;

namespace Bookstore.Domain.Tests.Builders;

public class OfferBuilder
{
    private int id;
    private int customerId = 1;
    private string bookName = "test";
    private string author = "author";
    private string isbn = "12345678";
    private int bookTypeId = TestReferenceData.BookTypeId;
    private int conditionId = TestReferenceData.ConditionId;
    private int genreId = TestReferenceData.GenreId;
    private int publisherId = TestReferenceData.PublisherId;
    private decimal bookPrice = 5;
    private OfferStatus status = OfferStatus.PendingApproval;

    public Offer Build()
    {
        var offer = new Offer(customerId, bookName, author, isbn, TestReferenceData.Classification(publisherId, bookTypeId, genreId, conditionId), Money.Of(bookPrice))
        {
            Id = id
        };

        DriveTo(offer, status);

        return offer;
    }

    // Drives the offer through the transitions needed to reach the requested state, so a test can
    // start from an arbitrary point without relying on a raw status setter.
    private static void DriveTo(Offer offer, OfferStatus status)
    {
        if (status == OfferStatus.PendingApproval) return;

        if (status == OfferStatus.Rejected)
        {
            offer.Reject();
            return;
        }

        offer.Approve();
        if (status == OfferStatus.Approved) return;

        offer.ConfirmReceipt();
        if (status == OfferStatus.Received) return;

        offer.RecordPayment(PaidOn);
    }

    // A fixed payment date, so a test that cares about when the store paid can assert on it.
    public static readonly DateTime PaidOn = new DateTime(2026, 1, 15, 9, 0, 0, DateTimeKind.Utc);

    public OfferBuilder Status(OfferStatus value)
    {
        status = value;
        return this;
    }

    public OfferBuilder BookPrice(decimal value)
    {
        bookPrice = value;
        return this;
    }

    public OfferBuilder Id(int value)
    {
        id = value;
        return this;
    }

    public OfferBuilder CustomerId(int value)
    {
        customerId = value;
        return this;
    }
}
