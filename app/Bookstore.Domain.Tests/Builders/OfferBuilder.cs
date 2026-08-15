using Bookstore.Domain.Offers;

namespace Bookstore.Domain.Tests.Builders;

public class OfferBuilder
{
    private int id;
    private int customerId = 1;
    private string bookName = "test";
    private string author = "author";
    private string isbn = "12345678";
    private int bookTypeId = 2;
    private int conditionId = 3;
    private int genreId = 4;
    private int publisherId = 5;
    private decimal bookPrice = 5;
    private OfferStatus status = OfferStatus.PendingApproval;

    public Offer Build()
    {
        var offer = new Offer(customerId, bookName, author, isbn, bookTypeId, conditionId, genreId, publisherId, bookPrice)
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

        offer.RecordPayment();
    }

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
